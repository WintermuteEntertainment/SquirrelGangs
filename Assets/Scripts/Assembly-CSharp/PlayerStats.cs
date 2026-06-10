using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
	public int maxHP = 100;

	public int maxAmmo = 3;

	public int currentHP;

	public int currentAmmo;

	public int bankedNuts;

	public int deaths;

	public int playerIndex;

	public int lives;

	[SerializeField]
	private PlayerController playerController;

	[SerializeField]
	private GameObject playerObject;

	[SerializeField]
	private bool isFrozen;

	public event Action<PlayerStats> OnDeath;

	private void Start()
	{
		maxHP = PlayerPrefs.GetInt((playerIndex == 0) ? "Player1Health" : "Player2Health", maxHP);
		maxAmmo = PlayerPrefs.GetInt((playerIndex == 0) ? "Player1Ammo" : "Player2Ammo", maxAmmo);
		currentHP = maxHP;
		currentAmmo = maxAmmo;
		bankedNuts = 0;
		deaths = 0;
		if (lives <= 0)
		{
			lives = 3;
		}
		Debug.Log($"Player {playerIndex} starting lives = {lives}");
		if (playerObject == null)
		{
			Debug.LogError("Player object is not assigned in PlayerStats!");
		}
		else
		{
			playerController = playerObject.GetComponent<PlayerController>();
			if (playerController == null)
			{
				Debug.LogError("PlayerController component is missing on playerObject in PlayerStats!");
			}
		}
		Debug.Log($"PlayerStats initialized. PlayerIndex: {playerIndex}, MaxHP: {maxHP}, MaxAmmo: {maxAmmo}");
		Debug.Log($"PlayerIndex: {playerIndex}, CurrentHP: {currentHP}, CurrentAmmo: {currentAmmo}");
	}

	private void Update()
	{
	}

	public void FreezePlayer()
	{
		isFrozen = true;
		GetComponent<Collider2D>().enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;
		playerController.enabled = false;
	}

	public void UnfreezePlayer()
	{
		isFrozen = false;
		GetComponent<Collider2D>().enabled = true;
		GetComponent<SpriteRenderer>().enabled = true;
		playerController.enabled = true;
	}

	public void TakeDamage(int damage)
	{
		if (isFrozen)
		{
			return;
		}
		currentHP -= damage;
		if (currentHP > 0)
		{
			return;
		}
		currentHP = 0;
		lives--;
		this.OnDeath?.Invoke(this);
		if (UIManager.Instance != null)
		{
			FreezePlayer();
		}
		UIManager.Instance.ShowDeathPanel(playerIndex, lives);
		if (UIManager.Instance != null)
		{
			UIManager.Instance.FlashGlobalDeathMessage($"{base.gameObject.name}(Player {playerIndex + 1}) Died!");
		}
		if (lives <= 0)
		{
			PlayerInput component = GetComponent<PlayerInput>();
			if (component != null)
			{
				component.DeactivateInput();
			}
			Debug.Log($"[PlayerStats] Player {playerIndex} eliminated (no lives left).");
		}
		else
		{
			Debug.Log($"[PlayerStats] Player {playerIndex} died, {lives} lives remaining.");
		}
		deaths++;
		currentHP = maxHP;
		UIManager.Instance?.UpdateUI();
		GameManager.Instance?.CheckGameOver();
	}

	public void AddAmmo(int ammo)
	{
		currentAmmo = Mathf.Clamp(currentAmmo + ammo, 0, maxAmmo);
		UIManager.Instance.UpdateUI();
		Debug.Log($"Player {playerIndex} added ammo. Current Ammo: {currentAmmo}");
	}

	public void DropAmmo(int ammo)
	{
		currentAmmo = Mathf.Clamp(currentAmmo - ammo, 0, maxAmmo);
		Debug.Log($"Player {playerIndex} dropped {ammo} ammo. Current Ammo: {currentAmmo}");
		UIManager.Instance.UpdateUI();
	}

	public void BankNut()
	{
		bankedNuts++;
	}

	public void ResetStats()
	{
		currentHP = maxHP;
		currentAmmo = maxAmmo;
		bankedNuts = 0;
	}

	public void Die()
	{
		deaths++;
		currentHP = maxHP;
		UIManager.Instance?.FlashGlobalDeathMessage($"Player {playerIndex + 1} Died!");
		GameManager.Instance?.CheckGameOver();
	}
}
