﻿using System;
using UniRx;
using UnityEngine;
using UnityRawInput;

public class InputManager : IInputManager
{
    public IObservable<Unit> OnHotkeyPressed(params RawKey[] keys)
    {
        return Observable.CreateSafe<Unit>(o =>
        {
            bool KeyDownEvent(RawKey k)
            {
                if (RawKeyInput.PressedKeysCount != keys.Length) 
                    return false;

                for (var i = 0; i < keys.Length; i++)
                {
                    if (!RawKeyInput.IsKeyDown(keys[i])) 
                        return false;
                }

                o.OnNext(Unit.Default);
                return true;
            }

            RawKeyInput.AddKeyDownListener(KeyDownEvent);
            return Disposable.Create(() =>
            {
                RawKeyInput.RemoveKeyDownListener(KeyDownEvent);
            });
        });
    }

    public IObservable<bool> OnHotkey(params RawKey[] keys)
    {
        return Observable.CreateSafe<bool>(o =>
        {
            bool KeyUpdatedEvent()
            {
                foreach (var t in keys)
                {
                    if (!RawKeyInput.IsKeyDown(t))
                    {
                        o.OnNext(false);
                        return false;
                    }
                }

                o.OnNext(true);
                return true;
            }

            bool KeyDownEvent(RawKey k) => KeyUpdatedEvent();
            bool KeyUpEvent(RawKey k) => KeyUpdatedEvent();

            RawKeyInput.AddKeyDownListener(KeyDownEvent);
            RawKeyInput.AddKeyUpListener(KeyUpEvent);
            return Disposable.Create(() =>
            {
                RawKeyInput.RemoveKeyDownListener(KeyDownEvent);
                RawKeyInput.RemoveKeyUpListener(KeyUpEvent);
            });
        }).StartWith(false).DistinctUntilChanged();
    }
}
