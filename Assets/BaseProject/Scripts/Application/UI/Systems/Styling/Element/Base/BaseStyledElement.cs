using System.Text;
using UnityEngine;

namespace CreativeMode
{
    [ExecuteInEditMode]
    public abstract class BaseStyledElement<T> : MonoBehaviour, IResolveLogProvider
        where T : ScriptableObject
    {
        [SerializeField] private T style;

        public T Style
        {
            get => style;
            set
            {
                style = value;
                ApplyStyle();
            }
        }

        public string ResolverLog { get; private set; }

#if UNITY_EDITOR
        protected void Update()
        {
            ApplyStyle(true);
        }
#endif

        private void ApplyStyle(bool log = false)
        {
            if (Application.isPlaying || !Style)
                return;

            var builder = log ? new StringBuilder() : null;
            ApplyStyle(builder);
            ResolverLog = builder?.ToString(); 
        }

        protected abstract void ApplyStyle(StringBuilder logger);

    }

    public interface IResolveLogProvider
    {
        string ResolverLog { get; }
    }
}