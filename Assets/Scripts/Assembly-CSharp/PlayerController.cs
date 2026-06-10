using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	[Serializable]
	public class PlayerSlotUI
	{
		public Toggle toggle;

		public TMP_Dropdown characterDropdown;

		public TMP_Dropdown deviceDropdown;

		public TMP_Dropdown nutColorDropdown;

		public Slider healthSlider;

		public Slider ammoSlider;

		public ColorPicker colorPicker;
	}

	public Color nutColor;

	[SerializeField]
	private float speed = 5f;

	public float sprintMultiplier = 1.5f;

	public float jumpForce = 15f;

	public float fallMultiplier = 3.5f;

	public float sprintDuration = 3f;

	public int sprintCost = 1;

	public bool isGrounded;

	public bool isJumping;

	public bool isSprinting;

	[SerializeField]
	private bool isFacingRight;

	[SerializeField]
	private int currentHealth;

	[SerializeField]
	private int currentAmmo;

	[SerializeField]
	private bool canPickupNuts;

	public bool canExplode;

	public Transform groundCheck;

	public LayerMask groundLayer;

	public Transform[] throwPoints;

	[SerializeField]
	private float groundCheckRadius = 0.2f;

	[SerializeField]
	private float throwCharge;

	[SerializeField]
	private float throwChargeMax;

	[SerializeField]
	private float defaultThrowChargeMax;

	[SerializeField]
	private float pickupCooldown = 2.5f;

	[SerializeField]
	private bool canHideAcorns = true;

	[SerializeField]
	private bool canDigAcorns = true;

	[SerializeField]
	private int explosion;

	public bool explodeTriggered;

	public bool exploded = true;

	[SerializeField]
	private int stashExplosion;

	[SerializeField]
	private bool isThrowing;

	[SerializeField]
	private bool isHidingAcorn;

	[SerializeField]
	private bool isDigging;

	public int playerIndex;

	[SerializeField]
	private Transform throwPointP1;

	[SerializeField]
	private Transform throwPointP2;

	[SerializeField]
	private Transform throwPointP3;

	[SerializeField]
	private Transform throwPointP4;

	[SerializeField]
	private GameObject acornPrefab;

	[SerializeField]
	private float throwForce = 500f;

	[SerializeField]
	private Transform throwPointToUse;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GameObject colourPickerGO;

	[SerializeField]
	private Transform stashCheck;

	[SerializeField]
	private LayerMask stashLayer;

	public Rigidbody2D rb;

	public PlayerStats stats;

	[SerializeField]
	private PlayerAudio playerAudio;

	[SerializeField]
	private PlayerEffects playerEffects;

	[SerializeField]
	private PlayerInput playerInput;

	[SerializeField]
	private InputAction moveAction;

	[SerializeField]
	private InputAction jumpAction;

	[SerializeField]
	private InputAction sprintAction;

	[SerializeField]
	private InputAction fireAction;

	[SerializeField]
	private InputAction hideAction;

	[SerializeField]
	private InputAction digAction;

	[SerializeField]
	private InputAction explodeAction;

	private List<GameObject> instantiatedAcorns = new List<GameObject>();

	[SerializeField]
	private GameObject throwEffectP1;

	[SerializeField]
	private GameObject throwEffectP2;

	[SerializeField]
	private GameObject throwEffectP3;

	[SerializeField]
	private GameObject throwEffectP4;

	[SerializeField]
	private GameObject landingEffectP1;

	[SerializeField]
	private GameObject landingEffectP2;

	[SerializeField]
	private Sprite[] characterSprites;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private PlayerAnimationController animationController;

	[SerializeField]
	private PlayerData playerData;

	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private UIManager uIManager = UIManager.Instance;

	[SerializeField]
	private Stash stashScript;

	private void Awake()
	{
		throwChargeMax = defaultThrowChargeMax;
		isFacingRight = true;
		canPickupNuts = true;
		rb = GetComponent<Rigidbody2D>();
		stats = GetComponent<PlayerStats>();
		playerAudio = GetComponent<PlayerAudio>();
		playerEffects = GetComponent<PlayerEffects>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animationController = GetComponent<PlayerAnimationController>();
		virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
		playerInput = GetComponent<PlayerInput>();
	}

	private void InitializePlayer(int playerIndex, int characterIndex, Color nutColor)
	{
		if (playerIndex >= 0 && playerIndex < throwPoints.Length)
		{
			throwPointToUse = throwPoints[playerIndex];
		}
		else
		{
			Debug.LogWarning($"No throw point defined for player {playerIndex}");
		}
		playerInput.enabled = true;
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = characterSprites[characterIndex];
		}
		playerEffects.playerIndex = playerIndex;
		InputActionAsset actions = playerInput.actions;
		playerInput.SwitchCurrentActionMap($"P{playerIndex + 1}");
		moveAction = actions.FindAction("Move");
		jumpAction = actions.FindAction("Jump");
		sprintAction = actions.FindAction("Sprint");
		fireAction = actions.FindAction("Fire");
		hideAction = actions.FindAction("Hide");
		digAction = actions.FindAction("Dig");
		explodeAction = actions.FindAction("Explode");
		MonoBehaviour.print("Initialized Player with index " + playerIndex + " at throwPoint: " + throwPointToUse);
		if (virtualCamera != null)
		{
			virtualCamera.Follow = base.transform;
		}
	}

	public void Initialize(PlayerData data)
	{
		playerData = data;
		playerIndex = data.PlayerIndex;
		nutColor = data.NutColor;
		currentHealth = data.Health;
		currentAmmo = data.Ammo;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.material.name.Contains("Nut"))
			{
				renderer.material.color = nutColor;
			}
		}
		Debug.Log($"PlayerController.Initialize called for Player {data.PlayerIndex} with prefab {data.CharacterIndex}");
	}

	private void OnEnable()
	{
		if (playerInput != null)
		{
			playerInput.onActionTriggered += OnActionTriggered;
			if (moveAction != null)
			{
				moveAction.Enable();
			}
			if (jumpAction != null)
			{
				jumpAction.Enable();
			}
			if (sprintAction != null)
			{
				sprintAction.Enable();
			}
			if (fireAction != null)
			{
				fireAction.Enable();
			}
			if (hideAction != null)
			{
				hideAction.Enable();
			}
			if (digAction != null)
			{
				digAction.Enable();
			}
		}
		else
		{
			Debug.LogError("PlayerInput is null in OnEnable.");
		}
	}

	private void OnDisable()
	{
		if (playerInput != null)
		{
			playerInput.onActionTriggered -= OnActionTriggered;
			if (moveAction != null)
			{
				moveAction.Disable();
			}
			if (jumpAction != null)
			{
				jumpAction.Disable();
			}
			if (sprintAction != null)
			{
				sprintAction.Disable();
			}
			if (fireAction != null)
			{
				fireAction.Disable();
			}
			if (hideAction != null)
			{
				hideAction.Disable();
			}
			if (digAction != null)
			{
				digAction.Disable();
			}
		}
	}

	private void Update()
	{
		UpdateAnimations();
		if (!explodeTriggered)
		{
			return;
		}
		bool flag = false;
		foreach (Stash item in Stash.All)
		{
			flag |= item.TryDetonate(this);
		}
		if (flag)
		{
			Debug.Log($"Player {playerIndex} successfully detonated a stash.");
		}
		else
		{
			Debug.Log($"Player {playerIndex} tried to detonate, but no stash exploded.");
		}
		explodeTriggered = false;
	}

	private void UpdateAnimations()
	{
		if (!(animationController == null))
		{
			animationController.ResetAnimations();
			animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
			animator.SetBool("isJumping", !isGrounded);
			animator.SetBool("isSprinting", isSprinting);
			animator.SetBool("isWalking", Mathf.Abs(rb.velocity.x) > 0.1f);
			UpdateAnimationState();
		}
	}

	private void UpdateAnimationState()
	{
		if (isSprinting)
		{
			animationController.SetState(PlayerAnimationController.AnimationState.Sprinting);
		}
		else if (!isGrounded)
		{
			animationController.SetState(PlayerAnimationController.AnimationState.Jumping);
		}
		else if (Mathf.Abs(rb.velocity.x) > 0.1f)
		{
			animationController.SetState(PlayerAnimationController.AnimationState.Walking);
		}
		else
		{
			animationController.SetState(PlayerAnimationController.AnimationState.Idle);
		}
		if (isThrowing)
		{
			animator.SetTrigger("ThrowAcorn");
		}
		if (isHidingAcorn)
		{
			animator.SetTrigger("HideAcorn");
		}
		if (isDigging)
		{
			animator.SetBool("isDigging", value: true);
		}
	}

	private void OnActionTriggered(InputAction.CallbackContext context)
	{
		if (context.action == moveAction)
		{
			OnMove(context);
		}
		else if (context.action == jumpAction)
		{
			OnJump(context);
		}
		else if (context.action == sprintAction)
		{
			OnSprint(context);
		}
		else if (context.action == fireAction)
		{
			if (context.phase == InputActionPhase.Started)
			{
				OnFireCharge(context);
			}
			else if (context.phase == InputActionPhase.Performed)
			{
				OnFireCharge(context);
			}
			else if (context.phase == InputActionPhase.Canceled)
			{
				OnFireRelease(context);
			}
		}
		else if (context.action == hideAction)
		{
			if (context.performed && canHideAcorns)
			{
				OnHide(context);
			}
		}
		else if (context.action == digAction && context.performed && canDigAcorns)
		{
			StartCoroutine(DigAcorn());
		}
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		if (rb == null)
		{
			return;
		}
		float x = context.ReadValue<Vector2>().x;
		float num = (isSprinting ? (speed * sprintMultiplier) : speed);
		rb.velocity = new Vector2(x * num, rb.velocity.y);
		animator.SetFloat("Speed", Mathf.Abs(x));
		if (Mathf.Abs(x) > 0.1f)
		{
			animator.SetBool("isWalking", value: true);
			if ((x > 0f && !isFacingRight) || (x < 0f && isFacingRight))
			{
				Flip();
			}
		}
		else
		{
			animator.SetBool("isWalking", value: false);
		}
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		if (!(stats == null) && stats.currentAmmo >= sprintCost && !isSprinting)
		{
			isSprinting = true;
			stats.currentAmmo -= sprintCost;
			StartCoroutine(SprintCoroutine());
		}
	}

	private IEnumerator SprintCoroutine()
	{
		yield return new WaitForSeconds(sprintDuration);
		isSprinting = false;
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (!(rb == null))
		{
			isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
			if (isGrounded)
			{
				rb.velocity = new Vector2(rb.velocity.x, jumpForce);
				isJumping = true;
				animator.SetBool("isJumping", value: true);
				playerEffects.PlayLandingEffect();
			}
		}
	}

	public void OnFireCharge(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Started)
		{
			throwCharge = 0f;
		}
		else if (context.phase == InputActionPhase.Performed)
		{
			throwCharge += Time.deltaTime;
			throwCharge = Mathf.Clamp(throwCharge, 0f, throwChargeMax);
			Debug.Log($"Player {playerIndex} charging throw: {throwCharge}");
		}
	}

	public void OnFireRelease(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Canceled)
		{
			ThrowAcorn();
			throwCharge = 0f;
		}
	}

	public void OnHide(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Performed)
		{
			StartCoroutine(HideAcorn(nutColor));
		}
	}

	public void OnDig(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Performed)
		{
			StartCoroutine(DigAcorn());
		}
	}

	public void OnExplode(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Performed)
		{
			return;
		}
		foreach (Stash item in Stash.All)
		{
			item.TryDetonate(this);
		}
	}

	public void ThrowAcorn()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"Player {playerIndex} cannot throw acorn because the game object is inactive!");
		}
		else if (stats == null)
		{
			Debug.LogError("PlayerStats is not assigned!");
		}
		else if (stats.currentAmmo > 0)
		{
			if (throwPointToUse != null)
			{
				Debug.Log($"Player {playerIndex} Instantiating acornPrefab at: {throwPointToUse.position}");
				playerEffects.PlayThrowEffect();
				playerAudio.PlayThrowSound();
				GameObject gameObject = UnityEngine.Object.Instantiate(acornPrefab, throwPointToUse.position, throwPointToUse.rotation);
				instantiatedAcorns.Add(gameObject);
				Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
				float num = throwCharge * throwForce;
				Debug.Log($"Player {playerIndex} throwCharge: {throwCharge}, throwForce: {throwForce}, appliedThrowForce: {num}");
				component.velocity += new Vector2(num * (float)(isFacingRight ? 1 : (-1)), 0f);
				gameObject.GetComponent<Nut>().Initialize(collectible: false, playerIndex, nutColor);
				if (stats != null)
				{
					stats.currentAmmo--;
				}
				animator.SetTrigger("ThrowAcorn");
				Debug.Log($"Player {playerIndex} Threw Acorn.");
				if (uIManager != null)
				{
					uIManager.IncrementInstantiatedNuts();
				}
				StartCoroutine(PickupCooldown());
			}
			else
			{
				Debug.LogError($"Player {playerIndex} throw point not found!");
			}
		}
		else
		{
			Debug.Log($"Player {playerIndex} No ammo to throw.");
		}
	}

	private void OnDestroy()
	{
		foreach (GameObject instantiatedAcorn in instantiatedAcorns)
		{
			if (instantiatedAcorn != null)
			{
				UnityEngine.Object.Destroy(instantiatedAcorn);
			}
		}
		instantiatedAcorns.Clear();
	}

	private IEnumerator PickupCooldown()
	{
		canPickupNuts = false;
		yield return new WaitForSeconds(pickupCooldown);
		canPickupNuts = true;
	}

	public void DropAmmo(int ammo)
	{
		if (stats.currentAmmo > 0)
		{
			stashScript.currentStashAmmo += ammo;
			stats.DropAmmo(ammo);
			stashScript = null;
		}
		if (uIManager != null)
		{
			uIManager.UpdateUI();
		}
		Debug.Log($"Player {playerIndex} dropped {ammo} ammo. Current Ammo: {stats.currentAmmo}");
	}

	public void AddAmmo(int ammo, int playerIndex)
	{
		if (!(stats == null))
		{
			stats.AddAmmo(ammo);
			if (uIManager != null)
			{
				uIManager.UpdateUI();
			}
			Debug.Log($"Player {playerIndex} added {ammo} ammo. Current Player Ammo: {stats.currentAmmo}.");
		}
	}

	public IEnumerator StashExplosion()
	{
		if (!canExplode)
		{
			Debug.Log($"Player {playerIndex} tried to explode but can't.");
			yield break;
		}
		explodeTriggered = true;
		Debug.Log($"Player {playerIndex} triggered stash explosion.");
		yield return null;
	}

	public IEnumerator HideAcorn(Color nutColor)
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(stashCheck.position, 0.5f, stashLayer);
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			Debug.Log("Stash Collider2D " + collider2D?.ToString() + " found in stashes " + array?.ToString() + " array. [INSIDE HIDE ACORN METHOD]");
			stashScript = collider2D.GetComponent<Stash>();
			if (!(stashScript != null))
			{
				continue;
			}
			yield return StartCoroutine(stashScript.HideAcornCoroutine(playerIndex, nutColor, delegate(bool success)
			{
				if (success && stashScript != null && stats != null)
				{
					if (stashScript.isFull)
					{
						Debug.Log("Stash is full.");
						stashScript.SetStashColour(stashScript.stashIsFullColour);
					}
					else
					{
						if (stashScript != null)
						{
							stats.DropAmmo(stashScript.hideAmmoAmount);
							stashScript.currentStashAmmo += stashScript.hideAmmoAmount;
							playerEffects.PlayHideEffect();
							Debug.Log($"Player {playerIndex} hid {stashScript.hideAmmoAmount} ammo to stash.");
						}
						if (uIManager != null)
						{
							uIManager.UpdateUI();
						}
					}
					Debug.Log($" Current Player Ammo: {stats.currentAmmo}, current stash ammo {stashScript.currentStashAmmo}," + $" current Player nutColor {nutColor}, current stash colour: {stashScript.spriteRenderer.color}," + $" current stashIsFullColor: {stashScript.stashIsFullColour}.");
				}
				else
				{
					Debug.Log("Stats " + stats?.ToString() + " is null in HideAcorn Coroutine in PlayerController or else Stash is already Full.");
				}
			}));
			break;
		}
	}

	public IEnumerator DigAcorn()
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(stashCheck.position, 0.5f, stashLayer);
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			Debug.Log("Stash Collider2D " + collider2D?.ToString() + " found in stashes " + array?.ToString() + " array.");
			stashScript = collider2D.GetComponent<Stash>();
			Debug.Log("Stash script is: " + stashScript.gameObject.name);
			if (stashScript != null)
			{
				yield return StartCoroutine(stashScript.DigAcornCoroutine(playerIndex, delegate(bool success)
				{
					if (success)
					{
						stashScript.isFull = false;
						stashScript.EmptyStash(playerIndex);
						playerEffects.PlayDigEffect();
						Debug.Log($"Player {playerIndex} dug {stashScript.digAmmoAmount} acorns, stash last filled by {stashScript.filledByPlayerIndex}, (this debug message from PlayerController: {base.name}.");
					}
					if (uIManager != null)
					{
						uIManager.UpdateUI();
					}
				}));
				break;
			}
			Debug.Log("Stash script" + stashScript?.ToString() + " is null.");
		}
	}

	public Color SetNutColor(Color color)
	{
		nutColor = color;
		return nutColor;
	}

	private IEnumerator ResetHidingAcornFlag()
	{
		yield return new WaitForSeconds(0.5f);
		isHidingAcorn = false;
	}

	private void Flip()
	{
		isFacingRight = !isFacingRight;
		Vector3 localScale = base.transform.localScale;
		localScale.x *= -1f;
		base.transform.localScale = localScale;
		if (throwPointToUse != null)
		{
			throwPointToUse.localPosition = new Vector3(isFacingRight ? Mathf.Abs(throwPointToUse.localPosition.x) : Mathf.Abs(0f - throwPointToUse.localPosition.x), throwPointToUse.localPosition.y, throwPointToUse.localPosition.z);
		}
	}

	public void TakeDamage(int damage)
	{
		if (!(stats == null))
		{
			stats.TakeDamage(damage);
			if (uIManager != null)
			{
				uIManager.UpdateUI();
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Acorn"))
		{
			Nut component = other.gameObject.GetComponent<Nut>();
			if (component != null && component.isCollectible && canPickupNuts)
			{
				AddAmmo(1, playerIndex);
				UnityEngine.Object.Destroy(other.gameObject);
				playerAudio.PlayPickupSound();
				Debug.Log("Player picked up a nut.");
			}
			else if (component != null && !component.isCollectible && component.GetFiringPlayerIndex() != playerIndex)
			{
				TakeDamage(10);
				UnityEngine.Object.Destroy(other.gameObject);
				playerAudio.PlayDamageSound();
			}
		}
		if (other.gameObject.CompareTag("Stash"))
		{
			stashScript = other.GetComponent<Stash>();
			stashScript.explodingPlayer = this;
			canExplode = true;
			Debug.Log($"Player {playerIndex} collided with stash: {other.name}, canExplode set to {canExplode}");
		}
		else if (!other.gameObject)
		{
			Debug.Log("Player stopped colliding with anything.");
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Stash"))
		{
			canExplode = false;
			_ = stashScript != null;
		}
	}
}
