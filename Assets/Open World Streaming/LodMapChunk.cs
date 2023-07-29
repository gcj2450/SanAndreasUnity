using System.Buffers;
using UnityEditor.Rendering;
using UnityEngine;

namespace LodMapMgr
{
    public class LodMapChunk
    {

        const float colliderGenerationDistanceThreshold = 5;
        //可见性变更事件
        public event System.Action<LodMapChunk, bool> onVisibilityChanged;
        //lod层级改变事件
        public event System.Action<LodMapChunk, int> onLodChanged;
        public Vector2 coord;
        public GameObject meshObject;
        public string coordKey = "";
        Bounds bounds;

        LODInfo[] detailLevels;
        //LODMesh[] lodMeshes;
        //int colliderLODIndex;

        /// <summary>
        /// Update完之后就是当前自己的LodIndex
        /// </summary>
        int previousLODIndex = -1;
        bool hasSetCollider;
        float maxViewDst;

        Transform viewer;

        public LodMapChunk(string coordKey, Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer)
        {
            this.coordKey = coordKey;
            this.coord = coord;
            this.detailLevels = detailLevels;
            //this.colliderLODIndex = colliderLODIndex;
            this.viewer = viewer;
            Vector2 position = coord * meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshWorldSize);

            if (LodMapGenerator.IsDebug)
            {
                meshObject = GameObject.CreatePrimitive(PrimitiveType.Cube); //
                meshObject.name = coordKey;
            }
            else
                meshObject= new GameObject($"Terrain Chunk_{coord.x}_{coord.y}");
            Debug.Log($"nnnnnnnn: {meshObject.name}");
            meshObject.transform.parent = parent;
            meshObject.transform.localPosition = new Vector3(position.x, 0, position.y);

            meshObject.transform.localScale = new Vector3(98, 1, 98);


            meshObject.tag = "Ground";

            SetVisible(false);

            //lodMeshes = new LODMesh[detailLevels.Length];
            //for (int i = 0; i < detailLevels.Length; i++)
            //{
            //    //lodMeshes[i] = new LODMesh(detailLevels[i].lod);

            //    //lodMeshes[i].updateCallback += UpdateTerrainChunk;
            //    //if (i == colliderLODIndex)
            //    //{
            //    //    lodMeshes[i].updateCallback += UpdateCollisionMesh;
            //    //}
            //}

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        }

        //public void Load()
        //{
        //    Debug.Log("gcj: LoadLoadLoadLoadLoadLoadLoadLoadLoadLoadLoadLoadLoad" + meshObject.gameObject.name);
        //    UpdateTerrainChunk();
        //}

        public void UnLoad()
        {
            //Debug.Log("UnLoad: "+meshObject.gameObject.name);
            GameObject.Destroy(meshObject.gameObject);
        }

        Vector2 viewerPosition
        {
            get
            {
                return new Vector2(viewer.position.x, viewer.position.z);
            }
        }

        public void UpdateTerrainChunk()
        {
            //最近的边到视点的距离
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                //找出当前块所属Lod层级
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    //LODMesh lodMesh = lodMeshes[lodIndex];
                    //if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        if (onLodChanged != null)
                        {
                            onLodChanged(this, lodIndex);
                        }
                    }
                    //else if (!lodMesh.hasRequestedMesh)
                    //{
                    //    lodMesh.RequestMesh();
                    //}

                }

                ////bounds有该片的尺寸和位置，heightMap有每个点的高度
                //int row = heightMap.values.GetLength(0); //行数
                //int col = heightMap.values.GetUpperBound(heightMap.values.Rank - 1) + 1; //列数，Rank为维数
                //for (int i = 0; i < row; i++)
                //{
                //    for (int j = 0; j < col; j++)
                //    {
                //        Vector3 rndPos = bounds.center + bounds.size * Random.Range(0, 1);
                //        rndPos.y = heightMap.values[i, j];
                //        ChangeUIEventArgs args = new ChangeUIEventArgs("OnSpawnItem", rndPos);
                //        App.Instance.EventManager.SendEvent(args);
                //    }
                //}
                //Debug.Log(bounds.center+"__"+bounds.size+"___"+ row + "__"+ col);
            }

            //if (wasVisible&&visible==false)
            //{
            //    Debug.Log("==========" + meshObject.gameObject.name);
            //}

            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if (meshObject != null)
                meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            if (meshObject == null)
                return false;
            else
                return meshObject.activeSelf;
        }

    }

    //class LODMesh
    //{
    //    //public bool hasRequestedMesh;
    //    //public bool hasMesh = false;
    //    public int lod;
    //    //public event System.Action updateCallback;

    //    public LODMesh(int lod)
    //    {
    //        this.lod = lod;
    //    }

    //    //public void RequestMesh()
    //    //{
    //    //    hasMesh = true;
    //    //    hasRequestedMesh = true;
    //    //    //if(updateCallback!=null)
    //    //    //    updateCallback();

    //    //}

    //}
}