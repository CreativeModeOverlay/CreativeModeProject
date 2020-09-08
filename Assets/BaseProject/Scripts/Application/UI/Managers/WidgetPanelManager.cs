using UnityEngine;

namespace CreativeMode
{
    public class WidgetPanelManager : MonoBehaviour
    {
        public RectTransform[] trackedRects;
        public LayoutSnapshot[] layoutSnapshots;
        
        public class LayoutSnapshot
        {
            public string name;
            public RectSnapshot[] snapshots;
        }
        
        public struct RectSnapshot
        {
            public RectTransform rect;
            public Rect position;
            public bool isEnabled;
        }

        private void ApplyLayout(LayoutSnapshot layout)
        {
            var hasUpdates = false;
            var hasAdditions = false;
            var hasDeletions = false;

            foreach (var s in layout.snapshots)
            {
                
            }
        }
    }
}