using System.Collections;
using UnityEngine;

public class Nut : MonoBehaviour
{
	private Rigidbody2D rb;

	public bool isCollectible;

	private bool canCollide;

	private bool isHot;

	public float hotDuration = 1f;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private int nutDamage = 10;

	private int firingPlayerIndex;

	public Color nutColor;

	private PlayerController collidingPlayer;

	private PlayerController playerController;

	private PlayerStats playerStats;

	private UIManager uIManager;

	public PlayerUIBinding playerUIBindings;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		uIManager = Object.FindFirstObjectByType<UIManager>();
	}

	public void Initialize(bool collectible, int playerIndex, Color color)
	{
		isCollectible = collectible;
		firingPlayerIndex = playerIndex;
		_ = nutColor;
		nutColor = color;
		GetFiringPlayerIndex();
		Debug.Log("firingPlayerIndex is:" + firingPlayerIndex);
		if (collidingPlayer != null)
		{
			nutColor = collidingPlayer.nutColor;
		}
		ApplyColor(nutColor);
		if (!isCollectible)
		{
			StartCoroutine(EnableCollisionAfterDelay());
			StartCoroutine(EnableCollectibleAfterHotDuration());
		}
	}

	public Color ApplyColor(Color color)
	{
		spriteRenderer.color = color;
		return color;
	}

	private IEnumerator EnableCollisionAfterDelay()
	{
		yield return new WaitForSeconds(0.5f);
		canCollide = true;
	}

	private IEnumerator EnableCollectibleAfterHotDuration()
	{
		isHot = true;
		ApplyColor(nutColor);
		yield return new WaitForSeconds(hotDuration);
		isHot = false;
		isCollectible = true;
		ApplyColor(Color.blue);
	}

	public bool IsHot()
	{
		return isHot;
	}

	public int GetFiringPlayerIndex()
	{
		return firingPlayerIndex;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!isHot || !canCollide || !collision.gameObject.CompareTag("Player"))
		{
			return;
		}
		PlayerController component = collision.gameObject.GetComponent<PlayerController>();
		if (component != null && component.playerIndex != firingPlayerIndex)
		{
			component.TakeDamage(nutDamage);
			if (uIManager != null)
			{
				uIManager.UpdateUI();
			}
			CameraShaker componentInChildren = component.GetComponentInChildren<CameraShaker>();
			if (componentInChildren != null)
			{
				componentInChildren.Shake(0.5f);
			}
			UIManager.Instance.IncrementHitNuts(component.playerIndex);
			Debug.Log("Nut collided with player and caused damage.");
			Object.Destroy(base.gameObject);
		}
		else
		{
			Debug.Log("Nut collision ignored for damage with the player who fired it.");
		}
		if (uIManager != null)
		{
			playerUIBindings.UpdateUI(component.stats);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (isCollectible && other.gameObject.CompareTag("Player"))
		{
			if (uIManager != null)
			{
				uIManager.UpdateUI();
			}
			if (uIManager.TryGetComponent<PlayerController>(out var component))
			{
				collidingPlayer = component;
				component.AddAmmo(1, component.playerIndex);
				Debug.Log("Added 1 Ammo (Nut collected by player " + component?.ToString() + ").");
				Object.Destroy(base.gameObject);
			}
			if (uIManager != null)
			{
				uIManager.UpdateUI();
			}
		}
	}
}
