using System;
using UniRx;

namespace CreativeMode
{
    public interface IChatProvider
    {
        IObservable<ChatMessageRemote> ChatMessages { get; }
        IObservable<Unit> OnChatCleared { get; }
    }
}