using UnityEngine;

namespace CreativeMode
{
    public class ModuleBase : MonoBehaviour
    {
        protected virtual void ProvideImplementations() { }
        protected virtual void Initialize() { }
        protected virtual void Cleanup() { }

        private void Awake() => ProvideImplementations();
        private void Start() => Initialize();
        private void OnDestroy() => Cleanup();
    }
}