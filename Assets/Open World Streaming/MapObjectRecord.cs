/****************************************************
    文件：MapObjectRecord.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct MapObjectRecord
{
    public string objectName;
    public Vector3 objectPosition;
    public Quaternion objectRotation;

    public MapObjectRecord(Transform mapObjRef)
    {
        objectName = mapObjRef.name;
        objectPosition = mapObjRef.position;
        objectRotation = mapObjRef.rotation;
    }
}

//使用
[CreateAssetMenu(fileName = "NewMapData", menuName = "Map/MapData"), Serializable]
public class MapDataDef : ScriptableObject
{
    public List<MapObjectRecord> mapObjects;

    private void Awake()
    {
        if (mapObjects == null)
        {
            mapObjects = new List<MapObjectRecord>();
        }
    }

    public void RecordObjects(MapDataDef mapDataDef,GameObject root)
    {
        Transform[] objs = root.GetComponentsInChildren<Transform>();
        mapDataDef.mapObjects.Clear();
        foreach (var item in objs)
        {
            if (item.name == root.name)
            {
                continue;
            }
            MapObjectRecord temp = new MapObjectRecord(item);
            mapDataDef.mapObjects.Add(temp);
        }
    }

    public void AddObject(MapObjectRecord mapObjectRecord)
    {
        mapObjects.Add(mapObjectRecord);
    }

    public GameObject InstantiateMapObject(MapObjectRecord mapObjectRecord)
    {
        GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map" + mapObjectRecord.objectName);
        if (temp == null)
        {
            return null;
        }
        temp = Instantiate(temp);
        temp.transform.position = mapObjectRecord.objectPosition;
        temp.transform.rotation = mapObjectRecord.objectRotation;
        temp.name = mapObjectRecord.objectName;
        return temp;
    }

}
