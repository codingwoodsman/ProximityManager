using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityManager : MonoBehaviour
{
    [SerializeField]
    int ANIMATING_DISTANCE;

    GameObject mainCamera;
    List<GameObject> npcs;
    List<List<Mesh>> npcMeshes;
    List<List<GameObject>> npcMeshesGO;
    GameObject meshTemplate;
    GameObject meshTemplate2;
    GameObject meshTemplate3;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("Main Camera");
        GameObject[] GOList = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));

        //List of GameObjects that are parent to the skinned mesh renderer component gameObjects
        npcs = new List<GameObject>();

        //A list of lists of meshes. Each list represents a collection of meshes (hair, lod, weapon etc.)
        //One list of meshes per animated model
        npcMeshes = new List<List<Mesh>>();

        //GameObjects associated with each list of meshes (probably 1 per animated model)
        npcMeshesGO = new List<List<GameObject>>();

        //Template with components and materials for building a new object easily
        //Template gameObject must be in scene and named "MeshTemplate"
        meshTemplate = GameObject.Find("MeshTemplate");

        //2nd Template with components and materials for building a new object easily
        //Template gameObject must be in scene and named "MeshTemplate2"
        //If animated object has components with more than 2 materials, then add MeshTemplate3 etc.
        meshTemplate2 = GameObject.Find("MeshTemplate2");

        //3nd Template with components and materials for building a new object easily
        meshTemplate3 = GameObject.Find("MeshTemplate3");

        int j = 0;

        for(int i = 0; i < GOList.Length; i++)
        {
            if(GOList[i].name.Contains("nackedSet"))
            {
                npcs.Add(GOList[i]);

                Mesh tempMesh = new Mesh();

                //if the list doesn't exist yet, make it
                if (npcMeshes.Count < j + 1)
                {
                    npcMeshes.Add(new List<Mesh>());
                    npcMeshesGO.Add(new List<GameObject>());
                }

                //for every child in the gameObject we are meshifying, create a new mesh slot
                //if there is a skinned mesh renderer 
                for(int k = 0; k < npcs[j].transform.childCount; k++)
                {
                    if(GOList[i].transform.GetChild(k).GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        tempMesh.name = "MyMesh" + j + "-" + k;
                        npcMeshes[j].Add(tempMesh);
                        GameObject tempGO = new GameObject("Mesh" + j + "-" + k);
                        tempGO.AddComponent<MeshFilter>();
                        tempGO.AddComponent<MeshRenderer>();
                        tempGO.SetActive(false);
                        npcMeshesGO[j].Add(tempGO);
                    }
                }
           
                if(Vector3.Distance(mainCamera.transform.position, GOList[i].transform.position) > ANIMATING_DISTANCE)
                {
                    BakeIt(GOList[i], j);
                }
                else
                {
                    //do nothing
                }

                j++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //test distance between camera and npcs
        for(int i = 0; i < npcs.Count; i++)
        {
            if(npcs[i].activeSelf == true)
            {
                if(Vector3.Distance(mainCamera.transform.position, npcs[i].transform.position) > ANIMATING_DISTANCE)
                {
                    BakeIt(npcs[i], i);
                }
            }
            else
            {
                if(Vector3.Distance(mainCamera.transform.position, npcs[i].transform.position) <= ANIMATING_DISTANCE)
                {
                    npcs[i].SetActive(true);
                    for(int j = 0; j < npcMeshesGO[i].Count; j++)
                    {
                        npcMeshesGO[i][j].SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Bakes the GameObject Passed in
    /// </summary>
    /// <param name="gO">GameObject to be baked</param>
    /// <param name="j">jth element</param>
    void BakeIt(GameObject gO, int j)
    {
        Vector3 npcLoc = gO.transform.position;

        //get animator and call the SampleAnimation function on the ith element
        Animator anim = gO.GetComponent<Animator>();
        anim.runtimeAnimatorController.animationClips[0].SampleAnimation(gO, 0);

        //bake mesh for each skinned mesh renderer component under the gameObject passed in as arg0
        SkinnedMeshRenderer[] listOSkins = gO.GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < listOSkins.Length; i++)
        {
            listOSkins[i].BakeMesh(npcMeshesGO[j][i].GetComponent<MeshFilter>().mesh);

            if(npcMeshesGO[j][i].name.Contains("-3") || npcMeshesGO[j][i].name.Contains("-6"))
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate2.GetComponent<MeshRenderer>().materials;
            }
            else if(npcMeshesGO[j][i].name.Contains("-5"))
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate3.GetComponent<MeshRenderer>().materials;
            }
            else
            {
                npcMeshesGO[j][i].GetComponent<MeshRenderer>().materials = meshTemplate.GetComponent<MeshRenderer>().materials;
            }

            //make the mesh and skinned mesh same loc
            gO.transform.position = npcLoc;
            npcMeshesGO[j][i].transform.position = gO.transform.position;
            npcMeshesGO[j][i].SetActive(true);
        }
        gO.SetActive(false);
    }
}
