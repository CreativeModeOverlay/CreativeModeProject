using UnityEngine.UI;

namespace CreativeMode
{
    public class SongLyricsWidgetEditorUI : BaseAppWidgetEditorUI<SongLyricsAppWidget>
    {
        public InputField voiceText;
        
        protected override void SetData(SongLyricsAppWidget widget)
        {
            voiceText.text = widget.voice;
        }

        protected override SongLyricsAppWidget GetData()
        {
            return new SongLyricsAppWidget
            {
                voice = voiceText.text
            };
        }
    }
}