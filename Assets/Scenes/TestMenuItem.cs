using Autodesk.Fbx;
using SanAndreasUnity.Behaviours.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using TreeEditor;
using UGameCore.Utilities;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

[ExecuteInEditMode]
public class TestMenuItem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            //StartCoroutine(exportFbx());
            StaticGeometry[] geos = GetComponentsInChildren<StaticGeometry>();
            foreach (var item in geos)
            {
                item.Show(UnityEngine.Random.Range(0, 100));
            }
        }
    }

    /// <summary>
    /// 会将Transform下的所有子物体都导出FBX
    /// </summary>
    /// <returns></returns>
    IEnumerator exportFbx()
    {
        for (int i = 0, cnt = transform.childCount; i < cnt; i++)
        {
            GameObject go = transform.GetChild(i).gameObject;
            var filename = go.name;
            var folderPath = Application.dataPath + "/GTA_SA/Models/";
            var filePath = System.IO.Path.Combine(folderPath, filename + ".fbx");

            if (System.IO.File.Exists(filePath))
            {
                Debug.LogErrorFormat("Failed to export to {1}, file already exists", filePath);
                yield return null;
            }

            GameObject[] toExport = new GameObject[] { go };
            ExportModelSettingsSerialize option =
            new ExportModelSettingsSerialize();
            option.SetAnimationSource(go.transform);
            option.SetAnimationDest(go.transform);

            Debug.Log(option.ExportFormat.ToString());
            Debug.Log(option.ModelAnimIncludeOption.ToString());
            Debug.Log(option.LODExportType.ToString());
            Debug.Log(option.ObjectPosition.ToString());
            Debug.Log(option.AnimateSkinnedMesh.ToString());
            Debug.Log(option.UseMayaCompatibleNames.ToString());
            Debug.Log(option.ExportUnrendered.ToString());
            Debug.Log(option.PreserveImportSettings.ToString());

            if (ModelExporter.ExportObjects(filePath, toExport, option) != null)
            {
                // refresh the asset database so that the file appears in the
                // asset folder view.
                AssetDatabase.Refresh();

                //替换选中的模型Mesh=======================
                //GameObject obj =
                //    (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/" + filename + ".fbx");
                //if (obj == null)
                //{
                //    Debug.Log("Not find fbx");
                //    yield return null;
                //}

                //go.GetComponent<MeshFilter>().sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;

                //替换选中的模型Mesh End=======================

                Debug.Log("OnExportOK: " + filePath);
            }
        }
        yield return null;

    }

}
