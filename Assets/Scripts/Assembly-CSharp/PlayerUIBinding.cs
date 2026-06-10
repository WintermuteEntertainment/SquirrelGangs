using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerUIBinding
{
	[Header("Texts")]
	public TMP_Text hpText;

	public TMP_Text ammoText;

	public TMP_Text deathsText;

	[Header("Sliders")]
	public Slider hpSlider;

	public Slider ammoSlider;

	[Header("Death Icons Parent")]
	public Transform deathIconsParent;

	[Header("Whole UI Parent")]
	public GameObject uiParent;

	public void Initialize(PlayerStats stats)
	{
		if (hpSlider != null)
		{
			hpSlider.maxValue = stats.maxHP;
			hpSlider.value = stats.currentHP;
		}
		if (ammoSlider != null)
		{
			ammoSlider.maxValue = stats.maxAmmo;
			ammoSlider.value = stats.currentAmmo;
		}
	}

	public void UpdateUI(PlayerStats stats)
	{
		if (hpText != null)
		{
			hpText.text = $"P{stats.playerIndex + 1} HP: {stats.currentHP}";
		}
		if (ammoText != null)
		{
			ammoText.text = $"P{stats.playerIndex + 1} Ammo: {stats.currentAmmo}";
		}
		if (deathsText != null)
		{
			deathsText.text = $"Deaths: {stats.deaths}";
		}
		if (hpSlider != null)
		{
			hpSlider.value = stats.currentHP;
		}
		if (ammoSlider != null)
		{
			ammoSlider.value = stats.currentAmmo;
		}
	}
}
