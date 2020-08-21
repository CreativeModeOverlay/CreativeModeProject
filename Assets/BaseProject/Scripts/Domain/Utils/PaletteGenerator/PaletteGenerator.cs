using System;
using System.Collections.Generic;
using System.Linq;
using CreativeMode;
using UnityEngine;
using UnityEngine.Rendering;

public static class PaletteGenerator
{
    public static void FromTexture(Texture t, Action<Palette> onPaletteLoaded)
    {
        if(onPaletteLoaded == null)
            return;

        if (texturePaletteCache.TryGetValue(t, out var existing))
        {
            onPaletteLoaded(existing);
            return;
        }

        if (paletteExtractionTasks.TryGetValue(t, out var listenerList))
        {
            listenerList.Add(onPaletteLoaded);
            return;
        }

        listenerList = new List<Action<Palette>>(1) { onPaletteLoaded };
        paletteExtractionTasks[t] = listenerList;

        Init();
        
        var xPixels = Mathf.Min(t.width, 64);
        var yPixels = Mathf.Min(t.height, 64);
        var colorCount = xPixels * yPixels;
        var downSampleStep = new Vector2((float) t.width / xPixels, (float) t.height / yPixels);
        
        var colorBuffer = new ComputeBuffer(colorCount, 16);
        var colorClusterBuffer = new ComputeBuffer(colorCount, 4);
        var clusterBuffer = new ComputeBuffer(clusterCount, 52);
        clusterBuffer.SetData(initialEntries);
        
        compute.SetInt(textureWidthId, t.width);
        compute.SetInt(textureHeightId, t.height);
        compute.SetInt(bufferWidthId, xPixels);
        compute.SetInt(colorBufferSizeId, colorCount);

        compute.SetVector(downSampleStepId, downSampleStep);
        compute.SetTexture(convertTextureKernel, inputTextureId, t);
        compute.SetBuffer(convertTextureKernel, colorBufferId, colorBuffer);
        
        compute.SetInt(clusterCountId, clusterCount);

        compute.SetBuffer(computeClustersKernel, colorBufferId, colorBuffer);
        compute.SetBuffer(computeClustersKernel, colorClusterBufferId, colorClusterBuffer);
        compute.SetBuffer(computeClustersKernel, clusterBufferId, clusterBuffer);
        
        compute.SetBuffer(moveClustersKernel, colorBufferId, colorBuffer);
        compute.SetBuffer(moveClustersKernel, colorClusterBufferId, colorClusterBuffer);
        compute.SetBuffer(moveClustersKernel, clusterBufferId, clusterBuffer);

        var computeGroupCount = Mathf.CeilToInt((float) colorCount / computeClustersKernelXGroup);
        var clusterGroupCount = Mathf.CeilToInt((float) clusterCount / moveClustersKernelXGroup);

        compute.Dispatch(convertTextureKernel, 
            Mathf.CeilToInt((float) xPixels / convertTextureKernelXGroup),
            Mathf.CeilToInt((float) yPixels / convertTextureKernelYGroup), 1);

        for (var i = 0; i < 16; i++)
        {
            compute.Dispatch(computeClustersKernel, computeGroupCount, 1, 1);
            compute.Dispatch(moveClustersKernel, clusterGroupCount, 1, 1);
        }

        AsyncGPUReadback.Request(clusterBuffer, r =>
        {
            var result = r.GetData<Palette.Swatch>().Select(s => new Palette.Swatch
            {
                count = s.count,
                center = s.center.gamma,
                darkest = s.darkest.gamma,
                lightest = s.lightest.gamma
            }).ToArray();

            var palette = CreatePalette(colorCount, result);

            //texturePaletteCache[t] = palette;
            if (paletteExtractionTasks.TryGetValue(t, out listenerList))
            {
                foreach (var listener in listenerList)
                {
                    listener(palette);
                }
            }

            paletteExtractionTasks.Remove(t);

            colorBuffer.Release();
            colorClusterBuffer.Release();
            clusterBuffer.Release();
        });
    }
    
