using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public interface ILayoutWidget : IInterfaceWidget
    {
        float DefaultMinLabelWidth { get; set; }
        
        IToggleFieldWidget AddToggleField(string title = "");
        IIntFieldWidget AddIntField(string title = "");
        IFloatFieldWidget AddFloatField(string title = "");
        ITextFieldWidget AddTextField(string title = "");
        IVectorFieldWidget AddVector2Field(string title = "");
        IVectorFieldWidget AddVector3Field(string title = "");
        IVectorFieldWidget AddVector4Field(string title = "");
        IEnumFieldWidget AddEnumField<T>(string title = "") where T : Enum;
        
        ILinearLayoutWidget AddLayout(Orientation orientation);
        IGroupWidget AddGroup(string title = "");

        ISpaceWidget AddSpace();
        ISpaceWidget AddSpace(LayoutParams p);

        T AddField<T>(T prefab) where T : MonoBehaviour;
        T AddField<T>(Object prefab) where T : MonoBehaviour;
    }

    public interface ILinearLayoutWidget : ILayoutWidget
    {
        Orientation Orientation { get; set; }
    }

    public interface IGroupWidget : ILinearLayoutWidget
    {
        string Title { get; set; }
    }
}