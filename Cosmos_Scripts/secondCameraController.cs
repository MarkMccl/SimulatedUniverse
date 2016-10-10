using UnityEngine;
using System.Collections;

public class secondCameraController : MonoBehaviour {
    Camera main;
    Camera secondary;

    public static float nearPlaneToFarPlaneDistance = 500f;

    float startingFarplane;
    float startingNearplane;

    // Use this for initialization
    void Awake()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        main = mainCamera.GetComponent<Camera>();
        secondary = gameObject.GetComponent<Camera>();
        initSize();
        alignCameras();

        startingFarplane = secondary.farClipPlane;
        startingNearplane = secondary.nearClipPlane;
    }

    // Update is called once per frame
    void Update()
    {
        alignCameras();
        updateSize();
    }

    //giving secondary camera same attributes as main camera
    void alignCameras()
    {
        secondary.transform.position = main.transform.position;
        secondary.transform.rotation = main.transform.rotation;
        secondary.fieldOfView = main.fieldOfView;
    }
    
    //setting the initial size of camera
    void initSize(){
        secondary.nearClipPlane = main.farClipPlane;
        secondary.farClipPlane = secondary.nearClipPlane + nearPlaneToFarPlaneDistance;
    }

    //updating frustum to be Camera.main*conversionFactor
    void updateSize(){
        secondary.farClipPlane = ((startingFarplane - secondary.nearClipPlane) * OctreeManager.conversionFactor) + secondary.nearClipPlane;
    }
}