using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class AlbumCoverGraphicTinter : MonoBehaviour
    {
        private IMediaVisualizationProvider MusicVisualizer => Instance<IMediaVisualizationProvider>.Get();

        public Graphic[] tintBackground;
        public Graphic[] tintVibrant;
        public float animationDuration;

        public bool PauseTintUpdates
        {
            get => pauseTintUpdates;
            set
            {
                pauseTintUpdates = value;

                if (!pauseTintUpdates && !lastPaletteApplied)
                {
                    ApplyColors();
                }
            }
        }
        
        [SerializeField]
        private bool pauseTintUpdates;

        private bool lastPaletteApplied;
        private Palette currentPalette;
        
        private void OnNewPalette(Palette p)
        {
            currentPalette = p;

            if (enabled && !pauseTintUpdates)
            {
                lastPaletteApplied = true;
                ApplyColors();
            }
            else
            {
                lastPaletteApplied = false;
            }
        }

        private void Start()
        {
            MusicVisualizer.MusicPalette
                .Subscribe(OnNewPalette)
                .AddTo(this);
        }

        private void ApplyColors()
        {
            ApplyGraphicColor(tintBackground, currentPalette.BackgroundColor);
            ApplyGraphicColor(tintVibrant, currentPalette.VibrantColor);
        }

        private void ApplyGraphicColor(Graphic[] elements, Color color)
        {
            if (elements == null) 
                return;
            
            foreach (var e in elements)
            {
                e.DOColor(new Color(color.r, color.g, color.b, e.color.a), animationDuration);
            }
        }
    }
}