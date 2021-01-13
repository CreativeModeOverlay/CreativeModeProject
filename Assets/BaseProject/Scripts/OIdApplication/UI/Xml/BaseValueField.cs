using UnityEngine;

namespace CreativeMode.UI
{
    public abstract class BaseValueField<T> : MonoBehaviour
    {
        public abstract T Value { get; set; }
        public abstract bool IsValid { get; }
    }
}