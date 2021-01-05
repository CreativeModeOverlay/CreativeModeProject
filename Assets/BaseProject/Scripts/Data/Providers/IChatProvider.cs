using System;
using UniRx;

namespace CreativeMode
{
    public interface IChatProvider
    {
        IObservable<ChatMessageRemote> ChatMessages { get; }
        IObservable<ChatEventRemote> ChatEvents { get; }
        IObservable<Unit> OnChatCleared { get; }
    }
}