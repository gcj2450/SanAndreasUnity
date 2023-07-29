using BDWorldSystem;
using LodMapMgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UGameCore.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DivideScene : EditorWindow
{
    float xStart, yStart, step, xEnd, yEnd;
    GameObject environment;
    string dataFileName = "Lod";
    int lodLevel =0;

    [MenuItem("SceneChunk", menuItem = "Tools/DivideScene")]
    public static void ShowWindow()
    {
        GetWindow<DivideScene>();
    }

    private void OnGUI()
    {
        dataFileName = EditorGUILayout.TextField("DataFileName: ", dataFileName);
        lodLevel = EditorGUILayout.IntField("LodLevel: ", lodLevel);
        EditorGUILayout.LabelField("Position of the bottom left corner:");
        EditorGUILayout.Space(2);
        xStart = EditorGUILayout.FloatField("x start: ", xStart);
        yStart = EditorGUILayout.FloatField("y start: ", yStart);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Position of the top right corner:");
        EditorGUILayout.Space(2);
        xEnd = EditorGUILayout.FloatField("x end: ", xEnd);
        yEnd = EditorGUILayout.FloatField("y end: ", yEnd);
        EditorGUILayout.Space(10);
        step = EditorGUILayout.FloatField("step: ", step);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("environment");
        environment = (GameObject)EditorGUILayout.
            ObjectField(environment, typeof(GameObject), true);

        //按块分隔场景
        if (GUILayout.Button("Chunk the map"))
        {
            //PlaceObjectsIntoChunks();
            ChunkObjects();
        }
        if (GUILayout.Button("Create map data"))
        {
            //将每块分隔的场景放到一个MapDataDef内
            CreateMapDatas();
        }
        if (GUILayout.Button("Create Scene"))
        {
            CreateScene();
        }

    }

    /// <summary>
    /// 创建空场景并把物体放进去
    /// </summary>
    private void CreateScene()
    {
        MapChunkInfo lod0Chunk = JsonStringConverter.Instance.BinaryFileToClass<MapChunkInfo>(Application.dataPath + "/" + "SanAndreas_Lod" + 2 + ".dat");
        foreach (var item in lod0Chunk.ObjectsInChunk)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            scene.name = item.Key;
            InstMapObjects(item.Value);
            EditorSceneManager.SaveScene(scene, $"Assets/SplitScenes/Lod_{2}_{item.Key}.unity");
        }
        //MapDataDef[] mapDatas = new MapDataDef(Selection.objects.Length);
        //for (int j = 0; j < mapDatas.Length; j++)
        //{
        //    mapDatas[j] = (MapDataDef)Selection.objects[j];
        //}

        //foreach (var item in mapDatas)
        //{
        //    Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        //    scene.name = item.name;
        //    item.InstantiateMapObject();
        //    EditorSceneManager.SaveScene(scene, "Assets/Scenes/Map/" + item.name + ".unity");
        //}
    }

    void InstMapObjects(List<ObjectSaver> objSaves)
    {
        foreach (var item in objSaves)
        {
            InstanObj(item);
        }
    }

    GameObject InstanObj(ObjectSaver objectSaver)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GTA_SA/Prefabs/" + objectSaver.ModelName + ".prefab");
        GameObject go = Instantiate(prefab);
        go.transform.localPosition = new Vector3(objectSaver.LoacalPositionX, objectSaver.LoacalPositionY, objectSaver.LoacalPositionZ);
        go.transform.localRotation = new Quaternion(objectSaver.LoacalRotationX, objectSaver.LoacalRotationY, objectSaver.LoacalRotationZ, objectSaver.LoacalRotationW);
        go.transform.localScale = Vector3.one;
        go.name = objectSaver.ModelName;
        if (go.GetComponent<MeshRenderer>() != null)
        {
            go.GetComponent<MeshRenderer>().enabled = true;
        }
        go.GetComponent<ObjectSaverComp>().ObjectSaverData = objectSaver;
        return go;
    }

    private void ChunkObjects()
    {
        Dictionary<string, GameObject> rootDic = new Dictionary<string, GameObject>();
        Dictionary<string, List<ObjectSaver>> chunkDicList = new Dictionary<string, List<ObjectSaver>>();
        Transform[] childs = environment.GetFirstLevelChildrenComponents<Transform>().ToArray();

        MapChunkInfo mapChunkInfo = new MapChunkInfo(lodLevel);
        for (int j = 0; j < childs.Length; j++)
        {
            if (childs[j].gameObject != environment)
            {
                int signx = childs[j].position.x < 0 ? -1 : 1;
                int signy = childs[j].position.z < 0 ? -1 : 1;
                int currentChunkCoordX = signx * Mathf.RoundToInt(Mathf.Abs(childs[j].position.x) / step);
                int currentChunkCoordY = signy * Mathf.RoundToInt(Mathf.Abs(childs[j].position.z) / step);

                string dicKey = $"{currentChunkCoordX}_{currentChunkCoordY}";
                GameObject parentRoot;
                if (!rootDic.ContainsKey(dicKey))
                {
                    rootDic.Add(dicKey, new GameObject(dicKey));
                    chunkDicList.Add(dicKey, new List<ObjectSaver>());
                }
                parentRoot = rootDic[dicKey];
                chunkDicList[dicKey].Add(childs[j].GetComponent<ObjectSaverComp>().ObjectSaverData);
                //parentRoot.transform.localPosition = new Vector3(currentChunkCoordX*step,0, currentChunkCoordY*step);
                childs[j].SetParent(parentRoot.transform);
            }
        }
        mapChunkInfo.ObjectsInChunk = chunkDicList;

        foreach (var item in chunkDicList)
        {
            if (rootDic[item.Key].transform.GetFirstLevelChildren().Count()!= item.Value.Count)
            {
                Debug.Log(item.Key+"_"+ rootDic[item.Key].transform.GetFirstLevelChildren().Count()+"__"+ item.Value.Count);
            }
        }

        JsonStringConverter.Instance.ClassToBinarySave (Application.dataPath + "/" , dataFileName + lodLevel + ".dat",mapChunkInfo);
    }

    private void CreateMapDatas()
    {
        foreach (var item in Selection.gameObjects)
        {
            MapDataDef mapDataDef = ScriptableObject.CreateInstance<MapDataDef>();
            string fileName = item.name;
            mapDataDef.RecordObjects(mapDataDef, item);

            AssetDatabase.CreateAsset(mapDataDef, "Asstes/MapData/" + fileName + ".asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void PlaceObjectsIntoChunks()
    {
        Transform[] childs;
        for (float x = xStart; x <= xEnd; x += step)
        {
            for (float y = yStart; y <= yEnd; y += step)
            {
                int signx = x < 0 ? -1 : 1;
                int signy = y < 0 ? -1 : 1;
                int currentChunkCoordX = signx * Mathf.RoundToInt(Mathf.Abs(x) / step);
                int currentChunkCoordY = signy * Mathf.RoundToInt(Mathf.Abs(y) / step);
                GameObject parentMap =
                    new GameObject($"{currentChunkCoordX}_{currentChunkCoordY}");
                //parentMap.transform.position = new Vector3(x, 0, y);
                childs = environment.GetFirstLevelChildrenComponents<Transform>().ToArray();
                for (int j = 0; j < childs.Length; j++)
                {
                    //MeshRenderer temp;
                    //if (childs[j].gameObject != environment && 
                    //    childs[j].TryGetComponent<MeshRenderer>(out temp))
                    if (childs[j].gameObject != environment)
                    {
                        if (IsVector3InArea(childs[j].position,
                            new Vector3(x - step * 0.5f, 0, y - step * 0.5f),
                            new Vector3(x + step * 0.5f, 0, y + step * 0.5f)))
                        {
                            childs[j].parent = parentMap.transform;
                        }
                    }
                }
            }
        }
    }

    private bool IsVector3InArea(Vector3 refPosition, Vector3 lowerBound, Vector3 upperBound)
    {
        return (refPosition.x >= lowerBound.x && refPosition.x <= upperBound.x &&
            refPosition.z >= lowerBound.z && refPosition.z <= upperBound.z);
    }
}
