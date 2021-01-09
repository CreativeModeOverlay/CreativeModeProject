using System;
using UniRx;

namespace CreativeMode
{
    public interface IInputManager
    {
        IObservable<Unit> OnHotkeyPressed(params WindowsKeyCode[] keys);
        IObservable<bool> OnHotkey(params WindowsKeyCode[] keys);
    }
}