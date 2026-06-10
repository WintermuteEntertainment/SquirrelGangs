using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stash : MonoBehaviour
{
	public static readonly List<Stash> All = new List<Stash>();

	public bool isFull;

	public int filledByPlayerIndex;

	public bool isArmed;

	public bool isColliding;

	[Header("Bomb")]
	[Tooltip("Damage dealt to each player in radius when detonated.")]
	public int stashExplosionDamage = 25;

	[Tooltip("Radius used to find players hit by the blast.")]
	public float explosionRadius = 2.5f;

	[Tooltip("Layer mask that contains the Player colliders.")]
	public LayerMask playerLayerMask;

	public float digDuration = 2f;

	public float hideDuration = 1.5f;

	public int hideAmmoAmount = 1;

	public int digAmmoAmount = 2;

	public int currentStashAmmo;

	public int maxAmmo = 15;

	public int defaultStashAmmo;

	public int growDuration = 10;

	private PlayerController pcTriggerEnterCollision2D;

	private PlayerController playerController;

	public PlayerController explodingPlayer;

	private PlayerStats playerStats;

	public Collision2D collisionHolder;

	public PlayerAudio audioScript;

	public SpriteRenderer spriteRenderer;

	public Color stashColor;

	public Color stashIsFullColour;

	public Color stashIsEmptyColour;

	public PlayerEffects playerEffects;

	public UIManager uIManager;

	public GameObject stashGrowFX;

	private Coroutine growRoutine;

	[SerializeField]
	private AudioSource dudSFX;

	[SerializeField]
	private AudioSource explosionSFX;

	[SerializeField]
	private ParticleSystem explosionFX;

	private void OnEnable()
	{
		if (!All.Contains(this))
		{
			All.Add(this);
		}
	}

	private void OnDisable()
	{
		All.Remove(this);
	}

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		stashColor = spriteRenderer.color;
		currentStashAmmo = defaultStashAmmo;
		uIManager = GetComponent<UIManager>();
		if (pcTriggerEnterCollision2D != null)
		{
			if (audioScript != null)
			{
				audioScript = pcTriggerEnterCollision2D.gameObject.GetComponent<PlayerAudio>();
			}
			if (playerController != null)
			{
				playerEffects = pcTriggerEnterCollision2D.gameObject.GetComponent<PlayerEffects>();
			}
		}
	}

	private void Update()
	{
		if (currentStashAmmo >= maxAmmo)
		{
			currentStashAmmo = maxAmmo;
			isFull = true;
			SetStashColour(stashIsFullColour);
		}
		else if (currentStashAmmo <= 0)
		{
			isFull = false;
			isArmed = false;
			filledByPlayerIndex = -1;
			SetStashColour(stashIsEmptyColour);
			if (growRoutine == null)
			{
				growRoutine = StartCoroutine(GrowAmmo(growDuration));
			}
		}
	}

	public bool TryDetonate(PlayerController requester)
	{
		if (!isFull || !isArmed)
		{
			return false;
		}
		if (requester == null || requester.stats == null)
		{
			return false;
		}
		if (requester.playerIndex != filledByPlayerIndex)
		{
			Debug.Log($"[Stash] Player {requester.playerIndex} tried to detonate {base.name}, not owner.");
			if (dudSFX != null)
			{
				dudSFX.Play();
			}
			return false;
		}
		Debug.Log($"[Stash] Player {requester.playerIndex} detonated {base.name}!");
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, explosionRadius, playerLayerMask);
		for (int i = 0; i < array.Length; i++)
		{
			PlayerController componentInParent = array[i].GetComponentInParent<PlayerController>();
			if (componentInParent != null)
			{
				componentInParent.TakeDamage(stashExplosionDamage);
				PlayerAudio component = componentInParent.GetComponent<PlayerAudio>();
				PlayerEffects component2 = componentInParent.GetComponent<PlayerEffects>();
				CameraShaker componentInChildren = componentInParent.GetComponentInChildren<CameraShaker>();
				if (component != null)
				{
					component.PlayBoomSound();
				}
				if (component2 != null)
				{
					component2.PlayBoomEffect();
				}
				if (componentInChildren != null)
				{
					componentInChildren.Shake(1.5f);
				}
				Debug.Log($"[Stash] Damaged Player {componentInParent.playerIndex}");
			}
		}
		isArmed = false;
		isFull = false;
		currentStashAmmo = 0;
		filledByPlayerIndex = -1;
		SetStashColour(stashColor);
		return true;
	}

	private void TriggerExplosionFX(PlayerController pc)
	{
		if (!(pc == null))
		{
			PlayerAudio component = pc.GetComponent<PlayerAudio>();
			PlayerEffects component2 = pc.GetComponent<PlayerEffects>();
			CameraShaker componentInChildren = pc.GetComponentInChildren<CameraShaker>();
			if (component != null)
			{
				component.PlayBoomSound();
			}
			if (component2 != null)
			{
				component2.PlayBoomEffect();
			}
			if (componentInChildren != null)
			{
				componentInChildren.Shake(1.5f);
			}
		}
	}

	private void PlayExplosionFXAtStash()
	{
		if (explosionSFX != null)
		{
			explosionSFX.Play();
		}
		if (explosionFX != null)
		{
			explosionFX.Play();
		}
		CameraShaker componentInChildren = Camera.main.GetComponentInChildren<CameraShaker>();
		if (componentInChildren != null)
		{
			componentInChildren.Shake(1.5f);
		}
		Debug.Log("Explosion FX triggered at stash position.");
	}

	public void SetStashColour(Color color)
	{
		spriteRenderer.color = color;
		Debug.Log($"[Stash] Color set to {color}");
	}

	public void FillStash(int playerIndex, Color nutColor)
	{
		if (currentStashAmmo < maxAmmo)
		{
			currentStashAmmo += hideAmmoAmount;
			if (currentStashAmmo >= maxAmmo)
			{
				currentStashAmmo = maxAmmo;
				isFull = true;
				isArmed = true;
				filledByPlayerIndex = playerIndex;
				PlayArmFX();
				SetStashColour(stashIsFullColour);
				Debug.Log($"[Stash] Armed by Player {playerIndex}");
			}
			else
			{
				SetStashColour(nutColor);
			}
			playerStats?.DropAmmo(hideAmmoAmount);
			UIManager.Instance?.UpdateUI();
		}
	}

	public void EmptyStash(int playerIndex)
	{
		if (currentStashAmmo > 0)
		{
			int num = Mathf.Min(digAmmoAmount, currentStashAmmo);
			currentStashAmmo -= num;
			if (currentStashAmmo < maxAmmo)
			{
				isArmed = false;
				filledByPlayerIndex = -1;
			}
			isFull = currentStashAmmo >= maxAmmo;
			playerController?.stats.AddAmmo(num);
			UIManager.Instance?.UpdateUI();
			if (currentStashAmmo <= 0)
			{
				StartCoroutine(GrowAmmo(growDuration));
			}
			Debug.Log($"Player {playerIndex} dug from {base.name}, ammo now {currentStashAmmo}");
		}
	}

	public IEnumerator HideAcornCoroutine(int playerIndex, Color nutColor, Action<bool> callback)
	{
		yield return new WaitForSeconds(hideDuration);
		if (!isFull)
		{
			FillStash(playerController.playerIndex, nutColor);
			callback(obj: true);
			string text = playerController.playerIndex.ToString();
			Color color = nutColor;
			Debug.Log("Player " + text + " filled stash, changed to colour: " + color.ToString());
		}
		else
		{
			callback(obj: false);
		}
		if (uIManager != null)
		{
			uIManager.UpdateUI();
		}
		Debug.Log("HideAcornCoroutine executed.");
	}

	public IEnumerator DigAcornCoroutine(int playerIndex, Action<bool> callback)
	{
		if (playerStats != null)
		{
			yield return new WaitForSeconds(digDuration);
			if (isColliding && currentStashAmmo >= 0)
			{
				callback(obj: true);
				Debug.Log("Player " + playerController.playerIndex + " emptied stash.");
				playerStats.currentAmmo += digAmmoAmount;
				Debug.Log("Added " + digAmmoAmount + " to current Player Ammo: " + playerStats.currentAmmo + ". This playerController.playerIndex is: " + playerController.playerIndex);
			}
		}
		else
		{
			callback(obj: false);
		}
		if (uIManager != null)
		{
			uIManager.UpdateUI();
		}
		Debug.Log("DigAcornCoroutine executed.");
	}

	private IEnumerator GrowAmmo(int growDuration)
	{
		while (currentStashAmmo < maxAmmo)
		{
			float seconds = (float)growDuration + (float)currentStashAmmo * 2f;
			yield return new WaitForSeconds(seconds);
			if (currentStashAmmo > 0)
			{
				currentStashAmmo++;
				Debug.Log($"[Stash] Grew to {currentStashAmmo}");
			}
		}
		growRoutine = null;
	}

	public void ExplodeNutBomb()
	{
		if (TryDetonate(explodingPlayer))
		{
			Debug.Log("[Stash] ExplodeNutBomb: stash " + base.name + " detonated by " + explodingPlayer?.name);
			PlayExplosionFXAtStash();
		}
		else
		{
			Debug.Log("[Stash] ExplodeNutBomb: stash " + base.name + " did NOT detonate.");
		}
		StartCoroutine(GrowAmmo(growDuration));
	}

	private void PlayArmFX()
	{
		PlayerAudio component = GetComponent<PlayerAudio>();
		PlayerEffects component2 = GetComponent<PlayerEffects>();
		if (component != null)
		{
			component.PlayArmSound();
		}
		if (component2 != null)
		{
			component2.PlayArmEffect();
		}
		Debug.Log("Arm FX triggered on this stash only.");
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, explosionRadius);
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerController component = collision.GetComponent<PlayerController>();
		if (component != null && component.CompareTag("Player"))
		{
			isColliding = true;
			pcTriggerEnterCollision2D = component;
			playerStats = component.GetComponent<PlayerStats>();
			playerController = component;
			Debug.Log($"[Stash] Player {component.playerIndex} entered trigger zone of {base.name}.");
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		PlayerController component = collision.GetComponent<PlayerController>();
		if (component != null && component.CompareTag("Player"))
		{
			isColliding = false;
			pcTriggerEnterCollision2D = null;
			playerController = null;
			playerStats = null;
			Debug.Log($"[Stash] Player {component.playerIndex} exited trigger zone of {base.name}.");
		}
	}
}
