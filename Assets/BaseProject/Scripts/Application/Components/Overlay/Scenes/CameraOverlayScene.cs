using UnityEngine;
 
namespace CreativeMode
{
    public class CameraOverlayScene : MonoBehaviour, IOverlayElement
    {
        public Camera targetCamera;
        public GameObject[] enableWithScene;
        
        public bool IsElementEnabled { get; private set; }
    
        protected virtual void Awake()
        {
            targetCamera.enabled = false;
            SetObjectsEnabled(false);
        }

        public void Render(RenderTexture target)
        {
            targetCamera.targetTexture = target;
            targetCamera.Render();
        }

        public virtual void OnElementEnabled()
        {
            IsElementEnabled = true;
            SetObjectsEnabled(true);
        }

        public virtual void OnElementDisabled()
        {
            IsElementEnabled = false;
            SetObjectsEnabled(false);
        }

        private void SetObjectsEnabled(bool enabled)
        {
            foreach (var obj in enableWithScene)
            {
                obj.SetActive(enabled);
            }
        }
    }
}

