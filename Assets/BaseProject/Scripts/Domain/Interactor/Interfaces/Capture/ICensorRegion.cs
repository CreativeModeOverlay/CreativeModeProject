using UnityEngine;

namespace CreativeMode
{
    public interface ICensorRegion
    {
        string Title { get; }
        Rect Rect { get; }
    }
}