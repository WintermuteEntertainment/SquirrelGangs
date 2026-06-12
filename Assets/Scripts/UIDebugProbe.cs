#if UNITY_EDITOR
// Temporary recovery diagnostic v2 - delete once UI input works.
// Logs the input module's INTERNAL pointer pipeline (action values, module
// raycast, click dispatch) plus runtime listeners on every Button.
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		HookAllButtons("probe-start");
	}

	private void OnSceneLoaded(Scene s, LoadSceneMode m) => HookAllButtons(s.name);

	private void HookAllButtons(string context)
	{
		Button[] buttons = FindObjectsOfType<Button>(true);
		foreach (Button b in buttons)
		{
			Button captured = b;
			b.onClick.AddListener(() => Debug.Log($"[UIDoctor] >>> Button.onClick FIRED: {captured.name}"));
		}
		Debug.Log($"[UIDoctor] hooked {buttons.Length} buttons ({context})");
	}

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
		EventSystem es = EventSystem.current;
		if (es == null) { Debug.Log(sb.Append("eventSystem=NULL").ToString()); return; }

		if (es.currentInputModule is InputSystemUIInputModule m)
		{
			InputAction point = m.point != null ? m.point.action : null;
			InputAction click = m.leftClick != null ? m.leftClick.action : null;
			if (point != null)
				sb.Append("pointValue=").Append(point.ReadValue<Vector2>()).Append(' ');
			if (click != null)
				sb.Append($"clickPhase={click.phase} ");
			// What does the MODULE think is under the pointer?
			RaycastResult rr = m.GetLastRaycastResult(0);
			sb.Append("moduleRaycast=").Append(rr.gameObject != null ? rr.gameObject.name : "NOTHING").Append(' ');
		}
		else
		{
			sb.Append("module=").Append(es.currentInputModule != null ? es.currentInputModule.GetType().Name : "NULL").Append(' ');
		}

		sb.Append("selected=").Append(es.currentSelectedGameObject != null ? es.currentSelectedGameObject.name : "none").Append(' ');
		sb.Append("playerInputs=").Append(FindObjectsOfType<PlayerInput>(true).Length);
		sb.Append(" pim=").Append(FindObjectsOfType<PlayerInputManager>(true).Length);
		if (Mouse.current != null)
			sb.Append(" rawMouse=").Append(Mouse.current.position.ReadValue());

		Debug.Log(sb.ToString());
	}

	private void LogClick()
	{
		EventSystem es = EventSystem.current;
		Vector2 pos = Mouse.current.position.ReadValue();
		if (es == null) { Debug.Log($"[UIDoctor] CLICK at {pos} - NO EventSystem"); return; }

		StringBuilder sb = new StringBuilder($"[UIDoctor] CLICK at {pos} ");
		if (es.currentInputModule is InputSystemUIInputModule m)
		{
			InputAction point = m.point != null ? m.point.action : null;
			if (point != null) sb.Append($"pointAction={point.ReadValue<Vector2>()} ");
			RaycastResult rr = m.GetLastRaycastResult(0);
			sb.Append("moduleSees=").Append(rr.gameObject != null ? rr.gameObject.name : "NOTHING").Append(' ');
		}
		PointerEventData ped = new PointerEventData(es) { position = pos };
		List<RaycastResult> results = new List<RaycastResult>();
		es.RaycastAll(ped, results);
		sb.Append("manualRaycast=");
		for (int i = 0; i < Mathf.Min(3, results.Count); i++)
			sb.Append(results[i].gameObject.name).Append(" > ");
		Debug.Log(sb.ToString());
	}
}
#endif
