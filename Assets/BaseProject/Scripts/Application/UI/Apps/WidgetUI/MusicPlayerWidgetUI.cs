﻿using System;
using CreativeMode;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayerWidgetUI : BaseWidgetUI<MusicPlayerWidget>
{
    public Text backgroundSongTitle;
    public Text backgroundSongInfo;
    public Text foregroundSongTitle;
    public Text foregroundSongInfo;
    public AlbumCoverGraphicTinter albumCoverTinter;

    public Image backgroundSongProgress;
    public Image foregroundSongProgress;
    public CanvasGroup pauseIcon;

    public CreativeImage albumArtwork;
    public Sprite noArtworkDummy;
    public AspectRatioFitter albumArtworkAspectRatio;
    private bool progressAnimationActive;
    
    private Sprite coverSprite;
    private bool isMusicDisplaying;
    private float currentProgress;
    private bool currentIsPaused = true;

    private readonly object songChangeAnimateId = new object();
    private readonly object albumSlideAnimateId = new object();

    private IMusicPlayer MusicPlayer => Instance<IMusicPlayer>.Get();

    private void Awake()
    {
        MusicPlayer.CurrentMusic
            .Subscribe(AnimateSongChange)
            .AddTo(this);
    }
    
    private void Update()
    {
        UpdatePlaybackInfo();
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
        var isPauseChanged = currentIsPaused != isPaused;

        if (isPauseChanged)
        {
            currentIsPaused = isPaused;
            pauseIcon.DOFade(isPaused ? 1f : 0f, 0.25f);
        }
    }

    private void AnimateSongChange(AudioMetadata info)
    {
        if (albumCoverTinter != null)
            albumCoverTinter.PauseTintUpdates = true;
        
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
        albumArtwork.PreloadImage(info.coverUrl);

        var initialAnimationDuration = animateFinish ? 0.25f : 0.5f;

        sequence.Append(
                DOTween.To(SetProgress, 1f, 0f, initialAnimationDuration)
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
                    if (albumCoverTinter != null)
                        albumCoverTinter.PauseTintUpdates = false;
                    
                    albumArtwork.SetImage(info.coverUrl, noArtworkDummy, s =>
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

    protected override void SetData(MusicPlayerWidget data)
    {
        // noop
    }
}
