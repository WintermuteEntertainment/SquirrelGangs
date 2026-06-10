using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerGameMode : GameMode
{
	[SerializeField]
	private int maxLives = 3;

	[SerializeField]
	private PlayerController player;

	protected override void HandlePlayerDeath(PlayerStats player)
	{
		if (!(player == null))
		{
			Debug.Log($"[SinglePlayer] Player died. Lives left: {player.playerIndex} = {player.playerIndex}");
			if (player.deaths >= maxLives)
			{
				Debug.Log("[SinglePlayer] Game Over. Restart tutorial.");
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		}
	}
}
