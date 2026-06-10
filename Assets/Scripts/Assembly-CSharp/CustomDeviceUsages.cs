using UnityEngine;
using UnityEngine.InputSystem;

public class CustomDeviceUsages : MonoBehaviour
{
	public int gamepadIndex;

	public string usageTag;

	private Gamepad m_Gamepad;

	protected void OnEnable()
	{
		if (gamepadIndex >= 0 && gamepadIndex < Gamepad.all.Count)
		{
			m_Gamepad = Gamepad.all[gamepadIndex];
			InputSystem.AddDeviceUsage(m_Gamepad, usageTag);
		}
	}

	protected void OnDisable()
	{
		if (m_Gamepad != null && m_Gamepad.added)
		{
			InputSystem.RemoveDeviceUsage(m_Gamepad, usageTag);
		}
		m_Gamepad = null;
	}
}
