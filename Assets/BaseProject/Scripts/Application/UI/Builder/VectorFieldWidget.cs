using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public class VectorFieldWidget : BaseFieldWidget<Vector4>, IVectorFieldWidget
    {
        public InputField xField;
        public InputField yField;
        public InputField zField;
        public InputField wField;

        public override Vector4 Value
        {
            get
            {
                float.TryParse(xField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x);
                float.TryParse(yField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y);
                float.TryParse(zField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var z);
                float.TryParse(wField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var w);
                return new Vector4(x, y, z, w);
            }
            set
            {
                xField.text = value.x.ToString(CultureInfo.InvariantCulture);
                yField.text = value.x.ToString(CultureInfo.InvariantCulture);
                zField.text = value.x.ToString(CultureInfo.InvariantCulture);
                wField.text = value.x.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override bool IsInputValid => 
            float.TryParse(xField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(yField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(zField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(wField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);

        public VectorFieldWidget SetFieldCount(int count)
        {
            xField.gameObject.SetActive(count >= 1);
            yField.gameObject.SetActive(count >= 2);
            zField.gameObject.SetActive(count >= 3);
            wField.gameObject.SetActive(count >= 4);
            return this;
        }
    }
}