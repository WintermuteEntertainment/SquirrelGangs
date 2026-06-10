using System.Collections.Generic;
using UnityEngine;

public class MultiPlayerGameMode : GameMode
{
	[SerializeField]
	private int roundsToWin = 2;

	private Dictionary<PlayerStats, int> scores = new Dictionary<PlayerStats, int>();

	private int currentRound = 1;

	public override void Init(List<PlayerStats> playerList)
	{
		base.Init(playerList);
		scores.Clear();
		foreach (PlayerStats player in players)
		{
			scores[player] = 0;
		}
	}

	protected override void HandlePlayerDeath(PlayerStats deadPlayer)
	{
		if (deadPlayer == null)
		{
			return;
		}
		Debug.Log("[Multiplayer] " + deadPlayer.name + " died.");
		List<PlayerStats> list = players.FindAll((PlayerStats p) => p.gameObject.activeSelf);
		if (list.Count == 1)
		{
			PlayerStats playerStats = list[0];
			scores[playerStats]++;
			Debug.Log($"[Multiplayer] {playerStats.name} wins Round {currentRound}!");
			if (scores[playerStats] >= roundsToWin)
			{
				Debug.Log("[Multiplayer] " + playerStats.name + " wins the match!");
				UIManager.Instance.ShowWinner(playerStats);
			}
			else
			{
				currentRound++;
				ResetRound();
			}
		}
	}

	private void ResetRound()
	{
		foreach (PlayerStats player in players)
		{
			player.gameObject.SetActive(value: true);
			player.ResetStats();
		}
		Debug.Log($"[Multiplayer] Starting Round {currentRound}.");
	}
}
