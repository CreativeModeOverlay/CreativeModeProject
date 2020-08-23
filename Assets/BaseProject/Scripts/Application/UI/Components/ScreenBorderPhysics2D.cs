using System.Collections.Generic;
using UnityEngine;

public class ScreenBorderPhysics2D : MonoBehaviour
{
    public RectOffset borders;
    public Camera screenCamera;
    public float distance;
    public float thickness;
    public float thicknessInnerOffset;
    public PhysicsMaterial2D borderMaterial;

    [SerializeField]
    [HideInInspector]
    private List<BoxCollider2D> colliders 
        = new List<BoxCollider2D>();
    
    private void Awake()
    {
        UpdateBorders();
    }
    
    [ContextMenu("Update borders")]
    public void UpdateBorders()
    {
        GetComponents(colliders);
        while (colliders.Count < 4)
            colliders.Add(gameObject.AddComponent<BoxCollider2D>());
        
        var bottomLeftWorld = screenCamera.ScreenToWorldPoint(new Vector3(
                borders.left - thicknessInnerOffset, 
                borders.bottom - thicknessInnerOffset, distance));
        
        var topRightWorld = screenCamera.ScreenToWorldPoint(new Vector3(
                Screen.width - borders.right + thicknessInnerOffset, 
                Screen.height - borders.top + thicknessInnerOffset, distance));

        var bottomLeft = screenCamera.transform.InverseTransformPoint(bottomLeftWorld);
        var topRight = screenCamera.transform.InverseTransformPoint(topRightWorld);
        transform.localPosition = screenCamera.transform.TransformPoint(new Vector3(0, 0, distance));

        var halfThickness = thickness / 2f;
        var doubleThickness = thickness * 2;
        var horizontalSize = new Vector2(topRight.x - bottomLeft.x + doubleThickness, thickness);
        var verticalSize = new Vector2(thickness, topRight.y - bottomLeft.y + doubleThickness);
        var center = (bottomLeft + topRight) / 2f;

        colliders[0].size = horizontalSize;
        colliders[0].offset = new Vector2(center.x, topRight.y + halfThickness);
        
        colliders[1].size = horizontalSize;
        colliders[1].offset = new Vector2(center.x, bottomLeft.y - halfThickness);
        
        colliders[2].size = verticalSize;
        colliders[2].offset = new Vector2(bottomLeft.x - halfThickness, center.y);
        
        colliders[3].size = verticalSize;
        colliders[3].offset = new Vector2(topRight.x + halfThickness, center.y);

        foreach (var c in colliders)
            c.sharedMaterial = borderMaterial;
    }
}
