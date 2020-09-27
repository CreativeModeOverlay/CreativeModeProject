using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    [ExecuteInEditMode]
    public abstract class BaseStyledElement<T> : MonoBehaviour, IStylePropertyProvider
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

        public List<ResolvedProperty> Properties { get; private set; }

#if UNITY_EDITOR
        protected void Update()
        {
            ApplyStyle();
        }
#endif

        private void ApplyStyle()
        {
            if (Application.isPlaying || !Style)
                return;

            if (Properties == null)
            {
                Properties = new List<ResolvedProperty>();
            }
            else
            {
                Properties.Clear();
            }

            ApplyStyle(Properties);
        }

        protected abstract void ApplyStyle(List<ResolvedProperty> outProperties);
    }

    public interface IStylePropertyProvider
    {
        List<ResolvedProperty> Properties { get; }
    }
}