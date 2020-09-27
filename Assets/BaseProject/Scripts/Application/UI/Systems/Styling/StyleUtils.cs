using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CreativeMode
{
    public static class StyleUtils
    {
        public static List<ResolvedProperty> ApplyButtonStyle(StyleResolver<ButtonStyleData> resolver, params Button[] buttons)
        {
            return ResolveObjects(resolver, buttons, (r, b) =>
            {
                ApplySelectableStyle(r.ForType<SelectableStyleData>(), b);
            });
        }

        public static List<ResolvedProperty> ApplyImageStyle(StyleResolver<ImageStyleData> resolver, params Image[] images)
        {
            return ResolveObjects(resolver, images, (r, i) =>
            {
                ApplyGraphicStyle(r.ForType<GraphicStyleData>(), i);

                i.sprite = r.Resolve(d => d.sprite);
            });
        }

        public static List<ResolvedProperty> ApplyTextStyle(StyleResolver<TextStyleData> resolver, params Text[] texts)
        {
            return ResolveObjects(resolver, texts, (r, t) =>
            {
                ApplyGraphicStyle(r.ForType<GraphicStyleData>(), t);

                var newFont = r.Resolve(p => p.font);

                if (newFont)
                    t.font = newFont;
                
                t.fontStyle = r.Resolve(p => p.fontStyle);
                t.fontSize = r.Resolve(p => p.fontSize);
                t.lineSpacing = r.Resolve(p => p.lineSpacing);
                t.supportRichText = r.Resolve(p => p.richText);
                t.alignment = r.Resolve(p => p.alignment);
                t.alignByGeometry = r.Resolve(p => p.alignByGeometry);
                t.horizontalOverflow = r.Resolve(p => p.horizontalOverflow);
                t.verticalOverflow = r.Resolve(p => p.verticalOverflow);
                t.resizeTextForBestFit = r.Resolve(p => p.bestFit);
                t.resizeTextMinSize = r.Resolve(p => p.bestFitMinSize);
                t.resizeTextMaxSize = r.Resolve(p => p.bestFitMaxSize);
            });
        }

        public static List<ResolvedProperty> ApplyGraphicStyle(StyleResolver<GraphicStyleData> resolver, params Graphic[] graphics)
        {
            return ResolveObjects(resolver, graphics, ApplyGraphicStyle);
        }
        
        private static void ApplyGraphicStyle(LoggingResolver<GraphicStyleData> resolver, Graphic graphic)
        {
            resolver.SetTarget(graphic);

            graphic.color = resolver.Resolve(d => d.color);
            graphic.material = resolver.Resolve(d => d.material);

            ApplyShadow(resolver, graphic);
        }

        private static void ApplySelectableStyle(LoggingResolver<SelectableStyleData> resolver, Selectable button)
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

        private static void ApplyShadow<T>(LoggingResolver<T> resolver, Graphic graphicObject)
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

        public static StyleResolver<T> CreateResolver<T>(ElementStyle<T> style)
            where T : new()
        {
            return new StyleResolver<T>(t => 
                TraverseHierarchy(style, style.Overrides, style.Inherits, t));
        }

        public static StyleResolver<T2> CreateResolver<T1, T2>(ElementStyle<T1> style,
            Func<T1, StyleReference<T2>> styleGetter)
            where T2 : new()
        {
            return new StyleResolver<T2>(t => 
                TraverseHierarchy(style, style.Overrides, style.Inherits, (asset, data) => 
                {
                    if (data is T1 castedData)
                    {
                        var nestedStyle = styleGetter(castedData); 
                        return TraverseHierarchy(asset, nestedStyle.overrides, nestedStyle.inherits, t); 
                    }

                    return false;
                }));
        }

        private static bool TraverseHierarchy<S>(ScriptableObject asset, object overrides, S[] inherits, 
            Func<ScriptableObject, object, bool> propertyGetter)
            where S : ElementStyle
        {
            HashSet<object> visitedData = new HashSet<object>();
            
            bool TraverseRecursive(ScriptableObject nestedObject, object nestedInherits, ElementStyle[] nestedOverrides)
            {
                if (visitedData.Contains(nestedObject))
                {
                    Debug.LogError($"Circular dependency found in {asset.name}, " +
                              $"{nestedObject.name} found multiple times in hierarchy");
                    return false;
                }

                visitedData.Add(nestedObject);
                
                if (propertyGetter(nestedObject, nestedInherits))
                    return true;

                if (nestedOverrides != null)
                {
                    for (var i = nestedOverrides.Length - 1; i >= 0; i--)
                    {
                        var o = nestedOverrides[i];
                    
                        if(!o)
                            continue;

                        if (TraverseRecursive(o, o.Overrides, o.Inherits))
                            return true;
                    }
                }
                
                visitedData.Remove(nestedObject);

                return false;
            }

            return TraverseRecursive(asset, overrides, inherits);
        }

        private static List<ResolvedProperty> ResolveObjects<D, T>(
            StyleResolver<D> styleResolver, T[] objects, Action<LoggingResolver<D>, T> loggingResolverApplier)
            where D : new() 
            where T : UIBehaviour
        {
            var resolver = new LoggingResolver<D>(styleResolver);

            foreach (var obj in objects)
            {
                if (!obj)
                    continue;

                resolver.SetTarget(obj);
                loggingResolverApplier(resolver, obj);
            }

            return resolver.GetResolvedProperties();
        }
        
        public class LoggingResolver<D>
            where D : new()
        {
            private readonly StyleResolver<D> resolver;
            private List<ResolvedProperty> properties;
            private UIBehaviour targetObject;
            
            public LoggingResolver(StyleResolver<D> resolver)
            {
                this.resolver = resolver;
                properties = new List<ResolvedProperty>();
            }

            public LoggingResolver<DNew> ForType<DNew>()
                where DNew : new()
            {
                return new LoggingResolver<DNew>(resolver.ForType<DNew>())
                {
                    properties = properties,
                    targetObject = targetObject
                };
            }

            public void SetTarget(UIBehaviour target)
            {
                targetObject = target;
            }

            public P Resolve<P>(Expression<Func<D, StyleProperty<P>>> propertyGetterExpression)
            {
                var resolvedProperty = resolver.Resolve(propertyGetterExpression);
                
                properties.Add(new ResolvedProperty
                {
                    value = resolvedProperty.value,
                    valueOrigin = resolvedProperty.origin,
                    propertyName = resolvedProperty.propertyName,
                    valueTarget = targetObject
                });
                
                return resolvedProperty.value;
            }

            public List<ResolvedProperty> GetResolvedProperties()
            {
                return properties;
            }
        }
    }
}