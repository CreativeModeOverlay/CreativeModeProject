using System.Collections.Generic;
using UnityEngine.UI;

namespace CreativeMode
{
    public class StyledButton : BaseStyledElement<ButtonStyle>
    {
        public Button button;
        public Image background;
        public Text innerText;
        public Graphic[] innerGraphic;

        protected override void ApplyStyle(List<ResolvedProperty> outProperties)
        {
            outProperties.AddRange(StyleUtils.ApplyButtonStyle(
                StyleUtils.CreateResolver(Style), button));
            
            outProperties.AddRange(StyleUtils.ApplyImageStyle(
                StyleUtils.CreateResolver(Style, r => r.backgroundStyle), background));
            
            outProperties.AddRange(StyleUtils.ApplyTextStyle(
                StyleUtils.CreateResolver(Style, r => r.innerText), innerText));
            
            outProperties.AddRange(StyleUtils.ApplyGraphicStyle(
                StyleUtils.CreateResolver(Style, r => r.innerGraphic), innerGraphic));
        }
    }
}