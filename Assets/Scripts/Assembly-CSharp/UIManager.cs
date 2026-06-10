using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI deathMessage;

	public Button restartButton;

	public GameObject deathScreenPanel;

	[SerializeField]
	private DeathPanelUI[] deathPanels = new DeathPanelUI[4];

	[SerializeField]
	private TMP_Text globalDeathMessage;

	[Header("Per-Player UI (assign in gameplay scene)")]
	[SerializeField]
	private GameObject[] playerUIParents = new GameObject[4];

	[SerializeField]
	private TMP_Text[] playerHPText = new TMP_Text[4];

	[SerializeField]
	private Slider[] playerHPSlider = new Slider[4];

	[SerializeField]
	private TMP_Text[] playerAmmoText = new TMP_Text[4];

	[SerializeField]
	private Slider[] playerAmmoSlider = new Slider[4];

	[Header("General UI (optional)")]
	[SerializeField]
	private TMP_Text nutsOnGroundText;

	[SerializeField]
	private TMP_Text hotNutsText;

	[SerializeField]
	private TMP_Text totalNutsText;

	[SerializeField]
	private TMP_Text instantiatedNutsText;

	[SerializeField]
	private TMP_Text remainingNutsText;

	[SerializeField]
	private TMP_Text hitNutsText;

	public TMP_Text winnerText;

	public PlayerStats[] playerStats = new PlayerStats[0];

	private int[] lastHP = new int[0];

	[SerializeField]
	private Image[] playerLivesIconsP1;

	[SerializeField]
	private Image[] playerLivesIconsP2;

	[SerializeField]
	private Image[] playerLivesIconsP3;

	[SerializeField]
	private Image[] playerLivesIconsP4;

	private int totalNuts;

	private int instantiatedNuts;

	private int hitNuts;

	private int[] playerHitNuts = new int[4];

	public static UIManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		Debug.Log("[UIManager] Awake in scene '" + base.gameObject.scene.name + "' on '" + base.name + "'.");
	}

	private void Start()
	{
		string arg = base.name;
		GameObject[] array = playerUIParents;
		Debug.Log($"[UIManager] Start() - instance: {arg}. playerUIParents count: {((array != null) ? array.Length : 0)}");
	}

	public void SetPlayerStats(List<PlayerInput> players)
	{
		List<PlayerStats> list = new List<PlayerStats>(players.Count);
		for (int i = 0; i < players.Count; i++)
		{
			PlayerStats playerStats = ((players[i] != null) ? players[i].GetComponent<PlayerStats>() : null);
			if (playerStats != null)
			{
				list.Add(playerStats);
			}
		}
		ApplyStatsList(list);
	}

	public void SetPlayerStats(List<PlayerStats> statsList)
	{
		ApplyStatsList(statsList);
	}

	public void SyncUIWithPlayers()
	{
		if (playerUIParents == null || playerUIParents.Length == 0)
		{
			Debug.LogWarning("[UIManager] SyncUIWithPlayers called but no playerUIParents assigned!");
			return;
		}
		for (int i = 0; i < playerUIParents.Length; i++)
		{
			if (playerUIParents[i] != null)
			{
				playerUIParents[i].SetActive(value: false);
			}
		}
		bool[] array = new bool[playerUIParents.Length];
		if (this.playerStats != null && this.playerStats.Length != 0)
		{
			Debug.Log($"[UIManager] Sync checking playerStats, count={this.playerStats.Length}");
			PlayerStats[] array2 = this.playerStats;
			foreach (PlayerStats playerStats in array2)
			{
				if (playerStats == null)
				{
					Debug.LogWarning("[UIManager] playerStats entry is null!");
					continue;
				}
				Debug.Log($"[UIManager] Found PlayerStats: index={playerStats.playerIndex}, HP={playerStats.currentHP}, Ammo={playerStats.currentAmmo}");
				int num = Mathf.Clamp(playerStats.playerIndex, 0, playerUIParents.Length - 1);
				array[num] = true;
			}
		}
		else if (PlayerDataHolder.SelectedPlayers != null && PlayerDataHolder.SelectedPlayers.Count > 0)
		{
			Debug.Log($"[UIManager] Sync fallback using PlayerDataHolder, count={PlayerDataHolder.SelectedPlayers.Count}");
			foreach (PlayerData selectedPlayer in PlayerDataHolder.SelectedPlayers)
			{
				Debug.Log($"[UIManager] Found PlayerData: PlayerIndex={selectedPlayer.PlayerIndex}, Health={selectedPlayer.Health}, Ammo={selectedPlayer.Ammo}");
				int num2 = Mathf.Clamp(selectedPlayer.PlayerIndex, 0, playerUIParents.Length - 1);
				array[num2] = true;
			}
		}
		else
		{
			Debug.LogWarning("[UIManager] No playerStats or PlayerDataHolder available!");
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (playerUIParents[k] != null)
			{
				playerUIParents[k].SetActive(array[k]);
				Debug.Log($"[UIManager] Slot {k} UI active={array[k]}");
			}
		}
	}

	private void ApplyStatsList(List<PlayerStats> statsList)
	{
		this.playerStats = statsList?.ToArray() ?? new PlayerStats[0];
		lastHP = new int[playerUIParents.Length];
		Debug.Log($"[UIManager] ApplyStatsList called. playerStats.Length = {this.playerStats.Length}");
		bool[] array = new bool[playerUIParents.Length];
		for (int i = 0; i < this.playerStats.Length; i++)
		{
			PlayerStats playerStats = this.playerStats[i];
			if (!(playerStats == null))
			{
				int num = Mathf.Clamp(playerStats.playerIndex, 0, playerUIParents.Length - 1);
				lastHP[num] = playerStats.currentHP;
				array[num] = true;
				UpdateOne(num, playerStats, force: true);
				Debug.Log($"[UIManager] Linking PlayerIndex {playerStats.playerIndex} -> UI slot {num}");
			}
		}
		for (int j = 0; j < playerUIParents.Length; j++)
		{
			if (playerUIParents[j] != null)
			{
				playerUIParents[j].SetActive(array[j]);
			}
		}
		UpdateUI();
	}

	public void ShowDeathPanel(int playerIndex, int livesRemaining)
	{
		if (playerIndex < 0 || playerIndex >= deathPanels.Length)
		{
			return;
		}
		DeathPanelUI deathPanelUI = deathPanels[playerIndex];
		if (deathPanelUI.panelRoot == null)
		{
			return;
		}
		deathPanelUI.panelRoot.SetActive(value: true);
		if (livesRemaining > 0)
		{
			deathPanelUI.respawnButton.gameObject.SetActive(value: true);
			deathPanelUI.gameOverText.gameObject.SetActive(value: false);
			if (deathPanelUI.deathMessage != null)
			{
				deathPanelUI.deathMessage.text = $"{deathPanelUI.gameObject.name} (Player {playerIndex + 1}) Died!";
			}
			deathPanelUI.respawnButton.onClick.RemoveAllListeners();
			deathPanelUI.respawnButton.onClick.AddListener(delegate
			{
				PlayerStats playerStats = Array.Find(this.playerStats, (PlayerStats s) => s != null && s.playerIndex == playerIndex);
				if (playerStats != null)
				{
					RespawnPlayer(playerStats);
					HideDeathPanel(playerIndex);
				}
			});
		}
		else
		{
			deathPanelUI.respawnButton.gameObject.SetActive(value: false);
			deathPanelUI.gameOverText.gameObject.SetActive(value: true);
			if (deathPanelUI.deathMessage != null)
			{
				deathPanelUI.deathMessage.text = "Game Over, Man!";
			}
		}
	}

	public void HideDeathPanel(int playerIndex)
	{
		if (playerIndex >= 0 && playerIndex < deathPanels.Length && deathPanels[playerIndex].panelRoot != null)
		{
			deathPanels[playerIndex].panelRoot.SetActive(value: false);
		}
	}

	public void FlashGlobalDeathMessage(string message)
	{
		if (!(globalDeathMessage == null))
		{
			StartCoroutine(FlashDeathMessageRoutine(message));
		}
	}

	private IEnumerator FlashDeathMessageRoutine(string message)
	{
		globalDeathMessage.text = message;
		globalDeathMessage.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(1.5f);
		globalDeathMessage.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		UpdateUI();
	}

	public void UpdateUI()
	{
		if (this.playerStats == null || this.playerStats.Length == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Acorn");
		num = array.Length;
		for (int i = 0; i < array.Length; i++)
		{
			Nut component = array[i].GetComponent<Nut>();
			if (component != null && component.IsHot())
			{
				num2++;
			}
		}
		if ((bool)nutsOnGroundText)
		{
			nutsOnGroundText.text = "Nuts on Ground: " + num;
		}
		if ((bool)hotNutsText)
		{
			hotNutsText.text = "Hot Nuts: " + num2;
		}
		if ((bool)totalNutsText)
		{
			totalNutsText.text = "Total Nuts: " + totalNuts;
		}
		if ((bool)instantiatedNutsText)
		{
			instantiatedNutsText.text = "Instantiated Nuts: " + instantiatedNuts;
		}
		if ((bool)remainingNutsText)
		{
			remainingNutsText.text = "Remaining Nuts: " + GetRemainingNuts();
		}
		if ((bool)hitNutsText)
		{
			hitNutsText.text = $"Hit Nuts: {hitNuts} (P1: {playerHitNuts[0]}, P2: {playerHitNuts[1]}, P3: {playerHitNuts[2]}, P4: {playerHitNuts[3]})";
		}
		for (int j = 0; j < this.playerStats.Length; j++)
		{
			PlayerStats playerStats = this.playerStats[j];
			if (!(playerStats == null))
			{
				int num3 = Mathf.Clamp(playerStats.playerIndex, 0, playerUIParents.Length - 1);
				if (playerStats.currentHP < ((j < lastHP.Length) ? lastHP[j] : playerStats.currentHP))
				{
					StartCoroutine(WiggleSlider((playerHPSlider != null && num3 < playerHPSlider.Length) ? playerHPSlider[num3] : null));
				}
				if (j < lastHP.Length)
				{
					lastHP[j] = playerStats.currentHP;
				}
				UpdateOne(num3, playerStats, force: false);
			}
		}
		for (int k = 0; k < this.playerStats.Length; k++)
		{
			PlayerStats playerStats2 = this.playerStats[k];
			if (!(playerStats2 == null))
			{
				int slot = Mathf.Clamp(playerStats2.playerIndex, 0, 3);
				UpdateLivesUI(slot, playerStats2.lives);
			}
		}
	}

	private void UpdateLivesUI(int slot, int livesRemaining)
	{
		if (slot < 0 || slot >= playerUIParents.Length)
		{
			return;
		}
		Image[] array = null;
		switch (slot)
		{
		case 0:
			array = playerLivesIconsP1;
			break;
		case 1:
			array = playerLivesIconsP2;
			break;
		case 2:
			array = playerLivesIconsP3;
			break;
		case 3:
			array = playerLivesIconsP4;
			break;
		}
		if (array == null)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				array[i].enabled = i < livesRemaining;
			}
		}
		Debug.Log($"Updating lives UI for Player {slot}: {livesRemaining} lives");
	}

	public void InitializeFromPlayerData(List<PlayerData> selectedPlayers)
	{
		foreach (PlayerData data in selectedPlayers)
		{
			PlayerStats playerStats = Array.Find(this.playerStats, (PlayerStats ps) => ps != null && ps.playerIndex == data.PlayerIndex);
			if (playerStats == null)
			{
				Debug.LogWarning($"[UIManager] No PlayerStats found for PlayerIndex {data.PlayerIndex}");
				continue;
			}
			int playerIndex = playerStats.playerIndex;
			Debug.Log($"[UIManager] Initializing UI for PlayerIndex {data.PlayerIndex} → slot {playerIndex} (Prefab {data.CharacterIndex})");
			if (playerIndex < playerHPSlider.Length && playerHPSlider[playerIndex] != null)
			{
				Slider obj = playerHPSlider[playerIndex];
				obj.minValue = 0f;
				obj.maxValue = data.Health;
				obj.value = data.Health;
			}
			if (playerIndex < playerAmmoSlider.Length && playerAmmoSlider[playerIndex] != null)
			{
				Slider obj2 = playerAmmoSlider[playerIndex];
				obj2.minValue = 0f;
				obj2.maxValue = data.Ammo;
				obj2.value = data.Ammo;
			}
		}
	}

	private void UpdateOne(int idx, PlayerStats stats, bool force)
	{
		if (playerHPText != null && idx < playerHPText.Length && playerHPText[idx] != null)
		{
			playerHPText[idx].text = $"HP: {stats.currentHP}/{stats.maxHP}";
		}
		if (playerHPSlider != null && idx < playerHPSlider.Length && playerHPSlider[idx] != null)
		{
			Slider slider = playerHPSlider[idx];
			if (force)
			{
				slider.minValue = 0f;
				slider.maxValue = stats.maxHP;
			}
			slider.value = stats.currentHP;
		}
		if (playerAmmoText != null && idx < playerAmmoText.Length && playerAmmoText[idx] != null)
		{
			playerAmmoText[idx].text = $"Ammo: {stats.currentAmmo}/{stats.maxAmmo}";
		}
		if (playerAmmoSlider != null && idx < playerAmmoSlider.Length && playerAmmoSlider[idx] != null)
		{
			Slider slider2 = playerAmmoSlider[idx];
			if (force)
			{
				slider2.minValue = 0f;
				slider2.maxValue = stats.maxAmmo;
			}
			slider2.value = stats.currentAmmo;
		}
	}

	private IEnumerator WiggleSlider(Slider slider)
	{
		if ((bool)slider)
		{
			Vector3 baseScale = slider.transform.localScale;
			float t = 0f;
			while (t < 0.2f)
			{
				t += Time.deltaTime;
				float num = 1f + Mathf.Sin(t * MathF.PI * 6f) * 0.1f;
				slider.transform.localScale = baseScale * num;
				yield return null;
			}
			slider.transform.localScale = baseScale;
		}
	}

	private int GetRemainingNuts()
	{
		int num = 0;
		for (int i = 0; i < playerStats.Length; i++)
		{
			if (playerStats[i] != null)
			{
				num += playerStats[i].currentAmmo;
			}
		}
		return num;
	}

	public void IncrementInstantiatedNuts()
	{
		instantiatedNuts++;
	}

	public void IncrementHitNuts(int playerIndex)
	{
		hitNuts++;
		if (playerIndex >= 0 && playerIndex < playerHitNuts.Length)
		{
			playerHitNuts[playerIndex]++;
		}
	}

	public void ShowWinner(PlayerStats winner)
	{
		if (!winnerText)
		{
			Debug.LogError("WinnerText UI element is not assigned!");
			return;
		}
		string text = $"Player {winner.playerIndex + 1}";
		winnerText.text = text + " Wins!";
		winnerText.gameObject.SetActive(value: true);
		StartCoroutine(AnimateWinnerText());
	}

	private IEnumerator AnimateWinnerText()
	{
		CanvasGroup cg = winnerText.GetComponent<CanvasGroup>();
		if (cg == null)
		{
			cg = winnerText.gameObject.AddComponent<CanvasGroup>();
		}
		cg.alpha = 0f;
		winnerText.transform.localScale = Vector3.one * 0.8f;
		float duration = 1.2f;
		float t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			float num = t / duration;
			cg.alpha = Mathf.SmoothStep(0f, 1f, num);
			float num2 = 0.8f + Mathf.Sin(num * MathF.PI) * 0.3f;
			winnerText.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		cg.alpha = 1f;
		winnerText.transform.localScale = Vector3.one;
	}

	private void RespawnPlayer(PlayerStats ps)
	{
		ps.currentHP = ps.maxHP;
		ps.currentAmmo = ps.maxAmmo;
		GameManager.Instance.Respawn(ps.playerIndex);
		Debug.Log($"[UIManager] Respawned Player {ps.playerIndex} with {ps.lives} lives remaining.");
		HideDeathPanel(ps.playerIndex);
		ps.UnfreezePlayer();
	}
}
