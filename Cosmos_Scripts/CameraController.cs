using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    // Moves camera in specified direction. Parameter axes defines movement:
    // X moves left/right
    // Y moves up/down
    // Z moves forward/backward
    public void Move(Vector3 movement){
        transform.Translate(movement, Space.World);
    }

    // Rotates camera by specified amount in degrees on each axis. Parameter axes defines rotation:
    // X rotates on Y axis (left/right)
    // Y rotates on X axis (up/down)
    public void Rotate(Vector2 deltaRotation){
        // flip Y so it behaves as expected (+Y looks up, -Y looks down)
        deltaRotation.y *= -1f;

        transform.Rotate(Vector3.up, deltaRotation.x);
        transform.Rotate(Vector3.right, deltaRotation.y);
    }
}