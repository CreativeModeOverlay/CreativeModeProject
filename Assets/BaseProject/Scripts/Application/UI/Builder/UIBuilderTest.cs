﻿using UnityEngine;

namespace CreativeMode
{
    public class UIBuilderTest : MonoBehaviour
    {
        public LinearLayoutWidget g;

        public void Awake()
        {
            g.Orientation = Orientation.Horizontal;
            
            var leftPanel = g.AddLayout(Orientation.Vertical);
            var rightPanel = g.AddLayout(Orientation.Vertical);
            
            leftPanel.LayoutParams = new LayoutParams { flexibleWidth = 0, minWidth = 300, preferredWidth = 300 };
            rightPanel.LayoutParams = new LayoutParams { flexibleWidth = 1 };
            
            leftPanel.AddFloatField("Float1");
            var testGroup = leftPanel.AddGroup("Test");
            testGroup.AddFloatField("Float2").Subtitle = "Test field";
            testGroup.AddFloatField("Float3");
            testGroup.AddToggleField("Toggle");
            
            var testGroup2 = leftPanel.AddGroup("Test");
            testGroup2.AddIntField("Int1");
            testGroup2.AddIntField("Int2");
            testGroup2.AddTextField("TextField1");
            
            rightPanel.AddTextField("TextField2");
            rightPanel.AddSpace();
            rightPanel.AddVector2Field("Vector2");
            rightPanel.AddVector3Field("Vector3");
            rightPanel.AddVector4Field("Vector4");
            rightPanel.AddEnumField<TextAnchor>("Enum");
            rightPanel.AddEnumField<Orientation>("Enum");
        }
    }
}