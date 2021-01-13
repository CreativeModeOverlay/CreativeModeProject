using System.Globalization;
using ThreeDISevenZeroR.XmlUI;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.UI
{
    public class VectorField : BaseValueField<Vector4>
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
                yField.text = value.y.ToString(CultureInfo.InvariantCulture);
                zField.text = value.z.ToString(CultureInfo.InvariantCulture);
                wField.text = value.w.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override bool IsValid => 
            float.TryParse(xField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(yField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(zField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _) &&
            float.TryParse(wField.text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);

        public void SetDimension(int count)
        {
            xField.GetComponent<XmlLayoutElement>().Visibility = (count >= 1) ? Visibility.Visible : Visibility.Gone;
            yField.GetComponent<XmlLayoutElement>().Visibility = (count >= 2) ? Visibility.Visible : Visibility.Gone;
            zField.GetComponent<XmlLayoutElement>().Visibility = (count >= 3) ? Visibility.Visible : Visibility.Gone;
            wField.GetComponent<XmlLayoutElement>().Visibility = (count >= 4) ? Visibility.Visible : Visibility.Gone;
        }
    }
}