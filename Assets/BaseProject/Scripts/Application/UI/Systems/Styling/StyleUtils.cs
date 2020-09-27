using System;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public static class StyleUtils
    {
        public static void ApplyButtonStyle(StyleResolver<ButtonStyleData> resolver, params Button[] buttons)
        {
            foreach (var button in buttons)
            {
                if(!button)
                    continue;
                
                resolver.NewObject(button);
                
                ApplySelectableStyle(button, resolver);
            }
        }

        public static void ApplyImageStyle(StyleResolver<ImageStyleData> resolver, params Image[] images)
        {
            foreach (var image in images)
            {
                if(!image)
                    continue;
                
                resolver.NewObject(image);
                
                ApplyGraphicStyle(resolver, image);

                image.sprite = resolver.Resolve(d => d.sprite);
            }
        }

        public static void ApplyTextStyle(StyleResolver<TextStyleData> resolver, params Text[] texts)
        {
            foreach (var text in texts)
            {
                if(!text)
                    continue;
                
                resolver.NewObject(text);
                
                ApplyGraphicStyle(resolver, text);

                var newFont = resolver.Resolve(p => p.font);

                if (newFont)
                    text.font = newFont;
                
                text.fontStyle = resolver.Resolve(p => p.fontStyle);
                text.fontSize = resolver.Resolve(p => p.fontSize);
                text.lineSpacing = resolver.Resolve(p => p.lineSpacing);
                text.supportRichText = resolver.Resolve(p => p.richText);
                text.alignment = resolver.Resolve(p => p.alignment);
                text.alignByGeometry = resolver.Resolve(p => p.alignByGeometry);
                text.horizontalOverflow = resolver.Resolve(p => p.horizontalOverflow);
                text.verticalOverflow = resolver.Resolve(p => p.verticalOverflow);
                text.resizeTextForBestFit = resolver.Resolve(p => p.bestFit);
                text.resizeTextMinSize = resolver.Resolve(p => p.bestFitMinSize);
                text.resizeTextMaxSize = resolver.Resolve(p => p.bestFitMaxSize);
            }
        }

        public static void ApplyGraphicStyle<T>(StyleResolver<T> resolver, params Graphic[] graphics)
            where T : GraphicStyleData, new()
        {
            foreach (var graphic in graphics)
            {
                if(!graphic)
                    continue;

                resolver.NewObject(graphic);

                graphic.color = resolver.Resolve(d => d.color);
                graphic.material = resolver.Resolve(d => d.material);

                ApplyShadow(graphic, resolver);
            }
        }

        private static void ApplySelectableStyle<T>(Selectable button, StyleResolver<T> resolver)
            where T : SelectableStyleData, new()
        {
            var colorBlock = button.colors;

            colorBlock.normalColor = resolver.Resolve(d => d.colorNormal);
            colorBlock.highlightedColor = resolver.Resolve(d => d.colorHighlighted);
            colorBlock.pressedColor = resolver.Resolve(d => d.colorPressed);
            colorBlock.selectedColor = resolver.Resolve(d => d.colorSelected);
            colorBlock.disabledColor = resolver.Resolve(d => d.colorDisabled); 
            colorBlock.colorMultiplier = resolver.Resolve(d => d.colorMultiplier);
            colorBlock.fadeDuration = resolver.Resolve(d => d.colorFadeDuration);

            button.colors = colorBlock;
        }

        private static void ApplyShadow<T>(Graphic graphicObject, StyleResolver<T> resolver)
            where T : GraphicStyleData, new()
        {
            var shadowType = resolver.Resolve(d => d.shadowType);
            var gameObject = graphicObject.gameObject;
            var shadow = gameObject.GetComponent<Shadow>();

            switch (shadowType)
            {
                case GraphicStyleData.ShadowType.None:
                    if (shadow)
                    {
                        Object.DestroyImmediate(shadow);
                        shadow = null;
                    }

                    break;

                case GraphicStyleData.ShadowType.Shadow:
                    if (!shadow || shadow.GetType() != typeof(Shadow))
                    {
                        Object.DestroyImmediate(shadow);
                        shadow = gameObject.AddComponent<Shadow>();
                    }

                    break;

                case GraphicStyleData.ShadowType.Outline:
                    if (!shadow || shadow.GetType() != typeof(Outline))
                    {
                        Object.DestroyImmediate(shadow);
                        shadow = gameObject.AddComponent<Outline>();
                    }

                    break;
            }

            if (!shadow)
                return;

            shadow.effectColor = resolver.Resolve(d => d.shadowColor);
            shadow.effectDistance = resolver.Resolve(d => d.shadowDistance);
        }

        public static StyleResolver<T> CreateResolver<S, T>(StringBuilder resolveLogger, BaseElementStyle<S, T> style) 
            where S : BaseElementStyle<S, T> 
            where T : new()
        {
            return new StyleResolver<T>(resolveLogger, t => 
                TraverseHierarchy(style.name, style.Overrides, style.Innherits, t));
        }

        public static StyleResolver<T2> CreateResolver<S1, T1, S2, T2>(StringBuilder resolveLogger, BaseElementStyle<S1, T1> style,
            Func<T1, StyleReference<S2, T2>> styleGetter)
            where S1 : BaseElementStyle<S1, T1>
            where S2 : BaseElementStyle<S2, T2> 
            where T2 : new()
        {
            return new StyleResolver<T2>(resolveLogger, t => 
                TraverseHierarchy(style.name, style.Overrides, style.Innherits, (s, name) => 
                { 
                    var nestedStyle = styleGetter(s); 
                    return TraverseHierarchy(name, nestedStyle.overrides, nestedStyle.innherits, t); 
                }));
        }

        private static bool TraverseHierarchy<S, T>(string name, T overrides, S[] inherits, 
            Func<T, string, bool> propertyGetter)
            where S : BaseElementStyle<S, T>
        {
            bool TraverseRecursive(string nestedName, T nestedInherits, S[] nestedOverrides)
            {
                if (propertyGetter(nestedInherits, nestedName))
                    return true;

                for (var i = nestedOverrides.Length - 1; i >= 0; i--)
                {
                    var o = nestedOverrides[i];
                    
                    if(!o)
                        continue;

                    if (TraverseRecursive(o.name, o.Overrides, o.Innherits))
                        return true;
                }

                return false;
            }

            return TraverseRecursive(name, overrides, inherits);
        }
 
        public class StyleResolver<D> 
            where D : new()
        {
            private static readonly D defaultData = new D();
            private const string defaultAssetName = "default";
            
            private readonly Func<Func<D, string, bool>, bool> hierarchyTraverser;
            private readonly StringBuilder resolverLog;
            private Component lastComponent;
            
            public StyleResolver(StringBuilder resolveLogger, Func<Func<D, string, bool>, bool> traverser)
            {
                hierarchyTraverser = traverser;
                resolverLog = resolveLogger;
            }

            public void NewObject(Component obj)
            {
                if (obj != lastComponent && resolverLog != null)
                {
                    if (resolverLog.Length > 0)
                        resolverLog.AppendLine();

                    resolverLog.AppendLine($"{obj.gameObject.name}#{obj.GetType().Name}");
                    lastComponent = obj;
                }
            }

            public P Resolve<P>(Expression<Func<D, StyleProperty<P>>> propertyGetterExpression)
            {
                P value = default;
                var assetName = defaultAssetName;
                var propertyName = "unknown";

                if (resolverLog != null)
                {
                    if (propertyGetterExpression.Body is MemberExpression memberExpression)
                        propertyName = memberExpression.Member.Name;
                }

                var propertyGetter = propertyGetterExpression.Compile();

                var isApplied = hierarchyTraverser((d, name) =>
                {
                    var prop = propertyGetter(d);

                    if (prop.shouldOverride)
                    {
                        value = prop.value;
                        assetName = name;
                        return true;
                    }

                    return false;
                });
                
                var resultValue = isApplied ? value : propertyGetter(defaultData).value;

                if (resolverLog != null)
                {
                    var propertyPath = $"{assetName}.{propertyName}";

                    if (assetName != defaultAssetName)
                        propertyPath = $"<color=#88ff88><b>{propertyPath}</b></color>";
                
                    resolverLog.AppendLine($"- {propertyPath}: {resultValue}");
                }
                
                return resultValue;
            }
        }
    }
}