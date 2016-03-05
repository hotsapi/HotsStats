using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StatsFetcher
{
	// Below you will see a bunch of ugly and unreliable code because structure of 'replay.server.battlelobby' is not documented
	public class BattleLobbyParser
	{
		private readonly int MaxTagLength = 24; // Longest player name is 12 letters. Unicode is allowed so it's roughly 24 bytes (but technically could be much more)
		private readonly byte[] data;

		public BattleLobbyParser(string file) : this(File.ReadAllBytes(file))
		{

		}

		public BattleLobbyParser(byte[] data)
		{
			this.data = data;
		}

		public Game Parse()
		{
			var game = new Game();
			game.Region = ExtractRegion();
			var tags = ExtractBattleTags();
			game.Players = tags.Select(tag => new PlayerProfile(game, tag, game.Region)).ToList();
			for (int i = 0; i < game.Players.Count; i++) {
				game.Players[i].Team = i >= 5 ? 0 : 1;
			}
			return game;
		}

		// Since we don't know structure of this file we will search for anything that looks like BattleTag
		// We will look for '#' symbol with digits on the right and letters to the left, prefixed with string length
		// We know that BattleTags reside at file end after large '0' padding
		public List<string> ExtractBattleTags()
		{
			var result = new List<string>();

			var offset = Find(Enumerable.Repeat<byte>(0, 32).ToArray());

			while (true) {
				offset = Find(new byte[] { (byte)'#' }, offset + 1);
				if (offset == -1)
					break;
				var tag = ExtractBattleTag(offset);
				if (tag != null)
					result.Add(tag);
			}

			return result;
		}

		/// <summary>
		/// Extract region
		/// </summary>
		public Region ExtractRegion()
		{
			// looks like region is always follows this pattern
			var i = Find(new byte[] { (byte)'s', (byte)'2', (byte)'m', (byte)'h', 0, 0 });
			if (i == -1)
				throw new ApplicationException("Can't parse region");
			else {
				var region = new string(new char[] { (char)data[i + 6], (char)data[i + 7] });
				return (Region)Enum.Parse(typeof(Region), region);
			}

		}

		/// <summary>
		/// Try to extract BattleTag given position of '#' symbol
		/// </summary>
		private string ExtractBattleTag(int offset)
		{
			var tag = new List<byte> { data[offset] };

			// look for digits to the right
			for (int i = 1; i < 10; i++) {
				var c = data[offset + i];
				if (char.IsDigit((char)c))
					tag.Add(c);
				else
					break;
			}

			// 3 digits for tag is too short and 9 is too much
			if (tag.Count < 5 || tag.Count > 9)
				return null;

			// look for player name to the right
			for (int i = 1; i < MaxTagLength + 2; i++) {
				var c = data[offset - i];
				tag.Insert(0, c);
				if (data[offset - i - 1] == tag.Count) // string length should be stored at string start
					break;
				if (i == MaxTagLength) // we exceeded max Name length
					return null;
			}

			return Encoding.UTF8.GetString(tag.ToArray());
		}

		/// <summary>
		/// Search for pattern in byte array
		/// </summary>
		private int Find(byte[] pattern, int offset = 0)
		{
			for (int i = offset; i < data.Length - pattern.Length; i++)
				if (Match(pattern, i))
					return i;

			return -1;
		}

		/// <summary>
		/// Try to match pattern at certain offset
		/// </summary>
		private bool Match(byte[] pattern, int offset = 0)
		{
			for (int i = 0; i < pattern.Length; i++)
				if (data[offset + i] != pattern[i])
					return false;

			return true;
		}
	}
}
