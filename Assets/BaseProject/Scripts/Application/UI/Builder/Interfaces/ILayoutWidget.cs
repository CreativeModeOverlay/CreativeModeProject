using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public interface ILayoutWidget : IInterfaceWidget
    {
        IToggleFieldWidget AddToggleField(string title = "");
        IIntFieldWidget AddIntField(string title = "");
        IFloatFieldWidget AddFloatField(string title = "");
        ITextFieldWidget AddTextField(string title = "");
        IVectorFieldWidget AddVector2Field(string title = "");
        IVectorFieldWidget AddVector3Field(string title = "");
        IVectorFieldWidget AddVector4Field(string title = "");
        IEnumFieldWidget AddEnumField<T>(string title = "") where T : Enum;

        ILayoutWidget AddHorizontalGroup();
        ILayoutWidget AddVerticalGroup();

        IGroupWidget AddGroup(string title = "");

        T AddField<T>(T prefab) where T : MonoBehaviour;
        T AddField<T>(Object prefab) where T : MonoBehaviour;
    }

    public interface IGroupWidget : ILayoutWidget
    {
        string Title { get; set; }
    }
}