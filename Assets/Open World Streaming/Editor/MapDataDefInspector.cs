/****************************************************
    文件：MapDataDefInspector.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDataDef))]
public class MapDataDefInspector : Editor
{
    public GameObject parentMap;
    MapDataDef mapDataDef;

    private void OnEnable()
    {
        mapDataDef = (MapDataDef)target;
    }

    public override void OnInspectorGUI()
    {
        parentMap=(GameObject)EditorGUILayout.ObjectField(parentMap,typeof(GameObject),true);
        EditorGUILayout.LabelField("Number of object: " + mapDataDef.mapObjects.Count.ToString());

        if (GUILayout.Button("Record Map Objects"))
        {
            mapDataDef.RecordObjects(mapDataDef,parentMap);
        }

        if (GUILayout.Button("Instantiate MapObjects"))
        {
            foreach (var item in mapDataDef.mapObjects)
            {
                mapDataDef.InstantiateMapObject(item);
            }
        }
    }
}
