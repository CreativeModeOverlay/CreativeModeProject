using System;
using UniRx;

namespace CreativeMode
{
    public interface IChatClient
    {
        IObservable<ChatMessageRemote> ChatMessages { get; }
        IObservable<Unit> OnChatCleared { get; }
    }
}