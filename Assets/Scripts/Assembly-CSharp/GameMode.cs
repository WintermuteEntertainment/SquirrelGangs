using System.Collections.Generic;
using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
	protected List<PlayerStats> players = new List<PlayerStats>();

	public virtual void Init(List<PlayerStats> playerList)
	{
		players = playerList;
		foreach (PlayerStats player in players)
		{
			player.OnDeath += HandlePlayerDeath;
		}
	}

	protected abstract void HandlePlayerDeath(PlayerStats player);

	protected virtual void OnDestroy()
	{
		foreach (PlayerStats player in players)
		{
			if (player != null)
			{
				player.OnDeath -= HandlePlayerDeath;
			}
		}
	}
}
