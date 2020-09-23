using UnityEngine;

namespace CreativeMode
{
    public class WindowUI : MonoBehaviour, IWindowUI
    {
        [SerializeField] private UIContentSize windowSize = UIContentSize.GetDefault();
        
        public GameObject Root => gameObject;
        public UIContentSize Size => windowSize;
        
        public IWindowUIContainer Container { get; private set; }
        
        public virtual void OnAttached(IWindowUIContainer container)
        {
            Container = container;
        }

        public virtual void OnDetached()
        {
            Container = null;
        }
    }
}