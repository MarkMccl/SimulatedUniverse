using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitTesting : MonoBehaviour {

    static float min = 0.0f;
    static float max = 20.0f;

    Vector3 minV = new Vector3(min, min, min);
    Vector3 maxV = new Vector3(max, max, max);

    Bounds[] b;

    System.Random r = new System.Random();

    // Use this for initialization
    void Start()
    {
        unitTestOctreePutandGet();
    }

    void unitTestOctreePutandGet()
    {
        OctreeObject[] randomStars = new OctreeObject[(int)(max - min)];
        Bounds testBounds = new Bounds();

        GameObject testCamera = GameObject.Find("testCamera");
        Camera test = testCamera.GetComponent<Camera>();

        List<OctreeObject[]> myList = new List<OctreeObject[]>();

        testBounds.SetMinMax(minV, maxV);

        for (int i=0;i<randomStars.Length;i++){
            randomStars[i] = new OctreeObject(new Vector3((float)rnd(min,max), (float)rnd(min,max), (float)rnd(min,max)),1.0f,1.0f);
        }

        //Expected 0 and 1 due to camera(near=0.3,far=3);
        //for (int i = 0; i < randomStars.Length; i++)
        //{
        //    randomStars[i] = new OctreeObject(new Vector3(i, i, i),1.0f,1.0f);
        //}

        OctreeDataStructure octree = new OctreeDataStructure(testBounds, randomStars);

        b = octree.divideAABB(testBounds);

        octree.init();
        octree.frustumCulling(myList, GeometryUtility.CalculateFrustumPlanes(test));

        foreach (OctreeObject[] oo in myList)
        {
            foreach (OctreeObject o in oo)
            {
                print("X:" + o.spatialPosition.x + " Y:" + o.spatialPosition.y + " Z:" + o.spatialPosition.z);
            }
        }
    }

    void OnDrawGizmos(){
        for (int i = 0; i < b.Length; i++){
            Gizmos.DrawWireCube(b[i].center, b[i].size);
        }
    }

        double rnd(double a, double b)
    {
        return a + r.NextDouble() * (b - a);
    }
}