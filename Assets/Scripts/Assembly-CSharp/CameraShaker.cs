using Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
	private CinemachineImpulseSource impulseSource;

	private void Awake()
	{
		impulseSource = GetComponent<CinemachineImpulseSource>();
		if (impulseSource == null)
		{
			Debug.LogError("CameraShaker on " + base.gameObject.name + " requires a CinemachineImpulseSource component!");
		}
	}

	public void Shake(float force = 1f)
	{
		if (impulseSource != null)
		{
			impulseSource.GenerateImpulse(Vector3.one * force);
		}
	}

	public static void ShakeAll(float force = 1f)
	{
		CameraShaker[] array = Object.FindObjectsOfType<CameraShaker>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Shake(force);
		}
	}
}
