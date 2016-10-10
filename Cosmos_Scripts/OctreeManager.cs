using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class OctreeManager : MonoBehaviour {

    //Octree globals
    OctreeDataStructure Octree;
    bool treeFinished   = false;
    List<OctreeObject[]> visibleStars;
    myVertex[] vertices;

    //Game variables
    public static float conversionFactor  = 1.0f;

    //Camera and mesh
    Camera      secondaryCamera;
    Mesh        mesh;
    int         meshSize;

    //Testing credentials
    static Stopwatch timer=new Stopwatch();
    bool testFlag = true;
    bool singleFrame = true;

    //globals for high detail stars and pool
    List<Vector3> pointAlreadyPassed;
    public PoolManager highDetailStarPool;
    public GameObject highDetailStar;

    void Awake () {
        visibleStars = new List<OctreeObject[]>();
        highDetailStarPool = new PoolManager(highDetailStar, 1000);

        //Reading from file and formating for octree
        StarListToVoxels myVoxelsStringFormat = new StarListToVoxels();
        OctreeObject[] voxels = myVoxelsStringFormat.readData();
        
        print("Stars parsed from dataset: " + voxels.Length);

        //Initializing root   
        Octree = new OctreeDataStructure(voxels);
        timer.Start();
        Octree.init();
        timer.Stop();

        print("Tree built: "+timer.ElapsedMilliseconds);
        print("---------------------------------------");

        if(Octree!=null) treeFinished = true;

        //Getting secondary camera
        GameObject secondaryObject = GameObject.Find("SecondaryCamera");
        secondaryCamera = secondaryObject.GetComponent<Camera>();

        pointAlreadyPassed = new List<Vector3>();
    }

    void Update(){
        if (treeFinished == true && singleFrame==true){
            singleFrame = false;
            generateMesh();
        }
        checkNearPlaneCollisions();
    }

    //Manager function for checking every third vertices to see if behind the camera, for each triangle that
    //is behind the plane will return the center of the triangle
    void checkNearPlaneCollisions(){
        Vector3[] vertices = mesh.vertices;
        Plane[] p = GeometryUtility.CalculateFrustumPlanes(secondaryCamera);

        for (int i = 0; i < vertices.Length; i += 3) {
            if (!pointAlreadyPassed.Contains(vertices[i])){

                if (vertexIsBehindPlane(vertices[i], p[4])){
                    pointAlreadyPassed.Add(vertices[i]);
                    //Getting center of a vertice for spawning high detail model
                    Vector3 centerOfTriangle = ((vertices[i] + vertices[i + 1] + vertices[i + 2]) / 3);
                    Vector3 highDetailPos = centerOfTriangle;
                    Color clr = mesh.colors[i];
                    highDetailPos.z = (centerOfTriangle.z - (secondaryCamera.transform.position.z + secondaryCamera.nearClipPlane)) * 2f;
                    highDetailPos.z += secondaryCamera.transform.position.z + secondaryCamera.nearClipPlane;
                    //Send coordinates to Pool Manager
                    GameObject star = highDetailStarPool.CreateOrPool(highDetailPos, Quaternion.identity);

                    MeshRenderer starRenderer = star.GetComponent<MeshRenderer>();
                    Material starMaterial = starRenderer.material;
                    starMaterial.color = mesh.colors[i];
                    starRenderer.material = starMaterial;
                    float scale = setScale(true);
                    star.transform.localScale=new Vector3(scale,scale, scale);

                    UnityEngine.Debug.Log("Star-added: " + centerOfTriangle.z + " " + (secondaryCamera.transform.position.z + secondaryCamera.nearClipPlane) + " " + highDetailPos.z, star);
                }
            }
        }
    }


    //Checking if a point is behind a plane, this will be used to check if vertices are behind the second camera nearplane
    bool vertexIsBehindPlane(Vector3 v,Plane p){
        return !p.GetSide(v);
    }

    //Function for later development
    void checkFarCollision(){
        int count = 0;
        Octree.countCulling(count, GeometryUtility.CalculateFrustumPlanes(secondaryCamera));
    }

    //When method is called it will make a call to the octree passing secondary Camera frustum
    //to the octree and fetch all points within the frustum
    //
    //Take the vector between each (point+camera.poisiton)/ conversionFactor
    //
    //Will then generate a triangle location for each point retrieved, and then bake mesh
    public void generateMesh(){
        timer = new Stopwatch();
        timer.Start();
        Octree.frustumCulling(visibleStars, GeometryUtility.CalculateFrustumPlanes(secondaryCamera));
        print("Fetch from Octree: "+timer.ElapsedMilliseconds);
        timer.Stop();
        myVertex[] array = listToArray(visibleStars);
        visibleStars.Clear();
        timer = new Stopwatch();
        timer.Start();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        meshSize = array.Length;
      
        int vertexLength = array.Length*3;
        Vector3[] vertices    = new Vector3[vertexLength];
        int[] indecies         = new int[vertexLength];
        Color[] colours        = new Color[vertexLength];
        print("Triangle-Count: " + printTriangles());

        int y = 0;
        for(int i=0; i<vertexLength; i+=3){
            //Shrinking the stars gathered by the currentScale
            array[y].position = (array[y].position+Camera.main.transform.position) / conversionFactor;

            Vector3[] temp = pointsOfTriangle(array[y].position);
            vertices[i]        = temp[2];
            vertices[i + 1]    = temp[1];
            vertices[i + 2]    = temp[0];

            colours[i]      = array[y].color;
            colours[i + 1]  = array[y].color;
            colours[i + 2]  = array[y].color;

            indecies[i]     = i;
            indecies[i + 1] = i + 1;
            indecies[i + 2] = i + 2;
            y++;
        }
        mesh.vertices = vertices;
        mesh.colors = colours;
        mesh.triangles = indecies;
        print("Mesh Generated: "+timer.ElapsedMilliseconds);
        timer.Stop();
    }

    //When given a point creates a equalilateral triangle given the center
    //I made a mistake to include depth value, will remove dynamic triangle size, will make static size
    Vector3[] pointsOfTriangle(Vector3 center){

        int[] degrees       = {120,240,360};
        Vector3[] result    = new Vector3[3];
        float radius = setScale(false);

        for (int i=0; i<result.Length;i++) {
            float angle = (Mathf.PI / 180) * degrees[i];
            Vector3 yElement = Camera.main.transform.up * Mathf.Round(radius * Mathf.Sin(angle));
            Vector3 xElement= Camera.main.transform.right * Mathf.Round(radius * Mathf.Cos(angle));
            result[i] = yElement + xElement + center;
        }
        return result;
    }

    //Table for looking up colours of stars from colourindex value taken from text file
    public static Color ciToRBG(float ci){
        double temp;
        double r = 0.0, g = 0.0, b = 0.0;

        if (ci < -0.4) ci = -0.4f;
        if (ci > 2.0) ci = 2.0f;

        if ((ci >= -0.40) && (ci < 0.00)){
            temp = (ci + 0.40) / (0.00 + 0.40);
            r = 0.61 + (0.11 * temp) + (0.1 * temp * temp);
        }
        else if ((ci >= 0.00) && (ci < 0.40)){
            temp = (ci - 0.00) / (0.40 - 0.00); r = 0.83 + (0.17 * temp);
        }
        else if ((ci >= 0.40) && (ci < 2.10)) {
            temp = (ci - 0.40) / (2.10 - 0.40); r = 1.00;
        }

        if ((ci >= -0.40) && (ci < 0.00)) {
            temp = (ci + 0.40) / (0.00 + 0.40); g = 0.70 + (0.07 * temp) + (0.1 * temp * temp);
        }
        else if ((ci >= 0.00) && (ci < 0.40)) {
            temp = (ci - 0.00) / (0.40 - 0.00); g = 0.87 + (0.11 * temp);
        }
        else if ((ci >= 0.40) && (ci < 1.60)) {
            temp = (ci - 0.40) / (1.60 - 0.40); g = 0.98 - (0.16 * temp);
        }
        else if ((ci >= 1.60) && (ci < 2.00)) {
            temp = (ci - 1.60) / (2.00 - 1.60); g = 0.82 - (0.5 * temp * temp);
        }

        if ((ci >= -0.40) && (ci < 0.40)) {
            temp = (ci + 0.40) / (0.40 + 0.40); b = 1.00;
        }
        else if ((ci >= 0.40) && (ci < 1.50)) {
            temp = (ci - 0.40) / (1.50 - 0.40); b = 1.00 - (0.47 * temp) + (0.1 * temp * temp);
        }
        else if ((ci >= 1.50) && (ci < 1.94)) {
            temp = (ci - 1.50) / (1.94 - 1.50); b = 0.63 - (0.6 * temp * temp);
        }
        return new Color((float)r,(float)g,(float)b,1);
    }


    //Utility function for breaking the individual arrays returned from octree into a single array
    myVertex[] listToArray(List<OctreeObject[]> list){
        int fullCount = 0;

        foreach(OctreeObject[] oo in list){
            fullCount = fullCount + oo.Length;
        }

        List<myVertex> array = new List<myVertex>();
        bool flag = false;
        int i = 0;

        foreach (OctreeObject[] oo in list) {
            foreach (OctreeObject o in oo){
                if (i < 21500){
                    //if (checkPointInFrustum(o.spatialPosition)) {
                        array.Add(new myVertex(o.colourScheme, o.spatialPosition, o.size));
                        i++;
                    //}
                }else {
                    flag = true;
                    break;
                }
            }
            if (flag == true) break;
        }

        if (flag) { print("list over 65k vertices"); }
        return array.ToArray();
    }
    int setScale(bool b){
        System.Random r = new System.Random();
        if (b) {
            if (conversionFactor > 0.5){
                return r.Next(1, 10);
            }
            else if (conversionFactor < 0.5){
                return r.Next(10, 50);
            }
            else{
                return r.Next(1,25);
            }
        }
        else {
            if (conversionFactor > 0.5) { 
                return 4;
            }
            else if (conversionFactor < 0.5){
                return 15;
            }
            else{
                return 7;
            }
        }
    }

    bool checkPointInFrustum(Vector3 p){
        Vector3 result = secondaryCamera.WorldToViewportPoint(p);
        if (    ((result.x < 0) || (result.x > 1))   ||   ((result.y < 0) || (result.y > 1))    ){
            return false;
        }
        return ((p.z > secondaryCamera.transform.position.z + secondaryCamera.nearClipPlane) && (p.z < secondaryCamera.transform.position.z + secondaryCamera.farClipPlane));
    }

    //Nested class for taking information from octree and creating vertex information for mesh
    public class myVertex {
        public Color color;
        public Vector3 position;
        public float size;

        public myVertex(float color0, Vector3 position0, float size0){
            size = size0;
            position = position0;
            color = ciToRBG(color0);
        }
    }
    int printTriangles()
    {
        return (int)(643.0 * conversionFactor);
    }

}

//Class used for reading from text file and setting up
//acceptable datastructure to place into Octree
public class StarListToVoxels {

    public string fileName = "refinedDatav4";   //file name
    public OctreeObject[] rootList;

    public OctreeObject[] readData(){

        TextAsset textFile = (TextAsset)Resources.Load(this.fileName);  //Load text file from resources
        string fileAsLongString = textFile.text;
        List<string> starCatalogue = new List<string>();
        starCatalogue.AddRange(fileAsLongString.Split("\n"[0]));

        rootList = new OctreeObject[starCatalogue.Count]; //Setting root list count = to file read count

        int counter = 0;
        foreach (string s in starCatalogue){
            string[] splitS = s.Split(',');

            float x         = float.Parse(splitS[3]);
            float y         = float.Parse(splitS[4]);
            float z         = float.Parse(splitS[5]);
            float size      = float.Parse(splitS[1]);
            float colour    = float.Parse(splitS[2]);

            rootList[counter] = new OctreeObject(new Vector3(x, y, z), colour, size);
            counter++;
        }
        starCatalogue = null;
        return rootList;
    }

}
