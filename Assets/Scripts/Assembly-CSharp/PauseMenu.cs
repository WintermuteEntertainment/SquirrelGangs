using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	public GameObject pauseMenuUI;

	public GameObject mainMenuUI;

	public GameObject playerSelectUI;

	public GameObject uIPanel;

	private bool isPaused;

	[SerializeField]
	private Button restartButton;

	private void Awake()
	{
		restartButton.onClick.AddListener(RestartMatch);
	}

	private void ResetTime()
	{
		Time.timeScale = 1f;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isPaused)
			{
				Resume();
			}
			else if (!isPaused)
			{
				Pause();
				Debug.Log("Paused.");
			}
		}
		if (UIManager.Instance.playerStats != null && UIManager.Instance.playerStats.Length <= 1)
		{
			restartButton.gameObject.SetActive(value: true);
			Debug.Log("Restart Button Activated!");
		}
		else
		{
			restartButton.gameObject.SetActive(value: false);
			Debug.Log("Restart Button Deactivated!");
		}
	}

	public void Resume()
	{
		if (pauseMenuUI != null)
		{
			pauseMenuUI.SetActive(value: false);
		}
		ResetTime();
		isPaused = false;
		Debug.Log("Unpaused.");
	}

	private void Pause()
	{
		if (pauseMenuUI != null)
		{
			pauseMenuUI.SetActive(value: true);
			if (uIPanel != null)
			{
				uIPanel.SetActive(value: false);
			}
			if (playerSelectUI != null)
			{
				playerSelectUI.SetActive(value: false);
			}
			if ((bool)mainMenuUI)
			{
				mainMenuUI.SetActive(value: false);
			}
		}
		Time.timeScale = 0f;
		isPaused = true;
		Debug.Log("Paused.");
	}

	public void OpenOptions()
	{
		if (pauseMenuUI != null)
		{
			pauseMenuUI.SetActive(value: false);
			uIPanel.SetActive(value: true);
		}
		Object.FindObjectOfType<OptionsMenu>().ToggleOptionsMenu();
	}

	public void Restart()
	{
		ResetTime();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void QuitToMainMenu()
	{
		ResetTime();
		SceneManager.LoadScene("MainMenu");
	}

	public void RestartMatch()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
