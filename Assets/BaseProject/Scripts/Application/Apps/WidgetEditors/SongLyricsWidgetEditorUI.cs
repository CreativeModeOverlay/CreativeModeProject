using UnityEngine.UI;

namespace CreativeMode
{
    public class SongLyricsWidgetEditorUI : BaseAppWidgetEditorUI<SongLyricsWidget>
    {
        public InputField voiceText;
        
        protected override void SetData(SongLyricsWidget widget)
        {
            voiceText.text = widget.voice;
        }

        protected override SongLyricsWidget GetData()
        {
            return new SongLyricsWidget
            {
                voice = voiceText.text
            };
        }
    }
}