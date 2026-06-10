using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
	public enum AnimationState
	{
		Idle = 0,
		Walking = 1,
		Sprinting = 2,
		Jumping = 3,
		Throwing = 4,
		Hiding = 5,
		Digging = 6
	}

	public Animator animator;

	private PlayerController playerController;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		playerController = GetComponent<PlayerController>();
	}

	private void Update()
	{
		HandleAnimations();
	}

	public void ResetAnimations()
	{
		animator.ResetTrigger("ThrowAcorn");
		animator.SetBool("isHidingAcorn", value: false);
		animator.SetBool("isDigging", value: false);
		animator.SetBool("isWalking", value: false);
		animator.SetBool("isIdle", value: false);
		animator.SetBool("isSprinting", value: false);
		animator.SetBool("isJumping", value: false);
	}

	public void SetState(AnimationState state)
	{
		switch (state)
		{
		case AnimationState.Idle:
			animator.SetFloat("Speed", 0f);
			break;
		case AnimationState.Walking:
			animator.SetFloat("Speed", Mathf.Abs(playerController.rb.velocity.x));
			break;
		case AnimationState.Sprinting:
			animator.SetBool("isSprinting", value: true);
			break;
		case AnimationState.Jumping:
			animator.SetBool("isJumping", value: true);
			break;
		case AnimationState.Throwing:
			animator.SetTrigger("ThrowAcorn");
			break;
		case AnimationState.Hiding:
			animator.SetBool("isHidingAcorn", value: true);
			break;
		case AnimationState.Digging:
			animator.SetBool("isDigging", value: true);
			break;
		}
	}

	private void HandleAnimations()
	{
		if (!(playerController == null))
		{
			animator.SetFloat("Speed", Mathf.Abs(playerController.rb.velocity.x));
			animator.SetBool("isJumping", !playerController.isGrounded);
			animator.SetBool("isSprinting", playerController.isSprinting);
			animator.SetBool("isWalking", Mathf.Abs(playerController.rb.velocity.x) > 0.1f);
		}
	}
}
