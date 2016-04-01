using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatsFetcher
{
	public class FileMonitor
	{
		public readonly string BattleLobbyPath = Path.Combine(Path.GetTempPath(), @"Heroes of the Storm\TempWriteReplayP1\replay.server.battlelobby");
		public readonly string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
		private FileSystemWatcher rejoinWatcher;
		private FileSystemWatcher replayWatcher;
		private DateTime lobbyLastModified;

		#region Events
		/// <summary>
		/// when battlelobby file is created during loading screen
		/// </summary>
		public event EventHandler<EventArgs<string>> BattleLobbyCreated;
		protected virtual void OnBattleLobbyCreated(string path)
		{
			BattleLobbyCreated?.Invoke(this, new EventArgs<string>(path));
		}
		/// <summary>
		/// when first partial replay is created (at 1:00 mark) to get map and hero data
		/// </summary>
		public event EventHandler<EventArgs<string>> RejoinFileCreated;
		protected virtual void OnRejoinFileCreated(string path)
		{
			RejoinFileCreated?.Invoke(this, new EventArgs<string>(path));
		}

		/// <summary>
		/// when full replay is avalable after match
		/// </summary>
		public event EventHandler<EventArgs<string>> ReplayFileCreated;
		protected virtual void OnReplayFileCreated(string path)
		{
			ReplayFileCreated?.Invoke(this, new EventArgs<string>(path));
		}
		#endregion

		public void StartMonitoring(int interval = 1000)
		{
			/*
			TODO : Avoid using the threadpool directly.
			Instead, for long running operations, it is recommended to use a long running task. This way the task scheduler will 
			alocate a dedicated thread for it and keep teh threadpool working smoothly
			*/

			Task.Factory.StartNew(() => {
				while (true) {
					if (File.Exists(BattleLobbyPath) && File.GetLastWriteTime(BattleLobbyPath) != lobbyLastModified) {
						lobbyLastModified = File.GetLastWriteTime(BattleLobbyPath);
						OnBattleLobbyCreated(BattleLobbyPath);
					}
					Thread.Sleep(interval);
				}
			},TaskCreationOptions.LongRunning);

			rejoinWatcher = new FileSystemWatcher();
			rejoinWatcher.Path = ProfilePath;
			rejoinWatcher.Filter = "*.StormSave";
			rejoinWatcher.IncludeSubdirectories = true;
			rejoinWatcher.Created += (o, e) => {
				OnRejoinFileCreated(e.FullPath);
			};
			rejoinWatcher.EnableRaisingEvents = true;

			replayWatcher = new FileSystemWatcher();
			replayWatcher.Path = ProfilePath;
			replayWatcher.Filter = "*.StormReplay";
			replayWatcher.IncludeSubdirectories = true;
			replayWatcher.Created += (o, e) => {
				OnReplayFileCreated(e.FullPath);
			};
			replayWatcher.EnableRaisingEvents = true;
		}
	}
}
