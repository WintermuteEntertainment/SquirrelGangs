using UnityEngine;

public static class PlayerPrefsUtility
{
	public static void SetColor(string key, Color color)
	{
		PlayerPrefs.SetFloat(key + "_r", color.r);
		PlayerPrefs.SetFloat(key + "_g", color.g);
		PlayerPrefs.SetFloat(key + "_b", color.b);
		PlayerPrefs.SetFloat(key + "_a", color.a);
	}

	public static Color GetColor(string key, Color defaultValue)
	{
		if (PlayerPrefs.HasKey(key + "_r"))
		{
			float r = PlayerPrefs.GetFloat(key + "_r");
			float g = PlayerPrefs.GetFloat(key + "_g");
			float b = PlayerPrefs.GetFloat(key + "_b");
			float a = PlayerPrefs.GetFloat(key + "_a");
			return new Color(r, g, b, a);
		}
		return defaultValue;
	}
}
