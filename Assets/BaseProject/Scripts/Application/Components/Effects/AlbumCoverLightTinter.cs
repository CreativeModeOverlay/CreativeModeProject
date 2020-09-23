using CreativeMode;
using DG.Tweening;
using UniRx;
using UnityEngine;

public class AlbumCoverLightTinter : MonoBehaviour
{
    private IMusicVisualizationProvider MusicPlayer => Instance<IMusicVisualizationProvider>.Get();
    
    public Light[] vibrantLights;
    public Light[] backgroundLights;

    private void OnEnable()
    {
        MusicPlayer.MusicPalette
            .Subscribe(p =>
            {
                foreach (var l in vibrantLights)
                    l.DOColor(p.VibrantColor, 1f);
                
                foreach (var l in backgroundLights)
                    l.DOColor(p.BackgroundColor, 1f);
            });
    }
}
