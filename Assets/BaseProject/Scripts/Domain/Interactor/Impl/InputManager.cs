using System;
using UniRx;
using UnityRawInput;

public class InputManager : IInputManager
{
    public IObservable<Unit> OnHotkey(params RawKey[] keys)
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

            RawKeyInput.OnKeyDown += KeyDownEvent;
            return Disposable.Create(() =>
            {
                RawKeyInput.OnKeyDown -= KeyDownEvent;
            });
        });
    }
}