    private static Palette CreatePalette(int pixelCount, Palette.Swatch[] swatches)
    {
        var backgroundColor = swatches.OrderBy(s => s.center.grayscale).First().center;
        
        Color.RGBToHSV(backgroundColor, out var bgH, out _, out _);
        
        // Никакого смысла нет, просто значения подогнанные чтобы нормально смотрелось
        var vibrantSwatch = swatches.OrderByDescending(sw =>
        {
            if ((float) sw.count / pixelCount < 0.01f)
                return -1;

            Color.RGBToHSV(sw.center, out var h, out var s, out var v);
            var bw = sw.center.grayscale;
            
            var saturationScore = (s - 0.6f) + (v - 0.6f);
            var clearScore = (0.41 - Mathf.PingPong(h, 0.0833f) * 6f);
            var diffScore = (180 - Mathf.Abs(Mathf.DeltaAngle(bgH * 360, h * 360))) / 720f;
            var brightScore = bw / 2;
            return saturationScore + clearScore + diffScore + brightScore;
        }).First();

        var vibrantGrayscale = vibrantSwatch.center.grayscale;
        
        // Если слишком тёмный или слишком похож по яркости на фон, берём светлый цвет
        var vibrantColor = vibrantGrayscale < 0.3f || Mathf.Abs(backgroundColor.grayscale - vibrantGrayscale) < 0.1f 
            ? Color.Lerp(vibrantSwatch.lightest, Color.white, 0.5f)
            : vibrantSwatch.center;

        return new Palette(swatches, vibrantColor, backgroundColor);
    }

    private static Dictionary<Texture, List<Action<Palette>>> paletteExtractionTasks = new Dictionary<Texture, List<Action<Palette>>>();
    private static Dictionary<Texture, Palette> texturePaletteCache = new Dictionary<Texture, Palette>();

    // compute
    private static bool isInitialized;
    private static ComputeShader compute;
    private static int textureWidthId;
    private static int textureHeightId;
    private static int bufferWidthId;
    private static int inputTextureId;
    private static int downSampleStepId;
    private static int clusterCountId;
    private static int colorBufferId;
    private static int colorBufferSizeId;
    private static int colorClusterBufferId;
    private static int clusterBufferId;
    private static int clusterMovedId;

    private static int convertTextureKernel;
    private static int computeClustersKernel;
    private static int moveClustersKernel;

    private static uint convertTextureKernelXGroup;
    private static uint convertTextureKernelYGroup;
    private static uint computeClustersKernelXGroup;
    private static uint moveClustersKernelXGroup;

    private const int clusterCount = 8;

    private static Palette.Swatch[] initialEntries = new Palette.Swatch[clusterCount]
    {
        new Palette.Swatch { center = new Color(0.25f, 0.25f, 0.25f) }, 
        new Palette.Swatch { center = new Color(0.75f, 0.25f, 0.25f) }, 
        new Palette.Swatch { center = new Color(0.25f, 0.75f, 0.25f) }, 
        new Palette.Swatch { center = new Color(0.75f, 0.75f, 0.25f) }, 
        new Palette.Swatch { center = new Color(0.25f, 0.25f, 0.75f) }, 
        new Palette.Swatch { center = new Color(0.75f, 0.25f, 0.75f) }, 
        new Palette.Swatch { center = new Color(0.25f, 0.75f, 0.75f) }, 
        new Palette.Swatch { center = new Color(0.75f, 0.75f, 0.75f) }, 
    };

    private static void Init()
    {
        if(isInitialized)
            return;

        compute = Resources.Load<ComputeShader>("PaletteGeneratorCompute");
        convertTextureKernel = compute.FindKernel("ConvertTextureToBuffer");
        computeClustersKernel = compute.FindKernel("ComputeClusters");
        moveClustersKernel = compute.FindKernel("MoveClusters");
        
        compute.GetKernelThreadGroupSizes(convertTextureKernel, 
            out convertTextureKernelXGroup, 
            out convertTextureKernelYGroup, 
            out _);
        
        compute.GetKernelThreadGroupSizes(computeClustersKernel, 
            out computeClustersKernelXGroup, 
            out _, out _);
        
        compute.GetKernelThreadGroupSizes(moveClustersKernel, 
            out moveClustersKernelXGroup, 
            out _, out _);

        textureWidthId = Shader.PropertyToID("TextureWidth");
        textureHeightId = Shader.PropertyToID("TextureHeight");
        bufferWidthId = Shader.PropertyToID("BufferWidth");
        inputTextureId = Shader.PropertyToID("InputTexture");
        downSampleStepId = Shader.PropertyToID("DownSampleStep");
        clusterCountId = Shader.PropertyToID("ClusterCount");
        colorBufferId = Shader.PropertyToID("ColorBuffer");
        colorBufferSizeId = Shader.PropertyToID("ColorBufferSize");
        colorClusterBufferId = Shader.PropertyToID("ColorClusterBuffer");
        clusterBufferId = Shader.PropertyToID("ClusterBuffer");
        clusterMovedId = Shader.PropertyToID("clusterMovedId");
        isInitialized = true;
    }
}
