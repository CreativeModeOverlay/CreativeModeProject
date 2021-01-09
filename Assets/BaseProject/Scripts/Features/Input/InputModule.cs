using CreativeMode.Impl;
using UnityRawInput;

namespace CreativeMode
{
    public static class InputModule
    {
        public static void Init()
        {
            RawKeyInput.Start(true);
            
            Instance<IInputManager>.Bind(() => new InputManager());
        }
    }
}