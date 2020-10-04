using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CreativeMode.Impl
{
    public class WidgetRegistry : MonoBehaviour, IWidgetRegistry
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
        
        private Dictionary<Type, WidgetInfo> widgetInfos 
            = new Dictionary<Type, WidgetInfo>();

        private void Awake()
        {
            RegisterWidget<CaptureDesktopWidget>(captureDesktop);
            RegisterWidget<CaptureDeviceWidget>(captureDevice);
            RegisterWidget<MusicPlayerWidget>(musicPlayer);
            RegisterWidget<MusicSpectrumWidget>(musicSpectrum);
            RegisterWidget<MusicWaveformWidget>(musicWaveform);
            RegisterWidget<SongLyricsWidget>(songLyrics);
            RegisterWidget<TextNoteWidget>(textNote);
            RegisterWidget<TimerWidget>(timer);
        }

        public List<WidgetInfo> GetWidgets()
        {
            return widgetInfos.Values.ToList();
        }

        public WidgetInfo GetWidgetInfo(Type type)
        {
            if(widgetInfos.TryGetValue(type, out var info))
                return info;

            return default;
        }

        public void RegisterWidget<T>(string name, Sprite icon) 
            where T : Widget, new()
        {
            RegisterWidget(typeof(T), name, icon, () => new T());
        }

        public void RegisterWidget(Type type, string name, Sprite icon, Func<Widget> factory)
        {
            widgetInfos[type] = new WidgetInfo
            {
                dataType = type,
                name = name,
                icon = icon,
                widgetFactory = factory
            };
        }

        private void RegisterWidget<T>(BuiltInWidgetInfo info)
            where T : Widget, new()
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