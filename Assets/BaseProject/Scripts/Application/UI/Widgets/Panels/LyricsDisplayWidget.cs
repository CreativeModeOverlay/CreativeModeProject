using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class LyricsDisplayWidget : MonoBehaviour
{
    private IMusicPlayer MusicPlayer => Instance<IMusicPlayer>.Get();
    private IMusicVisualizationProvider MusicVisualizer => Instance<IMusicVisualizationProvider>.Get();
    private ImageLoader ImageLoader => Instance<ImageLoader>.Get();

    public Text textLinePrefab;
    public float animationDuration;
    public Vector3[] lyricsPositions;
    public Vector3 lyricOffscreenOffset;
    public Gradient lyricsColors;
    public float displayOffset;
    public LyricFont[] fonts;

    // TODO: hacky, split voices in AudioMetadata
    public bool displayAllVoices;
    public string displayVoice;
    
    private CompositeDisposable compositeDisposable;
    private LyricLine[] currentLyrics;
    private LyricLine currentLine;

    private Text currentLineObject;
    private List<Text> lineObjects = new List<Text>();

    private Color topLineColor = Color.white;

    private void OnEnable()
    {
        compositeDisposable = new CompositeDisposable();
        
        MusicPlayer.CurrentMusic.Subscribe(OnNewMusic).AddTo(compositeDisposable);
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

    private void OnNewMusic(AudioMetadata m)
    {
        ClearLyrics();
        
        currentLyrics = m.lyrics;
        currentLine = null;
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

    private void OnLineChanged(LyricLine oldLine, LyricLine newLine)
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

    private void AddNewLyricLine(LyricLine line)
    {
        var from = lyricsPositions[0];
        var to = lyricsPositions[1];
        var font = fonts.FirstOrDefault(f => f.name == line.font);

        var instance = Instantiate(textLinePrefab, transform);
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
            case LyricLine.Position.Left: instance.alignment = TextAnchor.UpperLeft; break;
            case LyricLine.Position.Center: instance.alignment = TextAnchor.UpperCenter; break;
            case LyricLine.Position.Right: instance.alignment = TextAnchor.UpperRight; break;
        }

        instance.transform.DOLocalMove(to, animationDuration)
            .SetEase(Ease.InOutSine);
        
        currentLineObject = instance;
        UpdateCurrentLineColor(line);
        lineObjects.Insert(0, currentLineObject);
    }

    private void UpdateCurrentLineColor(LyricLine line)
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
                
                text.DOColor(lyricsColors.Evaluate((float) (i + 3) / lyricsPositions.Length), animationDuration);
                text.transform.DOLocalMove(targetPosition, animationDuration)
                    .SetEase(Ease.InOutSine);
                
                if (isLast)
                {
                    lineObjects.RemoveAt(i);
                    Destroy(text.gameObject, animationDuration);
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
        
        for (var i = 0; i < lineObjects.Count; i++)
        {
            var text = lineObjects[i];
            var targetPosition = lyricsPositions[i + 1] + lyricOffscreenOffset;
            var delay = ((lineObjects.Count - 1) - i) * 0.1f;

            text.transform.DOLocalMove(targetPosition, animationDuration)
                .SetEase(Ease.InOutSine)
                .SetDelay(delay);
            text.DOColor(lyricsColors.Evaluate(1f), animationDuration)
                .SetEase(Ease.InOutSine)
                .SetDelay(delay);

            Destroy(text.gameObject, animationDuration + delay);
        }
        
        lineObjects.Clear();
    }
    
    private LyricLine FindLineForPosition(float time)
    {
        // for multi voice lyrics
        LyricLine lastSuitableLine = null;
        
        for (var i = 0; i < currentLyrics.Length; i++)
        {
            var line = currentLyrics[i];
            
            if (ShowThisLine(line))
                lastSuitableLine = line;
            
            if (IsLineActive(line, time))
                return lastSuitableLine;
        }

        return null;
    }

    private bool ShowThisLine(LyricLine line)
    {
        return displayAllVoices || (line.voice ?? "") == (displayVoice ?? "");
    }

    private bool IsLineActive(LyricLine line, float time)
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
}
