using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
	public GameObject[] playerPrefabs;

	public Transform[] spawnPoints;

	public CameraPointer cameraPointer;

	private int playerCount;

	private Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();

	private List<GameObject> instantiatedPlayers = new List<GameObject>();

	private PlayerData playerData;

	private void Start()
	{
		Debug.Log("PlayerManager Start method called.");
		if (cameraPointer == null)
		{
			cameraPointer = Object.FindObjectOfType<CameraPointer>();
		}
		Debug.Log((cameraPointer != null) ? "CameraPointer found and assigned." : "CameraPointer not found.");
	}

	private void FixedUpdate()
	{
		if (playerCount < playerPrefabs.Length && IsInputPressed())
		{
			AddNewPlayer();
		}
	}

	private bool IsInputPressed()
	{
		foreach (Gamepad item in Gamepad.all)
		{
			if (item != null && (item.buttonSouth.wasPressedThisFrame || item.startButton.wasPressedThisFrame))
			{
				return true;
			}
		}
		if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
		{
			return true;
		}
		return false;
	}

	private void AddNewPlayer()
	{
		if (playerCount >= playerPrefabs.Length)
		{
			Debug.LogError("Maximum player count reached.");
			return;
		}
		int num = playerCount;
		int num2 = PlayerPrefs.GetInt($"Player{num + 1}Character", 0);
		PlayerPrefsUtility.GetColor($"Player{num + 1}NutColor", Color.yellow);
		GameObject gameObject = Object.Instantiate(playerPrefabs[num2], spawnPoints[num].position, Quaternion.identity);
		gameObject.name = playerPrefabs[num2].name;
		PlayerController component = gameObject.GetComponent<PlayerController>();
		if (component != null)
		{
			component.Initialize(playerData);
			PlayerInput component2 = gameObject.GetComponent<PlayerInput>();
			if (component2 != null)
			{
				component2.user.AssociateActionsWithUser(component2.actions);
				component2.SwitchCurrentActionMap($"P{num + 1}");
				Debug.Log($"Assigned device to player {num}.");
				if (cameraPointer != null)
				{
					cameraPointer.AssignCamera(num, gameObject.transform);
					playerInputs[num] = component2;
					instantiatedPlayers.Add(gameObject);
					playerCount++;
					Debug.Log($"Player {num} joined.");
					CleanupDuplicatePlayers();
				}
				else
				{
					Debug.LogError("CameraPointer is not assigned.");
					Object.Destroy(gameObject);
				}
			}
			else
			{
				Debug.LogError("PlayerInput component is missing on the instantiated player prefab.");
				Object.Destroy(gameObject);
			}
		}
		else
		{
			Debug.LogError("PlayerController component is missing on the instantiated player prefab.");
			Object.Destroy(gameObject);
		}
	}

	private void DeactivateCloneIfExists()
	{
		GameObject gameObject = GameObject.Find("Player1Prefab(Clone)");
		if (gameObject != null)
		{
			gameObject.SetActive(value: false);
			Debug.Log("Deactivated unwanted Player1Prefab(Clone).");
		}
	}

	private void CleanupDuplicatePlayers()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject instantiatedPlayer in instantiatedPlayers)
		{
			if (instantiatedPlayer.name.Contains("(Clone)"))
			{
				list.Add(instantiatedPlayer);
			}
		}
		foreach (GameObject item in list)
		{
			instantiatedPlayers.Remove(item);
			Object.Destroy(item);
			Debug.Log("Destroyed a duplicate player instance with '(Clone)' label.");
		}
		for (int i = 0; i < instantiatedPlayers.Count; i++)
		{
			GameObject gameObject = instantiatedPlayers[i];
			if (gameObject != null)
			{
				cameraPointer.AssignCamera(i, gameObject.transform);
			}
		}
	}
}
