using UnityEngine;

namespace CreativeMode
{
    public interface IVideoRenderer
    {
        void Render(RenderTexture target);
    }
}