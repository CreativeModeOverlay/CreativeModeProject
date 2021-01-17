using UnityEngine;

namespace CreativeMode
{
    internal struct ChatMessageRemote
    {
        public string authorId;
        
        public string author;
        public Color authorColor;
        
        public string message;
        public Emote[] messageEmotes;

        public bool isBroadcaster;
        public bool isModerator;
        
        public struct Emote
        {
            public int startIndex;
            public int endIndex;
            public bool isModifier;
            public string url1x;
            public string url2x;
            public string url4x;
        }
    }
}