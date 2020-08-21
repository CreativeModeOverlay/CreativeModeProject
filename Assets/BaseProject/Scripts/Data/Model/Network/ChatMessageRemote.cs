﻿using UnityEngine;

namespace CreativeMode
{
    public struct ChatMessageRemote
    {
        public string authorId;
        
        public string author;
        public Color authorColor;
        
        public string message;
        public Emote[] messageEmotes;
        public string rawMessage;
        
        public bool hasMention;
        public bool isBroadcaster;
        public bool isModerator;
        
        public struct Emote
        {
            public int position;
            public bool isModifier;
            public string url1x;
            public string url2x;
            public string url4x;
        }
    }
}