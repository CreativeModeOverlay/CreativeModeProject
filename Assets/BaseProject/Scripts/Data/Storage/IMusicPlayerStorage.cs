using System;
using System.Collections.Generic;

public interface IMusicPlayerStorage
{
    int HistoryPosition { get; set; }
    int HistorySize { get; }
    bool Shuffle { get; set; }
    bool SkipRepeats { get; set; }

    void AddHistory(string url);
    List<string> GetHistory(int offset, int maxCount);
    String GetHistory(int offset);

    void AddPlayedSong(string url);
    List<string> GetPlayedSongs(TimeSpan duration);
    void RemoveFromPlayedSongs(List<string> songs);

    void ClearPlaylist();
    void AddToPlaylist(ICollection<string> items);
    List<string> GetPlaylist();

    void ClearQueue();
    void AddToQueue(ICollection<string> items);
    string PopQueue();
    bool IsQueueEmpty { get; }
}
