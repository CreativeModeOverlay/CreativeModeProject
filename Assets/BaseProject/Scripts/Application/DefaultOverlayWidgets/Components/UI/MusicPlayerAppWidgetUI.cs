using CreativeMode;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayerAppWidgetUI : BaseAppWidgetUI<MusicPlayerAppWidget>
{
    private IMediaPlayer MusicPlayer => Instance<IMediaPlayer>.Get();
    
    public Text backgroundSongTitle;
    public Text backgroundSongInfo;
    public Text foregroundSongTitle;
    public Text foregroundSongInfo;

    public Image backgroundSongProgress;
    public Image foregroundSongProgress;
    
    public Image dimIconBackground;
    public Image pauseIcon;
    public Image bufferingIcon;

    public CreativeImage albumArtwork;
    public Sprite noArtworkDummy;
    public AspectRatioFitter albumArtworkAspectRatio;
    private bool progressAnimationActive;
    
    private Sprite coverSprite;
    private bool isMusicDisplaying;
    private float currentProgress;
    private bool currentIsPaused = true;
    private bool currentIsBuffering = true;

    private readonly object songChangeAnimateId = new object();
    private readonly object albumSlideAnimateId = new object();

    private void Awake()
    {
        MusicPlayer.CurrentMedia
            .Subscribe(AnimateSongChange)
            .AddTo(this);
    }
    
    private void Update()
    {
        UpdatePlaybackInfo();
        RotateBufferingIcon();
    }

    private void RotateBufferingIcon()
    {
        var bufferTransform = bufferingIcon.transform;
        var iconRotation = bufferTransform.eulerAngles;
        iconRotation.z += Time.deltaTime * 360;
        bufferTransform.eulerAngles = iconRotation;
    }
    
    private void OnDestroy()
    {
        DOTween.Kill(this);
        DOTween.Kill(albumSlideAnimateId);
        DOTween.Kill(songChangeAnimateId);
    }

    private void SetProgress(float progress)
    {
        backgroundSongProgress.fillAmount = 1 - progress;
        foregroundSongProgress.fillAmount = progress;
        currentProgress = progress;
    }

    private void UpdatePlaybackInfo()
    {
        if(!progressAnimationActive) 
            SetProgress(MusicPlayer.NormalizedPosition);

        var isPaused = !MusicPlayer.IsPlaying;
        var isBuffering = MusicPlayer.IsBuffering;
        var isDimShown = isPaused || isBuffering;
        var isPauseChanged = currentIsPaused != isPaused;
        var isBufferingChanged = currentIsBuffering != isBuffering;
        var isSomethingChanged = isPauseChanged || isBufferingChanged;

        if (isPauseChanged)
        {
            currentIsPaused = isPaused;
            pauseIcon.DOFade(isPaused ? 1f : 0f, 0.25f);
        }

        if (isBufferingChanged)
        {
            currentIsBuffering = isBuffering;
            bufferingIcon.DOFade(isBuffering ? 1f : 0f, 0.25f);
        }

        if (isSomethingChanged)
        {
            dimIconBackground.DOFade(isDimShown ? 0.5f : 0f, 0.25f);
        }
    }

    private void AnimateSongChange(MediaMetadata info)
    {
        DOTween.Kill(songChangeAnimateId);
        DOTween.Kill(albumSlideAnimateId);
        var sequence = DOTween.Sequence();
        var animateFinish = currentProgress < 0.95f;

        if (animateFinish)
        {
            sequence.Append(DOTween.To(SetProgress, currentProgress, 1f, 0.25f));
        }

        var titleLine = info.title;
        var infoLine = info.DottedInfoLine;

        progressAnimationActive = true;
        albumArtwork.PrefetchImage(info.thumbnailUrl);

        var initialAnimationDuration = animateFinish ? 0.25f : 0.5f;

        sequence.Append(DOTween.To(SetProgress, 1f, 0f, initialAnimationDuration)
                .SetEase(Ease.InSine)
                .OnStart(() =>
                {
                    backgroundSongTitle.text = titleLine;
                    backgroundSongInfo.text = infoLine;
                })
                .OnComplete(() =>
                {
                    progressAnimationActive = false;
                    foregroundSongTitle.text = titleLine;
                    foregroundSongInfo.text = infoLine;
                }))
            
            .Append(albumArtwork.transform.DOScaleX(0, 0.125f)
                .OnComplete(() =>
                {
                    albumArtwork.SetImage(info.thumbnailUrl, noArtworkDummy, s =>
                    {
                        var sprite = s.StaticImage;
                        albumArtworkAspectRatio.aspectRatio = sprite.rect.width / sprite.rect.height;

                        // cyclic move up/down or left/right if album cover is not square
                        DOTween.Sequence()
                            .Append(albumArtwork.rectTransform.DOPivot(new Vector2(0.5f, 0.5f), 5f))
                            .Append(albumArtwork.rectTransform.DOPivot(new Vector2(0, 0), 5f))
                            .Append(albumArtwork.rectTransform.DOPivot(new Vector2(1, 1), 10f))
                            .Append(albumArtwork.rectTransform.DOPivot(new Vector2(0.5f, 0.5f), 5f))
                            .SetId(albumSlideAnimateId)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Restart);
                    });
                }))
            .Append(albumArtwork.transform.DOScaleX(1, 0.125f))
            .Insert(0, albumArtwork.rectTransform.DOPivot(new Vector2(0.5f, 0.5f), initialAnimationDuration))
            .SetId(songChangeAnimateId)
            .Play();

        if (!isMusicDisplaying)
        {
            sequence.Complete(true);
            isMusicDisplaying = true;
        }
    }

    protected override void SetData(MusicPlayerAppWidget data)
    {
        // noop
    }
}
