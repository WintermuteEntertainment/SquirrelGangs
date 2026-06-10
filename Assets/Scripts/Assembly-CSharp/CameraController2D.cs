using UnityEngine;

public class CameraController2D : MonoBehaviour
{
	public Transform target;

	public float smoothSpeed = 0.125f;

	public Vector3 offset;

	public Vector2 minPosition;

	public Vector2 maxPosition;

	public Camera cameraComponent;

	public bool isPlayer1 = true;

	private void Start()
	{
		cameraComponent = GetComponent<Camera>();
		SetupCamera();
	}

	private void FixedUpdate()
	{
		if (target != null)
		{
			Vector3 vector = target.position + offset;
			Vector3 position = Vector3.Lerp(b: new Vector3(Mathf.Clamp(vector.x, minPosition.x, maxPosition.x), Mathf.Clamp(vector.y, minPosition.y, maxPosition.y), vector.z), a: base.transform.position, t: smoothSpeed);
			base.transform.position = position;
		}
	}

	private void SetupCamera()
	{
		if (isPlayer1)
		{
			cameraComponent.rect = new Rect(0f, 0f, 0.5f, 1f);
		}
		else
		{
			cameraComponent.rect = new Rect(0.5f, 0f, 0.5f, 1f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (target != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(base.transform.position, target.position);
			Gizmos.DrawWireSphere(target.position + offset, 0.5f);
		}
	}
}
