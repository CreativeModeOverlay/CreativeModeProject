using System.Collections.Generic;
using CreativeMode;
using UnityEngine;

namespace CreativeMode
{
    internal interface IChatStorage
    {
        void SaveChatMessage(ChatMessageDB chatMessage);
        List<ChatMessageDB> GetChatMessagesByAuthor(string authorId, int offset = 0, int limit = 25);

        Color32 GetAuthorColor(string userId);
        void SetAuthorColor(string userId, Color32 color);
    }
}
