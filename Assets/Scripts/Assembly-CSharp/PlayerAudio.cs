using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
	public AudioClip throwSound;

	public AudioClip pickupSound;

	public AudioClip damageSound;

	public AudioClip landingSound;

	public AudioClip digSound;

	public AudioClip hideSound;

	public AudioClip boomSound;

	public AudioClip armSound;

	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			Debug.LogError("PlayerAudio is missing an AudioSource component!", this);
		}
	}

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void PlayBoomSound()
	{
		PlaySound(boomSound);
	}

	public void PlayThrowSound()
	{
		PlaySound(throwSound);
	}

	public void PlayPickupSound()
	{
		PlaySound(pickupSound);
	}

	public void PlayDamageSound()
	{
		PlaySound(damageSound);
	}

	public void PlayLandingSound()
	{
		PlaySound(landingSound);
	}

	public void PlayDigSound()
	{
		PlaySound(digSound);
	}

	public void PlayHideSound()
	{
		PlaySound(hideSound);
	}

	public void PlayArmSound()
	{
		PlaySound(armSound);
	}

	public void PlayPlayArmEffect()
	{
	}

	public void PlaySound(AudioClip clip)
	{
		if ((bool)clip && (bool)audioSource && audioSource.isActiveAndEnabled)
		{
			if (clip == null)
			{
				Debug.LogWarning("Tried to play a null AudioClip!");
			}
			else if (audioSource == null)
			{
				Debug.LogError("No AudioSource found on PlayerAudio!", this);
			}
			else
			{
				audioSource.PlayOneShot(clip);
			}
		}
	}
}
