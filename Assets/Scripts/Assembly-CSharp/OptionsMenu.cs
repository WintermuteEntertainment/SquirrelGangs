using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
	public GameObject optionsMenuUI;

	public void ToggleOptionsMenu()
	{
		optionsMenuUI.SetActive(!optionsMenuUI.activeSelf);
	}

	public void SetVolume(float volume)
	{
		AudioListener.volume = volume;
	}

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
	}

	public void SetFullscreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}

	public void BackToPreviousMenu()
	{
		optionsMenuUI.SetActive(value: false);
		if (Time.timeScale == 0f)
		{
			Object.FindObjectOfType<PauseMenu>().Resume();
		}
		else
		{
			Object.FindObjectOfType<MainMenu>().BackToMainMenu();
		}
	}
}
