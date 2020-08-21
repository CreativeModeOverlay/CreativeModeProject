using System.Collections.Generic;
using SQLite;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class ChatStorage : IChatStorage
    {
        private SQLiteConnection connection;
        private Dictionary<string, Color> userColors 
            = new Dictionary<string, Color>();

        public ChatStorage(SQLiteConnection connection)
        {
            this.connection = connection;
            connection.CreateTable<ChatMessageDB>();
        }

        public void SaveChatMessage(ChatMessageDB message)
        {
            connection.Insert(message);
        }

        public Color32 GetUserColor(string userId)
        {
            if (userColors.TryGetValue(userId, out var color)) 
                return color;
            
            var dbColor = connection.Table<UserColor>()
                .FirstOrDefault(e => e.UserId == userId);
                
            color = new Color32(dbColor.R, dbColor.G, dbColor.B, dbColor.A);
            userColors[userId] = color;
            return color;
        }

        public void SetUserColor(string userId, Color32 color)
        {
            userColors[userId] = color;
            connection.InsertOrReplace(new UserColor
            {
                UserId = userId,
                R = color.r,
                G = color.g,
                B = color.b,
                A = color.a,
            });
        }
        
        private struct UserColor
        {
            [PrimaryKey]
            public string UserId { get; set; }
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public byte A { get; set; }
        }
    }
}