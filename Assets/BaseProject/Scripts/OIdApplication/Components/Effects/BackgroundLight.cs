using UnityEngine;

public class BackgroundLight : MonoBehaviour
{
    public Camera camera;
    public GameObject referenceObject;
    public float offset;

    private void LateUpdate()
    {
        var screenPoint = camera.WorldToScreenPoint(referenceObject.transform.position);
        var castRay = camera.ScreenPointToRay(screenPoint);

        if (gameObject.scene.GetPhysicsScene().Raycast(castRay.origin, castRay.direction, out var hitInfo))
        {
            transform.position = hitInfo.point + hitInfo.normal * offset;
        }
    }
}
