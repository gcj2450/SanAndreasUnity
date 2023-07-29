/****************************************************
    文件：MapDataLoader.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class MapDataLoader : MonoBehaviour
{
    List<Vector3> mapDatas;
    public float xStart, yStart, xEnd, yEnd,step;
    public float loadTolerance;
    List<Vector3> loadedMapDatas;
    Dictionary<Vector3, MapDataStreamer> mapDataStreamer = new Dictionary<Vector3, MapDataStreamer>();
    
    // Start is called before the first frame update
    void Start()
    {
        mapDatas = new List<Vector3>();
        loadedMapDatas = new List<Vector3>();

        for (float x = xStart; x <=xEnd; x+=step)
        {
            for (float y = xStart; y <=yEnd; y+=step)
            {
                mapDatas.Add(new Vector3(x, 0, y));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vector3 item in mapDatas)
        {
            if (IsVector3InArea(transform.position,item,loadTolerance)&&
                !loadedMapDatas.Contains(item))
            {
                loadedMapDatas.Add(item);
                mapDataStreamer.Add(item, new MapDataStreamer("Assets/MapData/MapX"+
                    item.x+"Y"+item.z+".asset"));
            }
            if (!IsVector3InArea(transform.position,item,loadTolerance)&&
                loadedMapDatas.Contains(item))
            {
                mapDataStreamer[item].Destroy();
                loadedMapDatas.Remove(item);
                mapDataStreamer.Remove(item);
            }
        }
    }

    private bool IsVector3InArea(Vector3 refPosition, Vector3 lowerBound, Vector3 upperBound)
    {
        return (refPosition.x >= lowerBound.x && refPosition.x <= upperBound.x &&
            refPosition.z >= lowerBound.z && refPosition.z <= upperBound.z);
    }

    private bool IsVector3InArea(Vector3 refPosition, Vector3 point, float range )
    {
        return Vector3.Distance(refPosition, point) < range;
    }
}

public class MapDataStreamer
{
    public string mapData;
    public GameObject environment;
    private List<GameObject> loadedGameObjects = new List<GameObject>();

    public MapDataStreamer(string mapDataAsset)
    {
        mapData = mapDataAsset;
        environment = new GameObject("environment");
        InstantiateAsync();
    }

    private void InstantiateAsync()
    {
        //从addressable中生成MapDataDef的asset，然后生成出MapDataDef中的每个mapObjects；
        //物体的父节点是environment
        //并把生成的每个mapObjects中的物体加到loadedGameObjects；
    }

    //判断Addressable中是否有某个key的物体
    //public static void AddressableResourceExists(string key)
    //{
    //    foreach (var item in Addressables.ResourceLocators)
    //    {
    //        IList<IResourceLoacation> locs;
    //        if (item.Locate(key,typeof(GameObject),out locs))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public void Destroy()
    {
        //foreach (var item in loadedGameObjects)
        //{
        //    if (item!=environment)
        //    {
        //        Addressables.ReleaseInstance(item);
        //    }
        //    GameObject.Destroy(environment);
        //}
    }
}
