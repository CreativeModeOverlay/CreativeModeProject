using CreativeMode;
using UnityEngine;

public interface IChatStorage
{
    void SaveChatMessage(ChatMessageDB message);
    
    Color32 GetUserColor(string userId);
    void SetUserColor(string userId, Color32 color);
}
