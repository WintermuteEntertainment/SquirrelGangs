using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public GameObject mainMenuUI;

	public GameObject playerSelectUI;

	public GameObject optionsMenuUI;

	private void Start()
	{
		playerSelectUI.SetActive(value: false);
		optionsMenuUI.SetActive(value: false);
	}

	public void StartGame()
	{
		mainMenuUI.SetActive(value: false);
		playerSelectUI.SetActive(value: true);
	}

	public void OpenOptions()
	{
		mainMenuUI.SetActive(value: false);
		optionsMenuUI.SetActive(value: true);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void BackToMainMenu()
	{
		playerSelectUI.SetActive(value: false);
		optionsMenuUI.SetActive(value: false);
		mainMenuUI.SetActive(value: true);
	}
}
