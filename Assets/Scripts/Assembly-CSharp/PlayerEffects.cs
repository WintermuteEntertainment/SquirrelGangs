using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
	public ParticleSystem throwEffectP1;

	public ParticleSystem throwEffectP2;

	public ParticleSystem landingEffectP1;

	public ParticleSystem landingEffectP2;

	public ParticleSystem digEffect;

	public ParticleSystem hideEffect;

	public ParticleSystem boomEffectP1;

	public ParticleSystem boomEffectP2;

	public ParticleSystem boomEffectP3;

	public ParticleSystem boomEffectP4;

	public ParticleSystem armEffect;

	public PlayerStats stats;

	public int playerIndex;

	public void PlayBoomEffect()
	{
		if (playerIndex == 0 && boomEffectP1 != null)
		{
			boomEffectP1.Play();
		}
		else if (playerIndex == 1 && boomEffectP2 != null)
		{
			boomEffectP2.Play();
		}
		else if (playerIndex == 1 && boomEffectP2 != null)
		{
			boomEffectP3.Play();
		}
		else if (playerIndex == 1 && boomEffectP2 != null)
		{
			boomEffectP4.Play();
		}
	}

	public void PlayThrowEffect()
	{
		if (playerIndex == 0 && throwEffectP1 != null)
		{
			throwEffectP1.Play();
		}
		else if (playerIndex == 1 && throwEffectP2 != null)
		{
			throwEffectP2.Play();
		}
	}

	public void PlayLandingEffect()
	{
		if (playerIndex == 0 && landingEffectP1 != null)
		{
			landingEffectP1.Play();
		}
		else if (playerIndex == 1 && landingEffectP2 != null)
		{
			landingEffectP2.Play();
		}
	}

	public void PlayDigEffect()
	{
		if (digEffect != null)
		{
			digEffect.Play();
		}
	}

	public void PlayArmEffect()
	{
		if (armEffect != null)
		{
			armEffect.Play();
		}
	}

	public void PlayHideEffect()
	{
		if (hideEffect != null)
		{
			hideEffect.Play();
		}
	}
}
