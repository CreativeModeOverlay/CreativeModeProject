using System;
using System.Collections.Generic;
using System.Linq;
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
            connection.CreateTable<ChatMessage>();
        }

        public void SaveChatMessage(ChatMessageDB message)
        {
            connection.Insert(new ChatMessage
            {
                AuthorId = message.authorId,
                AuthorName = message.authorName,
                Message = message.message,
                Date = message.date
            });
        }

        public List<ChatMessageDB> GetChatMessagesByAuthor(string authorId, int offset, int limit)
        {
            return connection.Table<ChatMessage>()
                .Where(m => m.AuthorId == authorId)
                .Skip(offset)
                .Take(limit)
                .OrderBy(d => d.Date)
                .Select(m => new ChatMessageDB
                {
                    authorId = m.AuthorId,
                    authorName = m.AuthorName,
                    message = m.Message,
                    date = m.Date
                }).ToList();
        }

        public Color32 GetAuthorColor(string userId)
        {
            if (userColors.TryGetValue(userId, out var color)) 
                return color;
            
            var dbColor = connection.Table<UserColor>()
                .FirstOrDefault(e => e.AuthorId == userId);
                
            color = new Color32(dbColor.R, dbColor.G, dbColor.B, dbColor.A);
            userColors[userId] = color;
            return color;
        }

        public void SetAuthorColor(string userId, Color32 color)
        {
            userColors[userId] = color;
            connection.InsertOrReplace(new UserColor
            {
                AuthorId = userId,
                R = color.r,
                G = color.g,
                B = color.b,
                A = color.a,
            });
        }
        
        public class ChatMessage
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public DateTime Date { get; set; }
        
            public string AuthorName { get; set; }
            public string AuthorId { get; set; }
        
            public string Message { get; set; }
        }

        private struct UserColor
        {
            [PrimaryKey]
            public string AuthorId { get; set; }
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
            public byte A { get; set; }
        }
    }
}