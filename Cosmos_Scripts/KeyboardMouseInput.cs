using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CameraController))]
public class KeyboardMouseInput : MonoBehaviour
{
    public float movementSpeed = 0.7f;
    public float mouseMultiplier = 0.2f;
    public bool allowStrafing = true;
    public bool allowUpDownMovement = true;
    public bool allowRotation = true;

    private CameraController camController;

    private Vector2 previousMousePosition = Vector2.zero;

    void Awake(){
        camController = GetComponent<CameraController>();
    }

    void Start(){
        Vector2 mousePosition = Input.mousePosition;
        previousMousePosition = mousePosition;
    }

    void Update(){
        Vector3 movement = GetMovementInput();
        Vector2 rotation = GetRotationInput();

        movement *= movementSpeed;
        rotation *= mouseMultiplier;

        camController.Move(movement);
        if (allowRotation){
            camController.Rotate(rotation);
        }
    }

    private Vector3 GetMovementInput(){
        Vector3 movement = Vector3.zero;

        // forward input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            movement.z += 1f;
        }
        // backward input
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            movement.z -= 1f;
        }
        if (allowStrafing){
            // left input
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                movement.x -= 1f;
            }
            // right input
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                movement.x += 1f;
            }
        }
        if (allowUpDownMovement){
            // up input
            if (Input.GetKey(KeyCode.Space)){
                movement.y += 1f;
            }
            // down input
            if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)){
                movement.y -= 1f;
            }
        }

        movement *= movementSpeed;

        return movement;
    }

    private Vector2 GetRotationInput() {
        Vector2 rotation = Vector2.zero;
        Vector2 mousePosition = Input.mousePosition;
        //gradually changing rotation
        rotation = mousePosition - previousMousePosition;

        previousMousePosition = Input.mousePosition;

        return rotation;
    }
}