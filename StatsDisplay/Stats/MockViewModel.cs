using System.Collections.Generic;
using Heroes.ReplayParser;
using StatsFetcher;

namespace StatsDisplay.Stats
{
	public class MockViewModel : ShortStatsVm
	{
		public MockViewModel()
		{
			var game = new Game {
				Region = Region.EU,
				GameMode = GameMode.QuickMatch,
				Map = "Cursed Hollow"
			};
			var players = new List<PlayerProfile>();
			for (int i = 0; i < 10; i++) {
				var p = new PlayerProfile(game, $"Player {i}#10{i}", Region.EU) { Hero = "Raynor", HeroLevel = 4 + i, MapWinRate = 48.7f + i, HeroWinRate = 35.2f + i, HotslogsId = 123 + i, GamesCount = 200 * i, Team = i >= 5 ? 1 : 0 };
				p.Ranks[GameMode.QuickMatch] = new PlayerProfile.MmrValue(GameMode.QuickMatch, 2200, null, null);
				players.Add(p);
			}
			game.Players = players;
			game.Me = players[3];
			App.Game = game;
			OnActivated();
		}
	}
}
