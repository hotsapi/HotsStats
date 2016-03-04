using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace StatsFetcher
{
	public class FileMonitor
	{
		public readonly string BattleLobbyPath = Path.Combine(System.IO.Path.GetTempPath(), @"Heroes of the Storm\TempWriteReplayP1\replay.server.battlelobby");
		DateTime lastTime;

		public event EventHandler<EventArgs<string>> BattleLobbyCreated;
		protected virtual void OnBattleLobbyCreated(string path)
		{
			BattleLobbyCreated?.Invoke(this, new EventArgs<string>(path));
		}

		public event EventHandler<EventArgs<string>> ReplayFileCreated;
		protected virtual void OnReplayFileCreated(string path)
		{
			ReplayFileCreated?.Invoke(this, new EventArgs<string>(path));
		}

		// In out case it's simpler to poll this file instead of using file system watchers
		public void StartWatchingForLobby(int interval = 1000)
		{
			ThreadPool.QueueUserWorkItem(q => {
				while (true) {
					if (File.Exists(BattleLobbyPath) && File.GetLastWriteTime(BattleLobbyPath) != lastTime) {
						lastTime = File.GetLastWriteTime(BattleLobbyPath);
						OnBattleLobbyCreated(BattleLobbyPath);
					}
					Thread.Sleep(interval);
				}
			});
		}

		public void StartWatchingForReplay()
		{
			// todo: watch when first partial replay is created (at 1:00 mark) to get map and hero data
			throw new NotImplementedException();
		}
	}
}
