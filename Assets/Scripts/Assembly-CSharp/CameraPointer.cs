using UnityEngine;

public class CameraPointer : MonoBehaviour
{
	public Camera[] playerCameras;

	public void AssignCamera(int playerIndex, Transform playerTransform)
	{
		if (playerIndex < 0 || playerIndex > playerCameras.Length)
		{
			Debug.LogError("Invalid player index for camera assignment.");
			return;
		}
		Camera camera = playerCameras[playerIndex];
		if (camera != null)
		{
			camera.transform.SetParent(playerTransform);
			camera.transform.localPosition = new Vector3(0f, 0f, -10f);
			camera.transform.localRotation = Quaternion.identity;
			camera.gameObject.SetActive(value: true);
			Debug.Log($"Assigned Camera {camera} for Player {playerIndex}");
		}
		else
		{
			Debug.LogError($"Camera not found for player index {playerIndex}");
		}
	}
}
