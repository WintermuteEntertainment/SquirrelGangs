using TMPro;
using UnityEngine;

public class ChangeTMPFont : MonoBehaviour
{
	public TMP_FontAsset newFont;

	private void Start()
	{
		TMP_Text[] array = Resources.FindObjectsOfTypeAll<TMP_Text>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].font = newFont;
		}
	}
}
