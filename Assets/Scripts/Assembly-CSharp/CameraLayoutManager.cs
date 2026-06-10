using System.Collections.Generic;
using UnityEngine;

public class CameraLayoutManager : MonoBehaviour
{
	public static void ArrangeCameras(List<Camera> activeCams)
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			allCameras[i].gameObject.SetActive(value: false);
		}
		int count = activeCams.Count;
		for (int j = 0; j < count; j++)
		{
			Camera camera = activeCams[j];
			if (camera == null)
			{
				continue;
			}
			camera.gameObject.SetActive(value: true);
			switch (count)
			{
			case 1:
				camera.rect = new Rect(0f, 0f, 1f, 1f);
				break;
			case 2:
				camera.rect = ((j == 0) ? new Rect(0f, 0.5f, 1f, 0.5f) : new Rect(0f, 0f, 1f, 0.5f));
				break;
			case 3:
				if (j == 0)
				{
					camera.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
				}
				if (j == 1)
				{
					camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				}
				if (j == 2)
				{
					camera.rect = new Rect(0f, 0f, 1f, 0.5f);
				}
				break;
			case 4:
				if (j == 0)
				{
					camera.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
				}
				if (j == 1)
				{
					camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
				}
				if (j == 2)
				{
					camera.rect = new Rect(0f, 0f, 0.5f, 0.5f);
				}
				if (j == 3)
				{
					camera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
				}
				break;
			}
		}
	}
}
