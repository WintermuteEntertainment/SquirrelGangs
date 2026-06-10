using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectionMenu : MonoBehaviour
{
	public GameObject playerSelectUI;

	public GameObject mainMenuUI;

	public TMP_Dropdown player1CharacterDropdown;

	public TMP_Dropdown player2CharacterDropdown;

	public Slider player1HealthSlider;

	public Slider player1AmmoSlider;

	public Slider player2HealthSlider;

	public Slider player2AmmoSlider;

	public TextMeshProUGUI player1HealthText;

	public TextMeshProUGUI player1AmmoText;

	public TextMeshProUGUI player2HealthText;

	public TextMeshProUGUI player2AmmoText;

	private int player1CharacterIndex;

	private int player2CharacterIndex;

	private int player1Health;

	private int player1Ammo;

	private int player2Health;

	private int player2Ammo;

	private void Start()
	{
		playerSelectUI.SetActive(value: false);
	}

	public void OpenPlayerSelectMenu()
	{
		mainMenuUI.SetActive(value: false);
		playerSelectUI.SetActive(value: true);
	}

	public void BackToMainMenu()
	{
		playerSelectUI.SetActive(value: false);
		mainMenuUI.SetActive(value: true);
	}

	public void StartGame()
	{
		player1CharacterIndex = player1CharacterDropdown.value;
		player2CharacterIndex = player2CharacterDropdown.value;
		player1Health = (int)player1HealthSlider.value;
		player1Ammo = (int)player1AmmoSlider.value;
		player2Health = (int)player2HealthSlider.value;
		player2Ammo = (int)player2AmmoSlider.value;
		PlayerPrefs.SetInt("Player1Character", player1CharacterIndex);
		PlayerPrefs.SetInt("Player2Character", player2CharacterIndex);
		PlayerPrefs.SetInt("Player1Health", player1Health);
		PlayerPrefs.SetInt("Player1Ammo", player1Ammo);
		PlayerPrefs.SetInt("Player2Health", player2Health);
		PlayerPrefs.SetInt("Player2Ammo", player2Ammo);
		SceneManager.LoadScene("GameScene");
	}

	public void UpdatePlayer1HealthText(float value)
	{
		player1HealthText.text = "Health: " + (int)value;
	}

	public void UpdatePlayer1AmmoText(float value)
	{
		player1AmmoText.text = "Ammo: " + (int)value;
	}

	public void UpdatePlayer2HealthText(float value)
	{
		player2HealthText.text = "Health: " + (int)value;
	}

	public void UpdatePlayer2AmmoText(float value)
	{
		player2AmmoText.text = "Ammo: " + (int)value;
	}
}
