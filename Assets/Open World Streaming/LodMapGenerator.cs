using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LodMapMgr
{
    public class LodMapGenerator : MonoBehaviour
    {
        public static bool IsDebug = true;
        const float viewerMoveThresholdForChunkUpdate = 25f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        public int colliderLODIndex;
        /// <summary>
        /// Lod分级信息
        /// </summary>
        public LODInfo[] detailLevels;

        public Transform viewer;
        Vector2 viewerPosition;
        Vector2 viewerPositionOld;

        //地块尺寸大小
        float meshWorldSize;
        /// <summary>
        /// 视野内可见距离内的块数
        /// </summary>
        int chunksVisibleInViewDst;

        Dictionary<Vector2, LodMapChunk> terrainChunkDictionary = new Dictionary<Vector2, LodMapChunk>();
        //可见地块
        List<LodMapChunk> visibleTerrainChunks = new List<LodMapChunk>();

        public Action<LodMapChunk, int> OnMapChunkLodChanged;
        public Action<LodMapChunk> OnMapChunkUnload;

        void Start()
        {
            float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            meshWorldSize = 98;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
            UpdateVisibleChunks();
        }

        void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            //if (viewerPosition != viewerPositionOld)
            //{
            //    foreach (LodMapChunk chunk in visibleTerrainChunks)
            //    {
            //        chunk.UpdateCollisionMesh();
            //    }
            //}

            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                viewerPositionOld = viewerPosition;
                UpdateVisibleChunks();
            }
        }

        void UpdateVisibleChunks()
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            //当前所在地块坐标
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

            for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    string dicKey = $"{currentChunkCoordX + xOffset}_{currentChunkCoordY + yOffset}";
                    if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                    {
                        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        {
                            terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                        else
                        {
                            //确保生成的都是能看见的，否则会造成多余生成
                            Vector2 position = viewedChunkCoord * meshWorldSize;
                            Bounds bounds = new Bounds(position, Vector2.one * meshWorldSize);
                            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                            float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
                            if (viewerDstFromNearestEdge <= maxViewDst)
                            {
                                LodMapChunk newChunk =
                                    new LodMapChunk(dicKey, viewedChunkCoord, meshWorldSize, detailLevels,
                                                                    colliderLODIndex, transform, viewer);
                                terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                                newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                                newChunk.onLodChanged += OnTerrainChunkLodChanged;
                                newChunk.UpdateTerrainChunk();

                            }
                        }
                    }

                }
            }

            Debug.Log($"ttt ChunkDic: {terrainChunkDictionary.Count}");
            Debug.Log($"ttt visual: {visibleTerrainChunks.Count}");
        }
        public Material[] mats;
        /// <summary>
        /// Lod层级更新回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnTerrainChunkLodChanged(LodMapChunk arg1, int arg2)
        {
            if(IsDebug)
                arg1.meshObject.gameObject.GetComponent<MeshRenderer>().material = mats[arg2];

            if (OnMapChunkLodChanged!=null)
            {
                OnMapChunkLodChanged(arg1, arg2);
            }

            Debug.Log($"ttt lod changed :{arg1.meshObject.gameObject.name}_{arg2}");
        }

        //这里的isVisible是变得可见，不可见的时候会删掉销毁
        void OnTerrainChunkVisibilityChanged(LodMapChunk chunk, bool isVisible)
        {

            //Debug.Log("ttt" + chunk.meshObject.name);

            if (isVisible)
            {
                Debug.Log($"visiable: {chunk.meshObject.gameObject.name}");
                visibleTerrainChunks.Add(chunk);
            }
            else
            {
                Debug.Log($"ttt unload:  {chunk.meshObject.gameObject.name}");
                if (OnMapChunkUnload!=null)
                {
                    OnMapChunkUnload(chunk);
                }
                chunk.UnLoad();
                visibleTerrainChunks.Remove(chunk);
                terrainChunkDictionary.Remove(chunk.coord);
                //Debug.Log("ttt visual" + visibleTerrainChunks.Count);
            }
        }

    }

    /// <summary>
    /// Lod信息
    /// </summary>
    [System.Serializable]
    public struct LODInfo
    {
        /// <summary>
        /// Lod层级ID
        /// </summary>
        public int lod;
        /// <summary>
        /// 该lod层级下可见距离
        /// </summary>
        public float visibleDstThreshold;


        public float sqrVisibleDstThreshold
        {
            get
            {
                return visibleDstThreshold * visibleDstThreshold;
            }
        }
    }

}