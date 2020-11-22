using ThreeDISevenZeroR.XmlUI;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode.UI
{
    [CreateAssetMenu]
    public class CreativeModeElements : ElementCollection
    {
        protected override void RegisterElements()
        {
            AddComponent(new XmlComponentInfo("DragAndDropSource")
                .AddComponent<DragAndDropSource>());
            
            AddComponent(new XmlComponentInfo("DragAndDropReceiver")
                .AddComponent<DragAndDropReceiver>());
            
            AddComponent(new XmlComponentInfo("Rigidbody2D")
                .AddComponent<Rigidbody2D>());
            
            AddComponent(new XmlComponentInfo("UiCollider"));
            
            AddComponent(new XmlComponentInfo("ChatMessageController"));

            AddComponent(new XmlComponentInfo("TextField")
                .AddComponent<TextField>(new AttributeHandler<TextField>()
                    .AddComponentReferenceAttr<InputField>("FieldId", (e, c, v) => c.inputField = v)
                    .AddStringAttr("Value", (e, c, v) => c.Value = v)));
            
            AddComponent(new XmlComponentInfo("ToggleField")
                .AddComponent<BooleanField>(new AttributeHandler<BooleanField>()
                    .AddComponentReferenceAttr<Toggle>("ToggleId", (e, c, v) => c.toggle = v)
                    .AddBoolAttr("Value", (e, c, v) => c.Value = v)));
            
            AddComponent(new XmlComponentInfo("IntField")
                .AddComponent<IntField>(new AttributeHandler<IntField>()
                    .AddComponentReferenceAttr<InputField>("FieldId", (e, c, v) => c.inputField = v)
                    .AddIntAttr("Value", (e, c, v) => c.Value = v)));
            
            AddComponent(new XmlComponentInfo("FloatField")
                .AddComponent<FloatField>(new AttributeHandler<FloatField>()
                    .AddComponentReferenceAttr<InputField>("FieldId", (e, c, v) => c.inputField = v)
                    .AddFloatAttr("Value", (e, c, v) => c.Value = v)));
            
            AddComponent(new XmlComponentInfo("VectorField")
                .AddComponent<VectorField>(new AttributeHandler<VectorField>()
                    .AddComponentReferenceAttr<InputField>("XFieldId", (e, c, v) => c.xField = v)
                    .AddComponentReferenceAttr<InputField>("YFieldId", (e, c, v) => c.yField = v)
                    .AddComponentReferenceAttr<InputField>("ZFieldId", (e, c, v) => c.zField = v)
                    .AddComponentReferenceAttr<InputField>("WFieldId", (e, c, v) => c.wField = v)
                    .AddIntAttr("Dimension", (e, c, v) => c.SetDimension(v))));
        }
    }
}