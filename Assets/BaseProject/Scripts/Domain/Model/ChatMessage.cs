using UnityEngine;

namespace CreativeMode
{
    public class ChatMessage
    {
        public string authorId;
        
        public string author;
        public Color authorColor;

        public SpannedText message;
        
        public bool hasMention;
        public bool isBroadcaster;
        public bool isModerator;

        public bool canDropOnDesktop;
    }
}