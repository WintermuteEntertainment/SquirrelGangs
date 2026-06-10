using System.Collections.Generic;

public static class PlayerDataHolder
{
	public static List<PlayerData> SelectedPlayers { get; set; } = new List<PlayerData>();

	public static int PlayerLimit { get; set; }
}
