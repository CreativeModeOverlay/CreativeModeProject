using System.Collections.Generic;
using SQLite;
using UnityEngine;
using Color = System.Drawing.Color;

namespace CreativeMode.Impl
{
    public class ChatStorage : IChatStorage
    {
        private SQLiteConnection connection;

        public ChatStorage(SQLiteConnection connection)
        {
            this.connection = connection;
            connection.CreateTable<ChatMessageDB>();
        }

        public void SaveChatMessage(ChatMessageDB message)
        {
            connection.Insert(message);
        }

        public List<ChatMessageDB> GetChatMessagesByAuthor(string authorId, int offset, int limit)
        {
            return connection.Table<ChatMessageDB>()
                .Where(m => m.AuthorId == authorId)
                .Skip(offset)
                .Take(limit)
                .OrderBy(d => d.Date)
                .ToList();
        }

        public Color32 GetAuthorColor(string userId)
        {
            var dbColor = connection.Table<UserColorDB>()
                .FirstOrDefault(e => e.AuthorId == userId);

            var color = Color.FromArgb(dbColor.ARGB);
            return new Color32(color.R, color.G, color.B, color.A);
        }

        public void SetAuthorColor(string userId, Color32 color)
        {
            connection.InsertOrReplace(new UserColorDB
            {
                AuthorId = userId,
                ARGB = Color.FromArgb(color.a, color.r, color.g, color.b).ToArgb()
            });
        }
    }
}