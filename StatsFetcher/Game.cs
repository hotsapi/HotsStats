using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Heroes.ReplayParser;

namespace StatsFetcher
{
    public class Game : INotifyPropertyChanged
    {
        public List<PlayerProfile> Players { get; set; }
        public string Map { get; set; }
        public GameMode GameMode { get; set; }
        public Region Region { get; set; }
        public PlayerProfile Me { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void TriggerPropertyChanged()
        {
            // I'm too lazy to integrate this stuff for each property so we just trigger them all at once
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            foreach (var p in Players) {
                p.TriggerPropertyChanged();
            }
        }
    }
}
