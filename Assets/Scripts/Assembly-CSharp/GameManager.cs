using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public GameObject[] playerPrefabs;

	public Transform[] spawnPoints;

	public CameraPointer cameraPointer;

	public GameObject verticalScreenDivider;

	public GameObject threePlayerScreenDivider;

	public Camera[] playerCameras;

	public GameObject[] playerUIParents;

	public GameObject canvasGameObject;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private List<PlayerInput> players = new List<PlayerInput>();

	public List<PlayerStats> playerStats = new List<PlayerStats>();

	[SerializeField]
	private List<Camera> activeCams;

	public static GameManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		canvas = canvasGameObject.GetComponent<Canvas>();
	}

	private void Start()
	{
		Time.timeScale = 1f;
		InitializePlayers();
		List<Camera> list = new List<Camera>();
		foreach (PlayerInput player in players)
		{
			if (player != null)
			{
				Camera componentInChildren = player.GetComponentInChildren<Camera>();
				if (componentInChildren != null)
				{
					list.Add(componentInChildren);
				}
			}
		}
		Debug.Log($"[GameManager] Found {list.Count} active cameras. Arranging layout...");
		CameraLayoutManager.ArrangeCameras(list);
	}

	private void InitializePlayers()
	{
		List<PlayerData> selectedPlayers = PlayerDataHolder.SelectedPlayers;
		if (selectedPlayers == null || selectedPlayers.Count <= 0)
		{
			Debug.LogError("No player data found!");
			return;
		}
		players.Clear();
		playerStats.Clear();
		Debug.Log($"[GameManager] Initializing players. SelectedPlayers count = {selectedPlayers.Count}");
		foreach (PlayerData item in selectedPlayers)
		{
			AddNewPlayer(item);
		}
		List<PlayerStats> list = new List<PlayerStats>();
		foreach (PlayerInput player in players)
		{
			if (!(player == null))
			{
				PlayerStats component = player.GetComponent<PlayerStats>();
				if (component != null)
				{
					list.Add(component);
				}
			}
		}
		Debug.Log($"[GameManager] Collected statsList.Count = {list.Count} | players.Count = {players.Count}");
		if (UIManager.Instance != null)
		{
			UIManager.Instance.SetPlayerStats(list);
			UIManager.Instance.InitializeFromPlayerData(selectedPlayers);
			UIManager.Instance.SyncUIWithPlayers();
			UIManager.Instance.UpdateUI();
		}
		if (selectedPlayers == null || selectedPlayers.Count == 0)
		{
			Debug.LogWarning("No player data found, loading MainMenu...");
			SceneManager.LoadScene("MainMenu");
		}
	}

	private void AddNewPlayer(PlayerData playerData)
	{
		if (playerData.Device == null)
		{
			Debug.LogError($"Player {playerData.PlayerIndex} has a null device!");
			return;
		}
		Transform transform = spawnPoints[playerData.PlayerIndex % spawnPoints.Length];
		PlayerInput playerInput = PlayerInput.Instantiate(playerPrefabs[playerData.CharacterIndex], -1, null, -1, playerData.Device);
		playerInput.transform.SetPositionAndRotation(transform.position, transform.rotation);
		PlayerInput component = playerInput.GetComponent<PlayerInput>();
		component.SwitchCurrentControlScheme(playerData.Device);
		Debug.Log($"[GameManager] Added PlayerInput for slot {playerData.PlayerIndex}. players.Count now {players.Count}");
		PlayerController component2 = playerInput.GetComponent<PlayerController>();
		PlayerStats stats = component2.stats;
		stats.maxHP = playerData.Health;
		stats.maxAmmo = playerData.Ammo;
		component2.nutColor = playerData.NutColor;
		component2.stats.playerIndex = playerData.PlayerIndex;
		playerStats.Add(stats);
		players.Add(component);
		if (cameraPointer != null)
		{
			cameraPointer.AssignCamera(playerData.PlayerIndex, playerInput.transform);
			Debug.Log("Camera" + playerCameras[playerData.PlayerIndex]?.ToString() + " assigned to player " + playerData.PlayerIndex + ".");
		}
		else
		{
			Debug.LogError("CameraPointer is not assigned in the GameManager.");
		}
		Debug.Log($"Player {playerData.PlayerIndex} joined with device {playerData.Device}. Action Map: P{playerData.PlayerIndex + 1}");
	}

	private void SpawnPlayers()
	{
		List<PlayerData> selectedPlayers = PlayerDataHolder.SelectedPlayers;
		if (UIManager.Instance != null)
		{
			UIManager.Instance.InitializeFromPlayerData(PlayerDataHolder.SelectedPlayers);
		}
		for (int i = 0; i < selectedPlayers.Count; i++)
		{
			PlayerData playerData = selectedPlayers[i];
			Transform transform = spawnPoints[i % spawnPoints.Length];
			PlayerController component = Object.Instantiate(playerPrefabs[playerData.CharacterIndex], transform.position, transform.rotation).GetComponent<PlayerController>();
			component.Initialize(playerData);
			Debug.Log($"Spawned Player {playerData.PlayerIndex} with {playerData.Health} HP, {playerData.Ammo} ammo, and color {playerData.NutColor}");
			if (UIManager.Instance != null)
			{
				List<PlayerStats> list = new List<PlayerStats>();
				foreach (PlayerData item in selectedPlayers)
				{
					if (item != null)
					{
						PlayerStats stats = component.stats;
						if (stats != null)
						{
							list.Add(stats);
						}
					}
				}
				UIManager.Instance.SetPlayerStats(list);
			}
			else
			{
				Debug.LogError("[GameManager] UIManager.Instance is null. Ensure a UIManager exists in the gameplay scene!");
			}
			activeCams = new List<Camera>();
			foreach (PlayerData item2 in selectedPlayers)
			{
				if (item2 != null)
				{
					Camera component2 = activeCams[i].GetComponent<Camera>();
					if (component2 != null)
					{
						activeCams.Add(component2);
					}
				}
			}
			CameraLayoutManager.ArrangeCameras(activeCams);
		}
	}

	public void AdjustCamerasAndUI()
	{
		int count = players.Count;
		for (int i = 0; i < playerCameras.Length; i++)
		{
			if (playerCameras[i] != null)
			{
				playerCameras[i].gameObject.SetActive(value: false);
			}
			if (i < playerUIParents.Length && playerUIParents[i] != null)
			{
				playerUIParents[i].SetActive(value: false);
			}
		}
		int num = Mathf.Min(count, playerCameras.Length);
		for (int j = 0; j < num; j++)
		{
			if (playerCameras[j] != null)
			{
				playerCameras[j].gameObject.SetActive(value: true);
			}
			if (j < playerUIParents.Length && playerUIParents[j] != null)
			{
				playerUIParents[j].SetActive(value: true);
			}
		}
		if (count == 1)
		{
			if (verticalScreenDivider != null)
			{
				verticalScreenDivider.SetActive(value: false);
			}
			if (threePlayerScreenDivider != null)
			{
				threePlayerScreenDivider.SetActive(value: false);
			}
			if (playerCameras[0] != null)
			{
				playerCameras[0].rect = new Rect(0f, 0f, 1f, 1f);
			}
		}
		else if (count == 2)
		{
			if (verticalScreenDivider != null)
			{
				verticalScreenDivider.SetActive(value: false);
			}
			if (threePlayerScreenDivider != null)
			{
				threePlayerScreenDivider.SetActive(value: false);
			}
			playerCameras[0].rect = new Rect(0f, 0.5f, 1f, 0.5f);
			playerCameras[1].rect = new Rect(0f, 0f, 1f, 0.5f);
		}
		else if (count == 3)
		{
			if (verticalScreenDivider != null)
			{
				verticalScreenDivider.SetActive(value: false);
			}
			if (threePlayerScreenDivider != null)
			{
				threePlayerScreenDivider.SetActive(value: true);
			}
			playerCameras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
			playerCameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			playerCameras[2].rect = new Rect(0f, 0f, 1f, 0.5f);
		}
		else if (count == 4)
		{
			if (verticalScreenDivider != null)
			{
				verticalScreenDivider.SetActive(value: true);
			}
			if (threePlayerScreenDivider != null)
			{
				threePlayerScreenDivider.SetActive(value: false);
			}
			playerCameras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
			playerCameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			playerCameras[2].rect = new Rect(0f, 0f, 0.5f, 0.5f);
			playerCameras[3].rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
		}
		else if (count >= 5)
		{
			return;
		}
		canvas.gameObject.SetActive(value: true);
		Debug.Log("Adjusted cameras and UI for " + count + " players.");
	}

	private void OnPlayerJoined(PlayerInput playerInput)
	{
		if (players.Count >= PlayerDataHolder.PlayerLimit)
		{
			Debug.LogError("Player limit reached. Cannot add more players.");
			Object.Destroy(playerInput.gameObject);
		}
		else
		{
			players.Add(playerInput);
		}
	}

	public void Respawn(int playerIndex)
	{
		PlayerInput playerInput = null;
		foreach (PlayerInput player in players)
		{
			if (!(player == null))
			{
				PlayerStats component = player.GetComponent<PlayerStats>();
				if (component != null && component.playerIndex == playerIndex)
				{
					playerInput = player;
					break;
				}
			}
		}
		if (playerInput == null)
		{
			Debug.LogWarning($"[GameManager] Respawn: no PlayerInput found for playerIndex {playerIndex}");
			return;
		}
		Transform transform = spawnPoints[playerIndex % spawnPoints.Length];
		playerInput.transform.SetPositionAndRotation(transform.position, transform.rotation);
		PlayerStats component2 = playerInput.GetComponent<PlayerStats>();
		if (component2 != null)
		{
			component2.currentHP = component2.maxHP;
			component2.currentAmmo = component2.maxAmmo;
		}
		PlayerInput component3 = playerInput.GetComponent<PlayerInput>();
		if (component3 != null && component2 != null && component2.lives > 0)
		{
			component3.ActivateInput();
		}
		Debug.Log($"[GameManager] Respawned playerIndex {playerIndex} at spawn {transform.position}");
	}

	public void CheckGameOver()
	{
		if (this.playerStats == null || this.playerStats.Count == 0)
		{
			return;
		}
		int num = 0;
		PlayerStats playerStats = null;
		foreach (PlayerStats playerStat in this.playerStats)
		{
			if (!(playerStat == null) && playerStat.lives > 0)
			{
				num++;
				playerStats = playerStat;
			}
		}
		Debug.Log($"[GameManager] CheckGameOver: playersWithLives = {num}");
		if (num > 1)
		{
			return;
		}
		Debug.Log("[GameManager] Game Over triggered by CheckGameOver()");
		if (playerStats != null && UIManager.Instance != null)
		{
			UIManager.Instance.ShowWinner(playerStats);
		}
		if (UIManager.Instance != null)
		{
			SetupRestartButton();
			UIManager.Instance.restartButton.gameObject.SetActive(value: true);
			UIManager.Instance.restartButton.onClick.RemoveAllListeners();
			UIManager.Instance.restartButton.onClick.AddListener(delegate
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			});
		}
	}

	public void SetupRestartButton()
	{
		if (!(UIManager.Instance == null) && !(UIManager.Instance.restartButton == null))
		{
			UIManager.Instance.restartButton.onClick.RemoveAllListeners();
			UIManager.Instance.restartButton.onClick.AddListener(delegate
			{
				Debug.Log("[GameManager] Restart button clicked. Reloading scene...");
				RestartGame();
			});
		}
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
