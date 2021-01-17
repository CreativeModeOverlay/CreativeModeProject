using UnityEngine;

namespace CreativeMode
{
    public interface IWindowUIContainer
    {
        string Title { get; set; }
        Color BackgroundColor { get; set; }

        void Close();
    }
}