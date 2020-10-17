using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class AppWidgetRegistry : MonoBehaviour, IAppWidgetRegistry
    {
        [Header("Builtin widgets info")]
        public BuiltInWidgetInfo musicPlayer;
        public BuiltInWidgetInfo captureDesktop;
        public BuiltInWidgetInfo captureDevice;
        public BuiltInWidgetInfo musicSpectrum;
        public BuiltInWidgetInfo musicWaveform;
        public BuiltInWidgetInfo songLyrics;
        public BuiltInWidgetInfo textNote;
        public BuiltInWidgetInfo timer;
        
        private Dictionary<Type, AppWidgetInfo> widgetInfos 
            = new Dictionary<Type, AppWidgetInfo>();

        private void Awake()
        {
            RegisterWidget<CaptureDesktopWidget>(captureDesktop);
            RegisterWidget<CaptureDeviceAppWidget>(captureDevice);
            RegisterWidget<MusicPlayerWidget>(musicPlayer);
            RegisterWidget<MusicSpectrumWidget>(musicSpectrum);
            RegisterWidget<MusicWaveformWidget>(musicWaveform);
            RegisterWidget<SongLyricsWidget>(songLyrics);
            RegisterWidget<TextNoteWidget>(textNote);
            RegisterWidget<TimerWidget>(timer);
        }

        public List<AppWidgetInfo> GetWidgets()
        {
            return widgetInfos.Values.ToList();
        }

        public AppWidgetInfo GetWidgetInfo(Type type)
        {
            if(widgetInfos.TryGetValue(type, out var info))
                return info;

            return default;
        }

        public void RegisterWidget<T>(string name, Sprite icon) 
            where T : AppWidget, new()
        {
            RegisterWidget(typeof(T), name, icon, () => new T());
        }

        public void RegisterWidget(Type type, string name, Sprite icon, Func<AppWidget> factory)
        {
            widgetInfos[type] = new AppWidgetInfo
            {
                dataType = type,
                name = name,
                icon = icon,
                widgetFactory = factory
            };
        }

        private void RegisterWidget<T>(BuiltInWidgetInfo info)
            where T : AppWidget, new()
        {
            RegisterWidget<T>(info.name, info.icon);
        }

        [Serializable]
        public struct BuiltInWidgetInfo
        {
            public string name;
            public Sprite icon;
        }
    }
}