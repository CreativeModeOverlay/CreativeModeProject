using System;
using UniRx;

namespace CreativeMode
{
    internal interface IChatProvider
    {
        IObservable<ChatMessageRemote> ChatMessages { get; }
        IObservable<ChatEventRemote> ChatEvents { get; }
        IObservable<Unit> OnChatCleared { get; }
    }
}