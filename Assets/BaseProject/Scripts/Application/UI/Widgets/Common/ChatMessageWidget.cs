using System;
using CreativeMode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatMessageWidget : MonoBehaviour, IPointerClickHandler
{
    public Action OnRemove { get; set; }
    
    public CreativeText messageText;
    public Text messageAuthor;
    public GameObject isModeratorIcon;
    public GameObject isBroadcasterIcon;
    public Image flashForeground;
    public Image bottomLine;
    public CanvasGroup canvasGroup;
    
    private ChatMessage message;

    public void SetMessage(ChatMessage message)
    {
        this.message = message;
        
        var color = message.authorColor;
        color.a = 0;
        
        flashForeground.color = color;
        messageAuthor.text = message.author;
        messageAuthor.color = message.authorColor;
        
        messageText.SetText(message.message);
        
        isBroadcasterIcon.SetActive(message.isBroadcaster);
        isModeratorIcon.SetActive(message.isModerator);

        if (bottomLine)
        {
            bottomLine.color = message.authorColor;
        }
    }

    public void Flash(float duration)
    {
        var color = flashForeground.color;
        color.a = 1f;
        flashForeground.color = color;
        flashForeground.DOFade(0, duration);
    }

    public void Remove()
    {
        var color = flashForeground.color;
        color.a = 1f;
        flashForeground.color = color;
        canvasGroup.DOFade(0f, 0.5f);
        
        OnRemove?.Invoke();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Right && e.eligibleForClick)
        {
            Remove();
        }
    }
}
