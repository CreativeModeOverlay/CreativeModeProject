using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncapsulateToOwnScene : MonoBehaviour
{
    private Scene scene;
    
    private void Awake()
    {
        var prefix = "";
        
        if (transform.parent)
        {
            prefix = "[" + transform.parent.name + "] ";
            transform.parent = null;
        }

        scene = SceneManager.CreateScene(prefix + name, new CreateSceneParameters
        {
            localPhysicsMode = LocalPhysicsMode.Physics3D | 
                               LocalPhysicsMode.Physics2D
        });

        SceneManager.MoveGameObjectToScene(gameObject, scene);

        foreach (var cam in FindObjectsOfType<Camera>())
            cam.scene = scene;
    }

    private void FixedUpdate()
    {
        scene.GetPhysicsScene2D()
            .Simulate(Time.fixedDeltaTime);
        scene.GetPhysicsScene()
            .Simulate(Time.fixedDeltaTime);
    }
}
