using System;
using System.Linq.Expressions;
using UnityEngine;

namespace CreativeMode
{
    public class StyleResolver<D> 
            where D : new()
    {
        private static readonly D defaultData = new D();
        
        private readonly Func<Func<ScriptableObject, object, bool>, bool> hierarchyTraverser;
        private Component lastComponent;

        public StyleResolver(Func<Func<ScriptableObject, object, bool>, bool> traverser)
        {
            hierarchyTraverser = traverser;
        }

        public StyleResolver<DNew> ForType<DNew>() 
            where DNew: new()
        {
            return new StyleResolver<DNew>(hierarchyTraverser);
        }

        public ResolvedValue<P> Resolve<P>(Expression<Func<D, StyleProperty<P>>> propertyGetterExpression)
        {
            P resolvedValue = default;
            ScriptableObject resolvedAsset = null;
            var propertyName = "?unknown?";

            if (propertyGetterExpression.Body is MemberExpression memberExpression)
                propertyName = memberExpression.Member.Name;

            var propertyGetter = propertyGetterExpression.Compile();
            var dType = typeof(D);
            
            var isApplied = hierarchyTraverser((asset, data) =>
            {
                if (dType.IsInstanceOfType(data))
                {
                    var prop = propertyGetter((D) data);

                    if (prop.shouldOverride)
                    {
                        resolvedValue = prop.value;
                        resolvedAsset = asset;
                        return true;
                    }
                }

                return false;
            });
            
            var resultValue = isApplied ? resolvedValue : propertyGetter(defaultData).value;

            return new ResolvedValue<P>
            {
                origin = resolvedAsset,
                value = resultValue,
                propertyName = propertyName
            };
        }
    }
}