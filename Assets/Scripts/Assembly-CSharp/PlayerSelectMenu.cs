using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectMenu : MonoBehaviour
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

	[Header("Player Setup")]
	public PlayerSlotUI[] playerSlots;

	public GameObject[] playerPrefabs;

	public Color[] nutColors;

	[Header("UI Elements")]
	public TMP_Dropdown levelSelectDropdown;

	[Header("Cameras & Dividers")]
	public Camera[] playerCameras;

	public GameObject[] screenDividers;

	private List<PlayerData> selectedPlayers = new List<PlayerData>();

	private void Start()
	{
		PopulateDeviceDropdowns();
		DeactivateAllCamerasAndDividers();
	}

	private void PopulateDeviceDropdowns()
	{
		PlayerSlotUI[] array = playerSlots;
		foreach (PlayerSlotUI playerSlotUI in array)
		{
			if (playerSlotUI.deviceDropdown == null)
			{
				continue;
			}
			playerSlotUI.deviceDropdown.ClearOptions();
			List<string> list = new List<string>();
			foreach (InputDevice device in InputSystem.devices)
			{
				list.Add($"{device.deviceId}: {device.displayName}");
			}
			playerSlotUI.deviceDropdown.AddOptions(list);
		}
	}

	public void ConfirmSelection()
	{
		selectedPlayers.Clear();
		for (int i = 0; i < playerSlots.Length; i++)
		{
			if (playerSlots[i].toggle.isOn)
			{
				selectedPlayers.Add(CreatePlayerData(i, playerSlots[i]));
			}
		}
		PlayerDataHolder.PlayerLimit = selectedPlayers.Count;
		if (selectedPlayers.Count < 1)
		{
			Debug.LogError("At least one player must be selected.");
			return;
		}
		PlayerDataHolder.SelectedPlayers = selectedPlayers;
		if (selectedPlayers.Count == 1)
		{
			SceneManager.LoadScene("SinglePlayerScene");
		}
		else
		{
			SceneManager.LoadScene(levelSelectDropdown.options[levelSelectDropdown.value].text);
		}
		base.gameObject.SetActive(value: false);
	}

	private PlayerData CreatePlayerData(int playerIndex, PlayerSlotUI slot)
	{
		int value = slot.characterDropdown.value;
		Color color = Color.white;
		if (slot.colorPicker != null && slot.colorPicker.colorPreview != null)
		{
			color = slot.colorPicker.colorPreview.color;
		}
		int health = (int)slot.healthSlider.value;
		int ammo = (int)slot.ammoSlider.value;
		string text = slot.deviceDropdown.options[slot.deviceDropdown.value].text;
		InputDevice device = null;
		foreach (InputDevice device2 in InputSystem.devices)
		{
			if ($"{device2.deviceId}: {device2.displayName}" == text)
			{
				device = device2;
				break;
			}
		}
		PlayerData result = new PlayerData
		{
			PlayerIndex = playerIndex,
			CharacterIndex = value,
			NutColor = color,
			Health = health,
			Ammo = ammo,
			Device = device
		};
		Debug.Log($"Player {playerIndex} selected prefab: {playerPrefabs[value].name}, NutColor: {color}");
		return result;
	}

	private void DeactivateAllCamerasAndDividers()
	{
		Camera[] array = playerCameras;
		foreach (Camera camera in array)
		{
			if (camera != null)
			{
				camera.gameObject.SetActive(value: false);
			}
		}
		GameObject[] array2 = screenDividers;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private void AdjustCamerasAndUI(int playerCount)
	{
		DeactivateAllCamerasAndDividers();
		for (int i = 0; i < playerCount; i++)
		{
			if (i < playerCameras.Length && playerCameras[i] != null)
			{
				playerCameras[i].gameObject.SetActive(value: true);
			}
		}
		if (playerCount == 2 && screenDividers.Length != 0)
		{
			screenDividers[0].SetActive(value: true);
		}
		else if (playerCount == 3 && screenDividers.Length > 1)
		{
			screenDividers[1].SetActive(value: true);
		}
		else if (playerCount == 4 && screenDividers.Length > 2)
		{
			screenDividers[2].SetActive(value: true);
		}
		Debug.Log($"Adjusted cameras and UI for {playerCount} players.");
	}

	private void ConfigureCameras(int playerCount)
	{
		for (int i = 0; i < playerCameras.Length; i++)
		{
			if (playerCameras[i] != null)
			{
				playerCameras[i].gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < screenDividers.Length; j++)
		{
			if (screenDividers[j] != null)
			{
				screenDividers[j].SetActive(value: false);
			}
		}
		for (int k = 0; k < playerCount; k++)
		{
			if (playerCameras[k] != null)
			{
				playerCameras[k].gameObject.SetActive(value: true);
			}
		}
		switch (playerCount)
		{
		case 1:
			playerCameras[0].rect = new Rect(0f, 0f, 1f, 1f);
			break;
		case 2:
			playerCameras[0].rect = new Rect(0f, 0f, 0.5f, 1f);
			playerCameras[1].rect = new Rect(0.5f, 0f, 0.5f, 1f);
			if (screenDividers.Length != 0)
			{
				screenDividers[0].SetActive(value: true);
			}
			break;
		case 3:
		case 4:
		{
			playerCameras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
			playerCameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			playerCameras[2].rect = new Rect(0f, 0f, 0.5f, 0.5f);
			if (playerCount == 4)
			{
				playerCameras[3].rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
			}
			GameObject[] array = screenDividers;
			for (int l = 0; l < array.Length; l++)
			{
				array[l].SetActive(value: true);
			}
			break;
		}
		}
	}
}
