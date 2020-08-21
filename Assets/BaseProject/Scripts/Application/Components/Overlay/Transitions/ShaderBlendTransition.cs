﻿using System;
using DG.Tweening;
using UnityEngine;

namespace CreativeMode
{
    public class ShaderBlendTransition : MonoBehaviour, IOverlayTransition
    {
        public Shader shader;
        public float duration;

        public IOverlayRenderer From { get; set; }
        public IOverlayRenderer To { get; set; }

        public bool IsTransitionFinished => currentTransition >= 1;

        private Material material;
        private float currentTransition;

        private void Awake()
        {
            material = new Material(shader);
        }

        public void Render(RenderTexture target)
        {
            var rtFrom = RenderTexture.GetTemporary(target.descriptor);
            var rtTo = RenderTexture.GetTemporary(target.descriptor);

            From.Render(rtFrom);
            To.Render(rtTo);

            material.SetTexture(fromPropertyId.Value, rtFrom);
            material.SetTexture(toPropertyId.Value, rtTo);
            material.SetFloat(blendPropertyId.Value, currentTransition);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, 1, 1, 0);
            
            var lastActive = RenderTexture.active;
            RenderTexture.active = target;
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.blackTexture, material);
            RenderTexture.active = lastActive;
            
            GL.PopMatrix();

            RenderTexture.ReleaseTemporary(rtFrom);
            RenderTexture.ReleaseTemporary(rtTo);
        }

        public void OnElementEnabled()
        {
        }

        public void OnElementDisabled()
        {
        }

        public void StartTransition()
        {
            currentTransition = 0;
            DOTween.To(v => currentTransition = v, 0, 1, duration)
                .SetTarget(this);
        }

        public void FinishTransition()
        {
            DOTween.Complete(this);
        }

        private static readonly Lazy<int> fromPropertyId = new Lazy<int>(() =>
            Shader.PropertyToID("_FromTex"));

        private static readonly Lazy<int> toPropertyId = new Lazy<int>(() =>
            Shader.PropertyToID("_ToTex"));

        private static readonly Lazy<int> blendPropertyId = new Lazy<int>(() =>
            Shader.PropertyToID("_Blend"));
    }
}