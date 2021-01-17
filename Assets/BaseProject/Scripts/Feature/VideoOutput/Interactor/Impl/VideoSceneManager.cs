using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CreativeMode
{
    [DefaultExecutionOrder(-100)]
    internal class VideoSceneManager : MonoBehaviour, IVideoSceneManager
    {
        public IVideoTransition DefaultTransition { get; set; }
        public IVideoLayerManager GlobalLayers => globalContainer;

        public int outputWidth;
        public int outputHeight;
        public int outputTimeout;
        public bool doubleBuffering;

        public GameObject defaultTransitionObject;
        public bool useHdr;
        public bool renderToEditorWindow;

        private RenderTexture renderTexture;
        private RenderTexture outputTexture;
        private UnityCapture.Interface captureInterface;

        private LayerContainer currentScene;
        private LayerContainer nextScene;
        private IVideoTransition sceneTransition;
        private LayerContainer globalContainer;

        private Dictionary<IVideoElement, LayerContainer> elementContainers = 
            new Dictionary<IVideoElement, LayerContainer>();

        private void OnDestroy()
        {
            Destroy(renderTexture);
            Destroy(outputTexture);
        }

        public IVideoLayerManager GetLayers(IVideoElement element)
        {
            return GetOrCreateLayerContainer(element);
        }

        public void Show(IVideoElement scene)
        {
            Show(scene, DefaultTransition);
        }

        public void Show(IVideoElement scene, IVideoTransition transition)
        {
            if (currentScene == null || transition == null)
            {
                currentScene = GetOrCreateLayerContainer(scene);
                currentScene.OnElementEnabled();
                globalContainer.Element = currentScene;
            }
            else
            {
                if (sceneTransition != null && !sceneTransition.IsTransitionFinished)
                {
                    if (currentScene.Element != scene)
                        currentScene.OnElementDisabled();

                    currentScene = nextScene;
                    sceneTransition.FinishTransition();
                }

                var isDifferentTransition = transition != sceneTransition;
                
                if(isDifferentTransition)
                    sceneTransition?.OnElementDisabled();

                nextScene = GetOrCreateLayerContainer(scene);
                nextScene.OnElementEnabled();
                
                sceneTransition = transition;
                sceneTransition.From = currentScene;
                sceneTransition.To = nextScene;

                if (isDifferentTransition)
                    sceneTransition.OnElementEnabled();
                
                sceneTransition.StartTransition();
                globalContainer.Element = sceneTransition;
            }
        }

        private void UpdateTransition()
        {
            if (sceneTransition != null && sceneTransition.IsTransitionFinished)
            {
                currentScene.OnElementDisabled();
                currentScene = nextScene;
                
                sceneTransition.OnElementDisabled();
                sceneTransition = null;

                globalContainer.Element = currentScene;
            }
        }

        private LayerContainer GetOrCreateLayerContainer(IVideoElement element)
        {
            if (elementContainers.TryGetValue(element, out var container))
            {
                return container;
            }

            var newContainer = new LayerContainer {Element = element};
            elementContainers[element] = newContainer;
            return newContainer;
        }

        private void Awake()
        {
            outputTexture = new RenderTexture(outputWidth, outputHeight, 24, RenderTextureFormat.ARGB32);

            renderTexture = useHdr
                ? new RenderTexture(outputWidth, outputHeight, 24, RenderTextureFormat.RGB111110Float, 
                    RenderTextureReadWrite.Default) { antiAliasing = 8 }
                : outputTexture;
            
            captureInterface = new UnityCapture.Interface(UnityCapture.ECaptureDevice.CaptureDevice1);
            globalContainer = new LayerContainer();
            globalContainer.OnElementEnabled();

            if (defaultTransitionObject)
                DefaultTransition = defaultTransitionObject.GetComponent<IVideoTransition>();

            if (!Application.isEditor || renderToEditorWindow)
            {
                Observable.EveryEndOfFrame()
                    .Subscribe(f => {
                        globalContainer?.Render(renderTexture);

                        if (renderTexture != outputTexture)
                            Graphics.Blit(renderTexture, outputTexture);
                    
                        captureInterface.SendTexture(outputTexture, outputTimeout, doubleBuffering);
                    });
            }
        }

        private void LateUpdate()
        {
            UpdateTransition();
        }

        private class LayerContainer : IVideoElement, IVideoLayerManager
        {
            public bool IsActive => isActive;
            
            public IVideoElement Element
            {
                get => element;
                set
                {
                    element = value;
                    UpdateElementChain();
                    UpdateActiveState();
                }
            }
            
            private IVideoElement element;
            private IVideoRenderer renderElement;
            private List<IVideoLayer> layers = new List<IVideoLayer>();
            private bool isEnabled;
            private bool isActive;

            public void OnElementEnabled()
            {
                if (!isEnabled)
                {
                    isEnabled = true;
                    UpdateActiveState();
                }
            }

            private void UpdateActiveState()
            {
                var currentIsActive = isEnabled && element != null;

                if (currentIsActive != isActive)
                {
                    isActive = currentIsActive;
                    
                    if (isActive)
                    {
                        element?.OnElementEnabled();
                        
                        for (var i = 0; i < layers.Count; i++)
                            layers[i].OnElementEnabled();
                    }
                    else
                    {
                        element?.OnElementDisabled();
                    
                        for (var i = 0; i < layers.Count; i++)
                            layers[i].OnElementDisabled();
                    }
                }
            }

            public void OnElementDisabled()
            {
                if (isEnabled)
                {
                    isEnabled = false;
                    UpdateActiveState();
                }
            }

            public bool Contains(IVideoRenderer e)
            {
                return element == e || layers.Contains(e);
            }

            public void AddLayer(IVideoLayer layer)
            {
                layers.Add(layer);
                UpdateElementChain();
                
                if (isActive)
                    layer.OnElementEnabled();
            }

            public void RemoveLayer(IVideoLayer layer)
            {
                layers.Remove(layer);
                UpdateElementChain();

                if (isActive)
                    layer.OnElementDisabled();
            }

            public void Render(RenderTexture target)
            {
                renderElement?.Render(target);
            }

            private void UpdateElementChain()
            {
                if (layers.Count == 0)
                {
                    renderElement = element;
                    return;
                }

                for (var i = layers.Count - 1; i > 0; i--)
                    layers[i].Background = layers[i - 1];

                layers[0].Background = element;
                renderElement = layers[layers.Count - 1];
            }
        }

        private void OnGUI()
        {
            if (Application.isEditor && renderToEditorWindow)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), outputTexture);
        }
    }
}