using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    public interface IFieldWidget<T> : IInterfaceWidget
    {
        string Title { get; set; }
        string Subtitle { get; set; }
        
        T Value { get; set; }
        
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        bool IsInputValid { get; }

        T ValueOrDefault(T defaultValue = default);
    }

    public interface IToggleFieldWidget : IFieldWidget<bool> { }
    
    public interface IInputFieldWidget<T> : IFieldWidget<T>
    {
        string Placeholder { get; set; }
    }
    
    public interface IIntFieldWidget : IInputFieldWidget<int> { }
    public interface IFloatFieldWidget : IInputFieldWidget<float> { }
    public interface ITextFieldWidget : IInputFieldWidget<string> { }
    public interface IVectorFieldWidget : IFieldWidget<Vector4> { }

    public interface IDropdownFieldWidget : IFieldWidget<string>
    {
        string[] Values { get; set; }
    }

    public interface IEnumFieldWidget : IFieldWidget<Enum>
    {
        Dictionary<Enum, string> DisplayNames { get; set; }
    }
}