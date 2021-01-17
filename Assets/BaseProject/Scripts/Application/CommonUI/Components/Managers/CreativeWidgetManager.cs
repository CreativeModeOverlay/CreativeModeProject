using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode.Impl
{
    /// <summary>
    /// Manager that handles extended widgets logic
    /// </summary>
    public class CreativeWidgetManager : MonoBehaviour
    {
        public static CreativeWidgetManager Instance { get; private set; }
        
        public int iconAtlasSize = 2048;
        public int iconSize = 64;

        private IconAtlas iconAtlas;
        private List<CreativeText> activeTexts;

        public Texture AtlasTexture => iconAtlas.Texture;
        
        private void Awake()
        {
            Instance = this;
            
            activeTexts = new List<CreativeText>();
            iconAtlas = new IconAtlas(iconAtlasSize, iconAtlasSize, 
                iconSize, iconSize);
        }

        private void Update()
        {
            if(activeTexts.Count > 0)
                iconAtlas.UpdateAnimation();
        }

        public void RegisterText(CreativeText text)
        {
            activeTexts.Add(text);
        }

        public void UnregisterText(CreativeText text)
        {
            activeTexts.Remove(text);
        }
        
        public Rect GetIcon(string url)
        {
            return iconAtlas.GetIcon(url);
        }
    }
}