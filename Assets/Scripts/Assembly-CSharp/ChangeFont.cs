using UnityEngine;
using UnityEngine.UI;

public class ChangeFont : MonoBehaviour
{
	public Font newFont;

	private void Start()
	{
		Text[] array = Resources.FindObjectsOfTypeAll<Text>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].font = newFont;
		}
	}
}
