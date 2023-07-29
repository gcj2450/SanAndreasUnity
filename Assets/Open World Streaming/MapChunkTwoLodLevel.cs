/****************************************************
    文件：MapChunk.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using BDWorldSystem.Utilities;
using LodMapMgr;
using System;
using System.Collections;
using System.Collections.Generic;
using UGameCore.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class MapChunkTwoLodLevel : MonoBehaviour
{

    public LodMapGenerator MapGenerator;

    MapChunkInfo lod0Chunk;
    MapChunkInfo lod1Chunk;

    private void Awake()
    {
        lod0Chunk = JsonStringConverter.Instance.BinaryFileToClass<MapChunkInfo>(Application.dataPath + "/"+"Lod" + 0 + ".dat");
        lod1Chunk = JsonStringConverter.Instance.BinaryFileToClass<MapChunkInfo>(Application.dataPath + "/"+"Lod" + 1 + ".dat");

        Debug.Log(lod0Chunk.ObjectsInChunk.Count);
        Debug.Log(lod1Chunk.ObjectsInChunk.Count);
        MapGenerator.OnMapChunkLodChanged += OnMapChunkLodChanged;
        MapGenerator.OnMapChunkUnload += OnMapChunkUnload;
    }

    Dictionary<string, List<GameObject>> lod0ChunkObjects = new Dictionary<string, List<GameObject>>();
    Dictionary<string, List<GameObject>> lod1ChunkObjects = new Dictionary<string, List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnMapChunkUnload(LodMapChunk obj)
    {
    }

    private void OnMapChunkLodChanged(LodMapChunk arg1, int arg2)
    {
        if (arg2==0)
        {
            if (lod1ChunkObjects.ContainsKey(arg1.coordKey))
            {
                foreach (var item in lod1ChunkObjects[arg1.coordKey])
                {
                    GameObject.Destroy(item);
                }
            }

            if (!lod0ChunkObjects.ContainsKey(arg1.coordKey))
            {
                lod0ChunkObjects.Add(arg1.coordKey, new List<GameObject>());
            }

            if (lod0Chunk.ObjectsInChunk.ContainsKey(arg1.coordKey))
            {
                foreach (var item in lod0Chunk.ObjectsInChunk[arg1.coordKey])
                {
                    lod0ChunkObjects[arg1.coordKey].Add(InstanObj(item));
                }
            }
        }
        else if (arg2==1)
        {
            if (lod0ChunkObjects.ContainsKey(arg1.coordKey))
            {
                foreach (var item in lod0ChunkObjects[arg1.coordKey])
                {
                    GameObject.Destroy(item);
                }
            }

            if (!lod1ChunkObjects.ContainsKey(arg1.coordKey))
            {
                lod1ChunkObjects.Add(arg1.coordKey, new List<GameObject>());
            }

            if (lod1Chunk.ObjectsInChunk.ContainsKey(arg1.coordKey))
            {
                foreach (var item in lod1Chunk.ObjectsInChunk[arg1.coordKey])
                {
                    lod1ChunkObjects[arg1.coordKey].Add(InstanObj(item));
                }
            }
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
        if (go.GetComponent<MeshRenderer>()!=null)
        {
            go.GetComponent<MeshRenderer>().enabled = true;
        }
        go.GetComponent<ObjectSaverComp>().ObjectSaverData = objectSaver;
        return go;
    }

}
