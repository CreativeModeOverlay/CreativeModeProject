using System.Collections.Generic;

namespace CreativeMode
{
    public interface IMediaPlaylistProvider
    {
        bool Shuffle { get; set; }
        bool SkipRepeats { get; set; }
        int CurrentSet { get; set; }
        
        void ClearPlaylist(int setId);
        void AddToPlaylist(int setId, MediaMetadata meta);
        void AddToPlaylist(int setId, IEnumerable<MediaMetadata> meta);

        void ResetQueueToPlaylist(int setId);
        void AddToQueue(int setId, MediaMetadata meta);
        void AddToQueue(int setId, IEnumerable<MediaMetadata> meta);
        
        MediaMetadata AdvanceToNext(int setId);
        MediaMetadata ReturnToPrevious();
        
        PlaylistSlice GetSlice(int setId, int maxPreviousCount, int maxNextCount);
    }
}