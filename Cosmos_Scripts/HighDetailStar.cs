using UnityEngine;
using System.Collections;

public class HighDetailStar : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    public float moveFactor = 4.0f;
    private PoolManager highDetailStarPool;

    void Start()
    {
        movementSpeed = FindObjectOfType<KeyboardMouseInput>().movementSpeed * moveFactor ;
        highDetailStarPool = FindObjectOfType<OctreeManager>().highDetailStarPool;
    }

    void Update()
    {
        transform.Translate(GetMovementInput(), Space.World);
    }

    private Vector3 GetMovementInput(){
        Vector3 movement = Vector3.zero;

        // forward input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            movement.z -= 1f;
        }
        // backward input
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            movement.z += 1f;
        }

        movement *= movementSpeed;

        return movement;
    }
}