using CreativeMode.Impl;
using UnityRawInput;

namespace CreativeMode
{
    public class InputModule : ModuleBase
    {
        protected override void ProvideImplementations()
        {
            base.ProvideImplementations();
            Instance<IInputManager>.Bind(() => new InputManager());
        }

        protected override void Initialize()
        {
            base.Initialize();
            RawKeyInput.Start(true);
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            RawKeyInput.Stop();
        }
    }
}