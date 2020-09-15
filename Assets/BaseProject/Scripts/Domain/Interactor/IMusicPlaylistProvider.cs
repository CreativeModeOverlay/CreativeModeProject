﻿using System.Collections.Generic;

namespace CreativeMode
{
    public interface IMusicPlaylistProvider
    {
        bool Shuffle { get; set; }
        bool SkipRepeats { get; set; }
        
        void ClearPlaylist();
        void AddToPlaylist(string url);
        void AddToPlaylist(ICollection<string> urls);

        void ResetQueueToPlaylist();
        void AddToQueue(string url);
        void AddToQueue(ICollection<string> urls);

        string AdvanceToNext();
        string ReturnToPrevious();
    }
}