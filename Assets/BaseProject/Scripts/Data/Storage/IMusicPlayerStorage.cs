using System;
using System.Collections.Generic;
using CreativeMode.Impl;

namespace CreativeMode
{
    public interface IMusicPlayerStorage
    {
        int CurrentSetId { get; set; }
        
        int HistoryPosition { get; set; }
        int HistorySize { get; }
        
        bool Shuffle { get; set; }
        bool SkipRepeats { get; set; }

        // For tracking actually played media (like history, but if music actually played)
        void AddPlayedMedia(string url);
        List<string> GetPlayedMedia(TimeSpan duration);
        void RemoveFromPlayedMedia(IEnumerable<string> songs);
        
        // Static collection of music
        void ClearPlaylist(int setId);
        void AddToPlaylist(IEnumerable<PlaylistEntryDB> items);
        List<PlaylistEntryDB> GetPlaylist(int setId);
        
        // For keeping playback history, 
        void AddHistory(HistoryEntryDB entry);
        List<HistoryEntryDB> GetHistory(int offset, int maxCount);
        HistoryEntryDB GetHistoryAt(int offset);

        // Mutable list of music that will be played
        void ClearQueue(int setId);
        void AddToQueue(IEnumerable<QueueEntryDB> items);
        List<QueueEntryDB> GetQueue(int setId, int offset = 0, int size = 25);
        QueueEntryDB PopQueue(int setId);
        QueueEntryDB PeekQueue(int setId);
        int GetQueueSize(int setId);
    }
}