using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class OctreeDataStructure{

    //encapsulating area of this node
    public Bounds nodeArea;
    //array of objects contained within this.octreenode
    public OctreeObject[] nodeItems;
    //array of pointers for each[8] possible child node
    public OctreeDataStructure[] childNodes = new OctreeDataStructure[8];
    //smallest size for subdivison of bounding box --max list size for bottom nodes will be 10
    public static float minSizeSubdivision = 0.000001f;

    public static int rootCounter = 0;

    //Constructor for non-root nodes
    public OctreeDataStructure(Bounds nodeArea0, OctreeObject[] nodeItems0){

        nodeArea = nodeArea0;
        nodeItems = nodeItems0;
    }

    //Constructor for root node at boundingbox area (min)through(max)
    public OctreeDataStructure(OctreeObject[] nodeItems0){
        Debug.Log("Construtor for root");
        Vector3 min     = new Vector3(-100000.0f, -100000.0f, -100000.0f);
        Vector3 max     = new Vector3(100000.0f, 100000.0f, 100000.0f);
        nodeArea = new Bounds();
        nodeArea.SetMinMax(min,max);
        rootCounter++;
        nodeItems = nodeItems0;
    }

    public OctreeDataStructure() { }

    //Checking that we dont go deeper that minimum size
    private bool checkMinSize(Vector3 aabb){
        return ((aabb.x <= minSizeSubdivision) && (aabb.y <= minSizeSubdivision) && (aabb.z <= minSizeSubdivision));
    }

    //subdivide current aabb into 8 octants
    //if we look from the top view of cube in scope starting at the top left and working clockwise regions[0-3]
    //second layer follows same conventions for regions[3-7]
    public Bounds[] divideAABB(Bounds currentBounds){

        Bounds[] regions = new Bounds[8];
        for(int i=0;i<regions.Length;i++){
            regions[i] = new Bounds();
        } 
        Vector3 center = currentBounds.min + ((currentBounds.max - currentBounds.min)/2f);

        regions[0].SetMinMax(currentBounds.min,center);
        regions[1].SetMinMax(new Vector3(center.x, currentBounds.min.y, currentBounds.min.z),     new Vector3(currentBounds.max.x, center.y, center.z));
        regions[2].SetMinMax(new Vector3(center.x, currentBounds.min.y, center.z),                new Vector3(currentBounds.max.x,center.y, currentBounds.max.z));
        regions[3].SetMinMax(new Vector3(currentBounds.min.x, currentBounds.min.y,center.z),      new Vector3(center.x, center.y, currentBounds.max.z));
        regions[4].SetMinMax(new Vector3(currentBounds.min.x, center.y, currentBounds.min.z),     new Vector3(center.x, currentBounds.max.y, center.z));
        regions[5].SetMinMax(new Vector3(center.x, center.y, currentBounds.min.z),                new Vector3(currentBounds.max.x, currentBounds.max.y, center.z));
        regions[7].SetMinMax(new Vector3(currentBounds.min.x, center.y, center.z),                new Vector3(center.x, currentBounds.max.y, currentBounds.max.z));
        regions[6].SetMinMax(center,currentBounds.max);

        return regions;
    }
  
    //Build the octree
    public void init(){

        if (!(nodeItems.Length > 1)){return;} //Base case for bottom node

        Vector3 diagOfAABB = nodeArea.max - nodeArea.min; //Getting diagnol vector from xyz.min to xyz.max

        if (checkMinSize(diagOfAABB)){return;} //Base case for not dividing any smaller
            
        Bounds[] octants = divideAABB(this.nodeArea);

        //Creating jagged array to hold child lists
        List<OctreeObject>[] subLists = new List<OctreeObject>[8];  
        for(int i = 0; i < 8; i++){
            subLists[i] = new List<OctreeObject>();
        }

        List<OctreeObject> tempList = new List<OctreeObject>();

        //Adding containing points to correlating children
        foreach (OctreeObject nodeObject in nodeItems){
            bool isChildObject = false;

            for(int i=0;i<8;i++){
                if (octants[i].Contains(nodeObject.spatialPosition)){
                    subLists[i].Add(nodeObject);
                    isChildObject = true;
                    break;
                }
            }
            if (isChildObject == false){
                tempList.Add(nodeObject);
            }
        }
        nodeItems = tempList.ToArray();
        tempList.Clear();

        //Creating children for each jagged array element with items
        for (int i=0;i<8;i++){
            if (subLists[i].Count > 0){
                childNodes[i] = createChild(octants[i],subLists[i].ToArray());
                childNodes[i].init();
            }
        }
    }

    //creating child node that != leaf node
    private OctreeDataStructure createChild(Bounds nodeArea, OctreeObject[] containingObjects){
        if (containingObjects.Length>=1){
            return new OctreeDataStructure(nodeArea,containingObjects);
        }
        return null;
    }

    //creating leaf node
    private OctreeDataStructure createChild(Bounds nodeArea, OctreeObject pointObject){
        return new OctreeDataStructure(nodeArea, new OctreeObject[1] {pointObject});     
    }

    //Frustum culling
    public void frustumCulling(List<OctreeObject[]> visibleList, Plane[] p){
        if (GeometryUtility.TestPlanesAABB(p, nodeArea)){

            visibleList.Add(nodeItems);

            for (int i=0;i<childNodes.Length;i++){
                if (childNodes[i] != null){
                    childNodes[i].frustumCulling(visibleList,p);
                }              
            }
        }
    }

    public void countCulling(int count, Plane[] p){
        if (GeometryUtility.TestPlanesAABB(p, nodeArea)) {
            count=count+nodeItems.Length;
            for (int i = 0; i < childNodes.Length; i++){
                if (childNodes[i] != null){
                    childNodes[i].countCulling(count, p);
                }
            }
        }
    }
}
//Class to hold voxel like structure to be placed into Octree
public class OctreeObject {
    public Vector3 spatialPosition;
    public float colourScheme;
    public float size;

    public OctreeObject(Vector3 spatialPosition0, float colourScheme0, float size0){

        spatialPosition = spatialPosition0;
        colourScheme    = colourScheme0;
        size            = size0;
    }
    public OctreeObject() {}
}