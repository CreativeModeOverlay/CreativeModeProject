using System.Collections.Generic;
using UniRx;

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
        void AddToQueue(int setId, MediaMetadata meta, int priority = 0);
        void AddToQueue(int setId, IEnumerable<MediaMetadata> meta, int priority = 0);

        MediaMetadata AdvanceToNext(int setId);
        MediaMetadata ReturnToPrevious();

        PlaylistSlice GetSlice(int setId, int maxPreviousCount, int maxNextCount);
    }
}