using System;
using UniRx;
using UnityRawInput;

public interface IInputManager
{
    IObservable<Unit> OnHotkey(params RawKey[] keys);
}
