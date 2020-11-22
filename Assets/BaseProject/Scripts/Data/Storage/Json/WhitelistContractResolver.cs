using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CreativeMode.Serializer
{
    public class WhitelistContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> visiblePropertiesByType =
            new Dictionary<Type, HashSet<string>>();

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (visiblePropertiesByType.TryGetValue(objectType, out var filterProperties))
            {
                var props = contract.Properties;
                
                for (var i = props.Count - 1; i >= 0; i--)
                {
                    var property = props[i];

                    if (!filterProperties.Contains(property.UnderlyingName))
                        props.RemoveAt(i);
                }
            }
            
            return contract;
        }

        public WhitelistContractResolver AddType(Type type, params string[] visibleProperties)
        {
            visiblePropertiesByType[type] = new HashSet<string>(visibleProperties);
            return this;
        }

        public WhitelistContractResolver AddType<T>(params string[] visibleProperties)
        {
            return AddType(typeof(T), visibleProperties);
        }

        public WhitelistContractResolver AddUnityTypes()
        {
            AddType<Vector2>("x", "y");
            AddType<Vector3>("x", "y", "z");
            AddType<Vector4>("x", "y", "z", "w");
            AddType<Rect>("x", "y", "width", "height");
            AddType<RectOffset>("left", "right", "top", "bottom");
            
            AddType<Quaternion>("x", "y", "z", "w");
            AddType<Matrix4x4>("m00", "m01", "m02", "m03", 
                "m10", "m11", "m12", "m13", 
                "m20", "m21", "m22", "m23", 
                "m30", "m31", "m32", "m33");
            
            AddType<Color>("r", "g", "b", "a");
            AddType<Color32>("r", "g", "b", "a");
            AddType<LayerMask>("value");
            
            AddType<Gradient>("colorKeys", "alphaKeys", "mode");
            AddType<AnimationCurve>("keys", "preWrapMode", "postWrapMode");
            
            AddType<Vector2Int>("x", "y");
            AddType<Vector3Int>("x", "y", "z");
            AddType<RectInt>("x", "y", "width", "height");

            return this;
        }
    }
}