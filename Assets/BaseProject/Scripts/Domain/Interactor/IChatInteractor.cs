﻿using System;
using UniRx;

namespace CreativeMode
{
    public interface IChatInteractor
    {
        IObservable<ChatMessage> ChatMessages { get; }
        IObservable<Unit> OnClearChatMessages { get; }

        void ClearChatMessages();
    }
}