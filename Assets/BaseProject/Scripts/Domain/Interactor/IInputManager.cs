﻿using System;
using UniRx;
using UnityRawInput;

public interface IInputManager
{
    IObservable<Unit> OnHotkeyPressed(params RawKey[] keys);
    IObservable<bool> OnHotkey(params RawKey[] keys);
}
