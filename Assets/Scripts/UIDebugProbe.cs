#if UNITY_EDITOR
// Temporary recovery diagnostic - delete once UI input works.
// Auto-spawns in Play mode; logs input/event-system state every 2s and a
// full UI raycast on every left click, prefixed [UIDoctor].
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIDebugProbe : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Spawn()
	{
		GameObject go = new GameObject("UIDebugProbe");
		DontDestroyOnLoad(go);
		go.AddComponent<UIDebugProbe>();
	}

	private float nextStatus;

	private void Update()
	{
		if (Time.unscaledTime >= nextStatus)
		{
			nextStatus = Time.unscaledTime + 2f;
			LogStatus();
		}
		if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
		{
			LogClick();
		}
	}

	private void LogStatus()
	{
		StringBuilder sb = new StringBuilder("[UIDoctor] STATUS ");

		sb.Append("devices=[");
		foreach (InputDevice d in InputSystem.devices) sb.Append(d.layout).Append(' ');
		sb.Append("] ");

		sb.Append("focused=").Append(Application.isFocused).Append(' ');

		EventSystem es = EventSystem.current;
		if (es == null)
		{
			sb.Append("eventSystem=NULL");
			Debug.Log(sb.ToString());
			return;
		}
		sb.Append("es=").Append(es.gameObject.name).Append(' ');
		sb.Append("module=").Append(es.currentInputModule != null ? es.currentInputModule.GetType().Name : "NULL").Append(' ');

		if (es.currentInputModule is InputSystemUIInputModule m)
		{
			InputAction point = m.point != null ? m.point.action : null;
			InputAction click = m.leftClick != null ? m.leftClick.action : null;
			sb.Append("pointAction=").Append(point == null ? "NULL" : $"{point.name}/enabled={point.enabled}/controls={point.controls.Count} ");
			sb.Append("clickAction=").Append(click == null ? "NULL" : $"{click.name}/enabled={click.enabled}/controls={click.controls.Count} ");
		}

		MainMenu mm = FindObjectOfType<MainMenu>(true);
		if (mm != null)
		{
			sb.Append("panels[main=").Append(mm.mainMenuUI != null && mm.mainMenuUI.activeInHierarchy ? 1 : 0);
			sb.Append(" select=").Append(mm.playerSelectUI != null && mm.playerSelectUI.activeInHierarchy ? 1 : 0);
			sb.Append(" options=").Append(mm.optionsMenuUI != null && mm.optionsMenuUI.activeInHierarchy ? 1 : 0).Append("] ");
		}

		if (Mouse.current != null)
			sb.Append("mousePos=").Append(Mouse.current.position.ReadValue());

		Debug.Log(sb.ToString());
	}

	private void LogClick()
	{
		EventSystem es = EventSystem.current;
		Vector2 pos = Mouse.current.position.ReadValue();
		if (es == null)
		{
			Debug.Log($"[UIDoctor] CLICK at {pos} - NO EventSystem");
			return;
		}
		PointerEventData ped = new PointerEventData(es) { position = pos };
		List<RaycastResult> results = new List<RaycastResult>();
		es.RaycastAll(ped, results);
		StringBuilder sb = new StringBuilder($"[UIDoctor] CLICK at {pos} - {results.Count} UI hit(s): ");
		for (int i = 0; i < Mathf.Min(5, results.Count); i++)
			sb.Append(results[i].gameObject.name).Append(" > ");
		Debug.Log(sb.ToString());
	}
}
#endif
