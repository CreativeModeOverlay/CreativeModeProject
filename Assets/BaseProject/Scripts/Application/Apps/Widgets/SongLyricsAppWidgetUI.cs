using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SongLyricsAppWidgetUI : BaseAppWidgetUI<SongLyricsAppWidget>
{
    private IMusicPlayer MusicPlayer => Instance<IMusicPlayer>.Get();
    private IMediaVisualizationProvider MusicVisualizer => Instance<IMediaVisualizationProvider>.Get();
    
    public Text textLinePrefab;
    public RectTransform textRoot;
    
    [Header("Animation")]
    public float animationDuration;
    public Vector3[] lyricsPositions;
    public Vector3 lyricOffscreenOffset;
    public Gradient lyricsColors;
    public float displayOffset;
    
    [Header("Visuals")]
    public Text voiceText;
    public CanvasGroup voiceTextGroup;
    // TODO: put in manager
    public LyricFont[] fonts;
    public LyricVoice[] voices;

    private MediaMetadata currentMetadata;
    private CompositeDisposable compositeDisposable;
    private SongLyrics currentLyrics;
    private SongLyrics.Line currentLine;

    private Text currentLineObject;
    private List<Text> lineObjects = new List<Text>();

    private Color topLineColor = Color.white;

    private void OnEnable()
    {
        compositeDisposable = new CompositeDisposable();
        
        MusicPlayer.CurrentMedia.Subscribe(OnNewMusic).AddTo(compositeDisposable);

        MusicVisualizer.MusicPalette.Subscribe(p =>
        {
            topLineColor = p.VibrantColor;
            UpdateCurrentLineColor(currentLine);
        }).AddTo(compositeDisposable);
    }

    private void OnDisable()
    {
        compositeDisposable?.Dispose();

        for (var i = 0; i < lineObjects.Count; i++)
            Destroy(lineObjects[i]);

        lineObjects.Clear();
    }

    protected override void SetData(SongLyricsAppWidget data)
    {
        base.SetData(data);
        
        UpdateData();
    }

    private void OnDestroy()
    {
        compositeDisposable?.Dispose();
        DOTween.Kill(this);
    }

    private void OnNewMusic(MediaMetadata m)
    {
        ClearLyrics();

        currentMetadata = m;
        currentLine = null;
        UpdateData();
    }

    private void UpdateData()
    {
        /*var voiceName = Data.voice ?? "";
        var newLyrics = currentMetadata.lyrics?.FirstOrDefault(w => w.voice == voiceName);

        if (currentLyrics != newLyrics)
        {
            currentLyrics = newLyrics;
            voiceText.text = voices.FirstOrDefault(v => v.voiceName == voiceName)?.displayName ?? voiceName;
            
            currentLine = null;
            ClearLyrics();
        }*/
    }

    private void Update()
    {
        if(currentLyrics == null || !MusicPlayer.IsPlaying)
            return;

        var previousLine = currentLine;
        var time = MusicPlayer.Position - displayOffset;

        if (currentLine != null && !IsLineActive(currentLine, time))
            currentLine = null;

        if (currentLine == null)
            currentLine = FindLineForPosition(time);
        
        if (previousLine != currentLine)
            OnLineChanged(previousLine, currentLine);
    }

    private void OnLineChanged(SongLyrics.Line oldLine, SongLyrics.Line newLine)
    {
        if (newLine == null || string.IsNullOrWhiteSpace(newLine.text))
        {
            ClearLyrics();
        }
        else
        {
            AdvanceLyrics();
            AddNewLyricLine(newLine);
        }
    }

    private void AddNewLyricLine(SongLyrics.Line line)
    {
        var from = lyricsPositions[0];
        var to = lyricsPositions[1];
        var font = fonts.FirstOrDefault(f => f.name == line.font);

        var instance = Instantiate(textLinePrefab, textRoot);
        instance.gameObject.SetActive(true);
        instance.text = line.text;
        instance.transform.localPosition = from;

        if (font != null)
        {
            instance.font = font.font;
            instance.fontSize = (int) (instance.fontSize * font.scale);
            instance.resizeTextMinSize = (int) (instance.resizeTextMinSize * font.scale);
            instance.resizeTextMaxSize = (int) (instance.resizeTextMaxSize * font.scale);
            instance.alignByGeometry = font.alignByGeometry;
        }

        switch (line.position)
        {
            case SongLyrics.Line.Position.Left: instance.alignment = TextAnchor.UpperLeft; break;
            case SongLyrics.Line.Position.Center: instance.alignment = TextAnchor.UpperCenter; break;
            case SongLyrics.Line.Position.Right: instance.alignment = TextAnchor.UpperRight; break;
        }
        
        instance.transform.DOLocalMove(to, animationDuration)
            .SetId(this)
            .SetEase(Ease.InOutSine);

        if (voiceTextGroup)
        {
            DOTween.Kill(voiceTextGroup);
            voiceTextGroup.DOFade(0f, 0.25f);
        }
        
        currentLineObject = instance;
        UpdateCurrentLineColor(line);
        lineObjects.Insert(0, currentLineObject);
    }

    private void UpdateCurrentLineColor(SongLyrics.Line line)
    {
        if(!currentLineObject)
            return;
        
        var color = line.color ?? topLineColor;
        currentLineObject.color = lyricsColors.Evaluate(0) * color;
    }

    private void AdvanceLyrics()
    {
        if (lyricsPositions.Length > 1)
        {
            for (var i = 0; i < lineObjects.Count; i++)
            {
                var text = lineObjects[i];
                var targetPosition = lyricsPositions[i + 2];
                var isLast = i == lyricsPositions.Length - 3;

                text.DOColor(lyricsColors.Evaluate((float) (i + 3) / lyricsPositions.Length), animationDuration)
                    .SetId(this);
                
                var tween = text.transform.DOLocalMove(targetPosition, animationDuration)
                    .SetId(this)
                    .SetEase(Ease.InOutSine);
                
                if (isLast)
                {
                    lineObjects.RemoveAt(i);
                    tween.OnComplete(() => { Destroy(text.gameObject); });
                }
            }
        }
        else
        {
            Destroy(currentLineObject);
        }
    }

    private void ClearLyrics()
    {
        currentLineObject = null;

        float delay = 0f;
        
        for (var i = 0; i < lineObjects.Count; i++)
        {
            var text = lineObjects[i];
            var targetPosition = lyricsPositions[i + 1] + lyricOffscreenOffset;
            delay = ((lineObjects.Count - 1) - i) * 0.1f;

            text.transform.DOLocalMove(targetPosition, animationDuration)
                .SetId(this)
                .SetEase(Ease.InOutSine)
                .SetDelay(delay)
                .OnComplete(() => Destroy(text.gameObject));

            text.DOColor(lyricsColors.Evaluate(1f), animationDuration)
                .SetId(this)
                .SetEase(Ease.InOutSine)
                .SetDelay(delay);
        }

        if (voiceTextGroup)
        {
            DOTween.Kill(voiceTextGroup);
            voiceTextGroup.DOFade(1f, 1f)
                .SetDelay(delay + 0.25f);
        }

        lineObjects.Clear();
    }
    
    private SongLyrics.Line FindLineForPosition(float time)
    {
        for (var i = 0; i < currentLyrics.lines.Length; i++)
        {
            var line = currentLyrics.lines[i];

            if (IsLineActive(line, time))
                return line;
        }

        return null;
    }

    private bool IsLineActive(SongLyrics.Line line, float time)
    {
        return line.startTime <= time 
               && line.endTime > time;
    }

    [Serializable]
    public class LyricFont
    {
        public string name;
        public Font font;
        public float scale = 1f;
        public bool alignByGeometry;
    }
    
    [Serializable]
    public class LyricVoice
    {
        public string voiceName;
        public string displayName;
    }
}
