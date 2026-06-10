using System.Collections.Generic;
using UnityEngine;

public class DynamicCameraSetup : MonoBehaviour
{
	public Camera[] playerCameras;

	public GameManager m_gameManager;

	private void Start()
	{
		List<int> list = new List<int>();
		if (PlayerPrefs.GetInt("Player1Character", -1) != -1)
		{
			list.Add(0);
		}
		if (PlayerPrefs.GetInt("Player2Character", -1) != -1)
		{
			list.Add(1);
		}
		if (PlayerPrefs.GetInt("Player3Character", -1) != -1)
		{
			list.Add(2);
		}
		if (PlayerPrefs.GetInt("Player4Character", -1) != -1)
		{
			list.Add(3);
		}
		SetCameraViewports(list.Count);
		m_gameManager.AdjustCamerasAndUI();
	}

	private void SetCameraViewports(int playerCount)
	{
		for (int i = 0; i < playerCameras.Length; i++)
		{
			if (i < playerCount)
			{
				playerCameras[i].gameObject.SetActive(value: true);
			}
			else
			{
				playerCameras[i].gameObject.SetActive(value: false);
			}
		}
		switch (playerCount)
		{
		case 2:
			playerCameras[0].rect = new Rect(0f, 0.5f, 1f, 0.5f);
			playerCameras[1].rect = new Rect(0f, 0f, 1f, 0.5f);
			break;
		case 3:
			playerCameras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
			playerCameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			playerCameras[2].rect = new Rect(0f, 0f, 0.5f, 0.5f);
			break;
		case 4:
			playerCameras[0].rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
			playerCameras[1].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			playerCameras[2].rect = new Rect(0f, 0f, 0.5f, 0.5f);
			playerCameras[3].rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
			break;
		}
	}
}
