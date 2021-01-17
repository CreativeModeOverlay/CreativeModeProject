using System.Linq;
using UnityEngine;

namespace CreativeMode.Impl
{
    internal class OverlayWidgetManager : MonoBehaviour, IOverlayWidgetManager
    {
        public OverlayWidgetUIPanel[] panels;
        
        public IOverlayWidgetUIPanel GetPanel(string panelId)
        {
            return panels.First(w => w.Id == panelId);
        }
    }
}