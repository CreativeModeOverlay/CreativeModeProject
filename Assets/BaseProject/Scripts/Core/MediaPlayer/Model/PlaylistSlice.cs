using System.Collections.Generic;

namespace CreativeMode
{
    public struct PlaylistSlice
    {
        public List<MediaMetadata> previous;
        public MediaMetadata current;
        public List<MediaMetadata> next;
    }
}