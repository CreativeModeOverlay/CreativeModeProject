using System.Text;
using UnityEngine.UI;

namespace CreativeMode
{
    public class StyledButton : BaseStyledElement<ButtonStyle>
    {
        public Button button;
        public Image background;
        public Text innerText;
        public Graphic[] innerGraphic;

        protected override void ApplyStyle(StringBuilder logger)
        {
            StyleUtils.ApplyButtonStyle(StyleUtils.CreateResolver(logger, Style), button);
            StyleUtils.ApplyImageStyle(StyleUtils.CreateResolver(logger, Style, r => r.backgroundStyle), background);
            StyleUtils.ApplyTextStyle(StyleUtils.CreateResolver(logger, Style, r => r.innerText), innerText); 
            StyleUtils.ApplyGraphicStyle(StyleUtils.CreateResolver(logger, Style, r => r.innerGraphic), innerGraphic);
        }
    }
}