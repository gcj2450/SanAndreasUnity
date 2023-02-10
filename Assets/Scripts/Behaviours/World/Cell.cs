﻿using System;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Placements;
using UGameCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Behaviours.WorldSystem;
using UnityEngine;
using Profiler = UnityEngine.Profiling.Profiler;
using UnityEngine.AI;

namespace SanAndreasUnity.Behaviours.World
{
    public class Cell : UGameCore.Utilities.SingletonComponent<Cell>
    {
	    private Dictionary<Instance, StaticGeometry> m_insts = new Dictionary<Instance, StaticGeometry>();
		/// <summary>
		/// 静态几何体实例字典
		/// </summary>
		public IReadOnlyDictionary<Instance, StaticGeometry> StaticGeometries => m_insts;
		/// <summary>
		/// 静态物体数
		/// </summary>
        public int NumStaticGeometries => m_insts.Count;
		/// <summary>
		/// 车辆数组
		/// </summary>
		private MapObject[] m_cars;
        private List<EntranceExitMapObject> m_enexes;

		/// <summary>
		/// 19个Cell Ids
		/// </summary>
		public IReadOnlyList<int> CellIds { get; } = Enumerable.Range(0, 19).ToList();

		public bool ignoreLodObjectsWhenInitializing = false;

        public bool HasMainExterior => this.CellIds.Contains(0);

        public Camera PreviewCamera;

		/// <summary>
		/// 注视点管理器
		/// </summary>
        public FocusPointManager<MapObject> FocusPointManager { get; private set; }

		/// <summary>
		/// 区域和距离
		/// </summary>
		private struct AreaWithDistance
		{
			public WorldSystem<MapObject>.Area area;
			public float distance;
		}

        /// <summary>
        /// AreaWithDistance比较器
        /// </summary>
        private class AreaWithDistanceComparer : IComparer<AreaWithDistance>
		{
			public int Compare(AreaWithDistance a, AreaWithDistance b)
			{
				if (a.distance < b.distance)
					return -1;

				if (a.distance > b.distance)
					return 1;

				// solve potential problem that 2 areas have same distance
				// - these areas may not be equal, they may belong to different world systems and have same index,
				// therefore their distances will be equal
				// - by comparing their ids, we are sure that comparer is always deterministic
				return a.area.Id.CompareTo(b.area.Id);
			}
		}

		/// <summary>
		/// 需要更新的区域
		/// </summary>
		private readonly SortedSet<AreaWithDistance> _areasToUpdate = new SortedSet<AreaWithDistance>(new AreaWithDistanceComparer());
		private readonly AreaWithDistance[] _bufferOfAreasToUpdate = new AreaWithDistance[64];
		private int _indexOfBufferOfAreasToUpdate = 0;
		private int _numElementsInBufferOfAreasToUpdate = 0;

		public ushort maxTimeToUpdatePerFrameMs = 10;

		private System.Diagnostics.Stopwatch _updateTimeLimitStopwatch = new System.Diagnostics.Stopwatch();

        public Water Water;

		public static Cell Instance => Singleton;

		public float divisionRefreshDistanceDelta = 20;

		private float _maxDrawDistance = 0;
		public float MaxDrawDistance
		{
			get => _maxDrawDistance;
			set
			{
				if (_maxDrawDistance == value)
					return;

				_maxDrawDistance = value;

				this.OnMaxDrawDistanceChanged();
			}
		}

		public float drawDistanceMultiplier = 1f;

		/// <summary>
		/// 默认地图尺寸6000*6000
		/// </summary>
		public int WorldSize => 6000; // current world size - in the future, this will be configurable
		public static int DefaultWorldSize => 6000;
		/// <summary>
		/// 每层绘制距离
		/// </summary>
        public float[] drawDistancesPerLayers = new float[] { 301, 801, 1501 };

        private WorldSystemWithDistanceLevels<MapObject> _worldSystem;
		/// <summary>
		/// 带距离层级的世界系统
		/// </summary>
        public WorldSystemWithDistanceLevels<MapObject> WorldSystem => _worldSystem;

        public uint yWorldSystemSize = 25000;
        public ushort yWorldSystemNumAreas = 3;

        public ushort[] xzWorldSystemNumAreasPerDrawDistanceLevel = { 100, 100, 100 };

		/// <summary>
		/// 商场等物体的Y轴偏移量
		/// </summary>
        public float interiorHeightOffset = 5000f;

        public float fadeRate = 2f;

        public bool loadParkedVehicles = true;

        public GameObject mapObjectActivatorPrefab;

		/// <summary>
		/// 静态几何体预制体
		/// </summary>
        public GameObject staticGeometryPrefab;

		/// <summary>
		/// 传送点预制体
		/// </summary>
        public GameObject enexPrefab;
		/// <summary>
		/// 光源预制体
		/// </summary>
        public GameObject lightSourcePrefab;

        public float lightScaleMultiplier = 1f;

		/// <summary>
		/// 红灯持续时间
		/// </summary>
        public float redTrafficLightDuration = 7;
		/// <summary>
		/// 黄灯持续时间
		/// </summary>
        public float yellowTrafficLightDuration = 2;
		/// <summary>
		/// 绿灯持续时间
		/// </summary>
        public float greenTrafficLightDuration = 7;

		/// <summary>
		/// 导航网格不可走的区域
		/// </summary>
		public static int NavMeshNotWalkableArea => NavMesh.GetAreaFromName("Not Walkable");

		private NavMeshData _navMeshData = null;
		private List<NavMeshBuildSource> _navMeshBuildSources = new List<NavMeshBuildSource>(1024);
		private AsyncOperation _navMeshUpdateAsyncOperation = null;
		private List<MapObject> _mapObjectsWithNavMeshToAdd = new List<MapObject>(128);

		[SerializeField] private bool m_generateNavMesh = false;
		[SerializeField] private float m_navMeshMinRegionArea = 10f;
		[SerializeField] private uint m_navMeshMaxJobWorkers = 1;
		[SerializeField] private int m_navMeshAsyncOperationPriority = 0;
		public float navMeshUpdateInterval = 2f;

		public float NavMeshUpdatePercentage => _navMeshUpdateAsyncOperation?.progress ?? 1f;
		public int NumMapObjectsWithNavMeshToAdd => _mapObjectsWithNavMeshToAdd.Count;



		protected override void OnSingletonAwake()
        {
			if (m_generateNavMesh)
            {
				_navMeshData = new NavMeshData(0);
				NavMesh.AddNavMeshData(_navMeshData);
				this.InvokeRepeating(nameof(this.UpdateNavMesh), this.navMeshUpdateInterval, this.navMeshUpdateInterval);
            }
        }


		private static string GetKey(StaticGeometry sg)
        {
			return $"{sg.SerializedObjectDefinitionId}_{sg.SerializedInstancePosition}_{sg.SerializedInstanceRotation}";
		}

		private static string GetKey(Instance inst)
		{
			return $"{inst.ObjectId}_{inst.Position}_{inst.Rotation}";
		}

        /// <summary>
        ///初始化全部, 资源导出时调用
        /// </summary>
        public void InitAll()
        {
			this.CreateStaticGeometry();
			this.InitStaticGeometry();
			this.LoadParkedVehicles();
			this.CreateEnexes();
			this.LoadWater();
			this.FinalizeLoad();
        }

		/// <summary>
		/// 加载世界的第一步，
		/// 创建静态几何体
		/// </summary>
		public void CreateStaticGeometry ()
		{
			//19个
			Debug.Log("gcj: CellIds.ToArray(): " + CellIds.ToArray().Length);

			var placements = Item.GetPlacements<Instance>(CellIds.ToArray());
            //50769个 placements.Count()
            Debug.Log($"gcj:Cell CreateStaticGeometry: placements Count: {placements.Count()}");
			// find existing objects

			var existingObjects = new Dictionary<string, object>(this.transform.childCount);

			foreach(var sg in this.gameObject.GetFirstLevelChildrenSingleComponent<StaticGeometry>())
            {
				string key = GetKey(sg);

				if (existingObjects.TryGetValue(key, out object obj))
                {
					if (obj is List<StaticGeometry> list)
                    {
						list.Add(sg);
                    }
					else
                    {
						list = new List<StaticGeometry>();
						list.Add(sg);
						existingObjects[key] = list;
                    }
                }
				else
                {
					existingObjects.Add(key, sg);
                }
            }

			int numExistingObjects = existingObjects.Count;

			// create or reuse objects

			m_insts = new Dictionary<Instance, StaticGeometry> (48 * 1024);

			int numObjectsReused = 0;
            var stopwatchCreation = System.Diagnostics.Stopwatch.StartNew();
			double totalCreationTime = 0;

			foreach (var plcm in placements)
            {
				if (this.ignoreLodObjectsWhenInitializing && plcm.IsLod)
					continue;

				string key = existingObjects.Count > 0 ? GetKey(plcm) : null; // this makes the function 2.5x faster
				if (existingObjects.Count > 0 && existingObjects.TryGetValue(key, out object obj))
                {
					StaticGeometry sg;

					if (obj is List<StaticGeometry> list)
                    {
						sg = list.RemoveFirst();
						if (list.Count == 0)
							existingObjects.Remove(key);
                    }
					else
                    {
						sg = (StaticGeometry) obj;
						existingObjects.Remove(key);
                    }

					m_insts.Add(plcm, sg);

					numObjectsReused++;
				}
				else
                {
					stopwatchCreation.Restart();
					m_insts.Add(plcm, StaticGeometry.Create());
					totalCreationTime += stopwatchCreation.Elapsed.TotalSeconds;
				}
            }

			// delete unused existing objects

			int numDeletedObjects = 0;
			foreach (var pair in existingObjects)
            {
				if (pair.Value is List<StaticGeometry> list)
                {
                    list.ForEach(sg => F.DestroyEvenInEditMode(sg.gameObject));
					numDeletedObjects += list.Count;
                }
                else
                {
                    F.DestroyEvenInEditMode(((StaticGeometry)pair.Value).gameObject);
					numDeletedObjects++;
                }
			}

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			_worldSystem = new WorldSystemWithDistanceLevels<MapObject>(
				this.drawDistancesPerLayers,
				this.xzWorldSystemNumAreasPerDrawDistanceLevel.Select(_ => new WorldSystemParams { worldSize = (uint) this.WorldSize, numAreasPerAxis = _ }).ToArray(),
				Enumerable.Range(0, this.drawDistancesPerLayers.Length).Select(_ => new WorldSystemParams { worldSize = this.yWorldSystemSize, numAreasPerAxis = this.yWorldSystemNumAreas }).ToArray(),
				this.OnAreaChangedVisibility);

			this.FocusPointManager = new FocusPointManager<MapObject>(_worldSystem, this.MaxDrawDistance);

            double worldSystemInitTime = stopwatch.Elapsed.TotalSeconds;

			Debug.Log($"Num static geometries {m_insts.Count}, existing {numExistingObjects}, reused {numObjectsReused}, deleted {numDeletedObjects}, creation time {totalCreationTime:F3} s, world system init time {worldSystemInitTime:F3} s");
		}

        /// <summary>
        /// 初始化静态几何体
        /// Cell下有51145个子物体=50769个静态物体+376个传送点
        /// </summary>
        public void InitStaticGeometry ()
		{
            //这里会将静态物体StaticGeometry生成出来
            //这个值是m_insts.Count=50769
            Debug.Log($"gcj:Cell.InitStaticGeometry m_insts.Count: {m_insts.Count}");
			foreach (var inst in m_insts)
			{
				Debug.Log($"gcj: inst.Value.name: {inst.Value.gameObject.name}");
				var staticGeometry = inst.Value;
				//初始化StaticGeometry
				staticGeometry.Initialize(inst.Key, m_insts);
				//初始化完成将其添加到世界系统里
				_worldSystem.AddObjectToArea(
					staticGeometry.transform.position,
					(staticGeometry.ObjectDefinition?.DrawDist ?? 0) * this.drawDistanceMultiplier,
					staticGeometry);
			}
		}

		/// <summary>
		/// 创建停着的汽车
		/// </summary>
		internal void LoadParkedVehicles ()
		{
			if (loadParkedVehicles)
			{
				var parkedVehicles = Item.GetPlacements<ParkedVehicle> (CellIds.ToArray ());
				m_cars = parkedVehicles.Select (x => VehicleSpawnMapObject.Create (x))
					.Cast<MapObject> ()
					.ToArray ();

				UnityEngine.Debug.Log ("Num parked vehicles " + m_cars.Length);
			}
		}

		/// <summary>
		/// 创建传送点物体
		/// </summary>
        internal void CreateEnexes()
        {
			var existingEnexes = this.gameObject
				.GetFirstLevelChildrenSingleComponent<EntranceExitMapObject>()
				.ToQueue();

            m_enexes = new List<EntranceExitMapObject>(256);

            foreach(var enex in Item.Enexes.Where(enex => this.CellIds.Contains(enex.TargetInterior)))
            {
				EntranceExitMapObject enexComponent;
				if (existingEnexes.Count > 0)
				{
					enexComponent = existingEnexes.Dequeue();
					enexComponent.Initialize(enex);
				}
				else
                {
                    enexComponent = EntranceExitMapObject.Create(enex);
                }

                m_enexes.Add(enexComponent);
	            _worldSystem.AddObjectToArea(enexComponent.transform.position, 100f, enexComponent);
            }

			// delete unused enexes
			existingEnexes.ForEach(enex => F.DestroyEvenInEditMode(enex.gameObject));

		}

		/// <summary>
		/// 加载水域
		/// </summary>
        internal void LoadWater ()
		{
			if (F.IsInHeadlessMode)
				return;

			if (Water != null)
			{
				Water.Initialize(Vector2.one * this.WorldSize);
			}
		}

		internal void FinalizeLoad ()
		{
			// set layer recursively for all game objects
			//	this.gameObject.SetLayerRecursive( this.gameObject.layer );

		}

		/// <summary>
		/// 区域更新可见性事件
		/// </summary>
		/// <param name="area"></param>
		/// <param name="visible"></param>
		private void OnAreaChangedVisibility(WorldSystem<MapObject>.Area area, bool visible)
		{
			if (null == area.ObjectsInside || area.ObjectsInside.Count == 0)
				return;

			Profiler.BeginSample("OnAreaChangedVisibility");

			Vector3 areaCenter = area.WorldSystem.GetAreaCenter(area);

			float minSqrDistance = area.FocusPointsThatSeeMe?.Min(f => (areaCenter - f.Position).sqrMagnitude) ?? float.PositiveInfinity;

			_areasToUpdate.Add(new AreaWithDistance
			{
				area = area,
				distance = minSqrDistance,
			});

			Profiler.EndSample();
		}

		public void RegisterFocusPoint(Transform tr, FocusPointParameters parameters)
		{
			this.FocusPointManager.RegisterFocusPoint(tr, parameters);
		}

		public void UnRegisterFocusPoint(Transform tr)
		{
			this.FocusPointManager.UnRegisterFocusPoint(tr);
		}

		private void OnMaxDrawDistanceChanged()
		{
			if (this.FocusPointManager != null)
				this.FocusPointManager.ChangeDefaultRevealRadius(this.MaxDrawDistance);
		}


        public IEnumerable<EntranceExit> GetEnexesFromLoadedInteriors()
        {
            int[] loadedInteriors = this.CellIds.Where(id => !IsExteriorLevel(id)).ToArray();
            foreach(var enex in Importing.Items.Item.Enexes.Where(enex => loadedInteriors.Contains(enex.TargetInterior)))
            {
                yield return enex;
            }
        }

        public TransformDataStruct GetEnexExitTransform(EntranceExit enex)
        {
            return new TransformDataStruct(
	            this.GetPositionBasedOnInteriorLevel(enex.ExitPos + Vector3.up * 0.2f, enex.TargetInterior),
	            Quaternion.Euler(0f, enex.ExitAngle, 0f));
        }

        public TransformDataStruct GetEnexEntranceTransform(EntranceExit enex)
        {
            return new TransformDataStruct(
	            this.GetPositionBasedOnInteriorLevel(enex.EntrancePos + Vector3.up * 0.2f, enex.TargetInterior),
	            Quaternion.Euler(0f, enex.EntranceAngle, 0f));
        }

		/// <summary>
		/// 是否是外部Level,比如商场内部等
		/// </summary>
		/// <param name="interiorLevel"></param>
		/// <returns></returns>
        public static bool IsExteriorLevel(int interiorLevel)
        {
	        return interiorLevel == 0 || interiorLevel == 13;
        }

		/// <summary>
		/// 返回外部Level物体的位置
		/// </summary>
		/// <param name="originalPos"></param>
		/// <param name="interiorLevel"></param>
		/// <returns></returns>
        public Vector3 GetPositionBasedOnInteriorLevel(Vector3 originalPos, int interiorLevel)
        {
	        if (!IsExteriorLevel(interiorLevel))
		        originalPos.y += this.interiorHeightOffset;
	        return originalPos;
        }


        private void Update()
        {

			if (!Loader.HasLoaded)
				return;

			_updateTimeLimitStopwatch.Restart();

			this.FocusPointManager.Update();

            /*if (this._focusPoints.Count > 0)
            {
                // only update divisions loading if there are focus points - because otherwise, 
                // load order of divisions is not updated

            }*/

            UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.Update()");
            _worldSystem.Update();
            UnityEngine.Profiling.Profiler.EndSample();

            Profiler.BeginSample("UpdateAreasLoop");

            while (true)
            {
	            if (_areasToUpdate.Count == 0 && _numElementsInBufferOfAreasToUpdate == 0)
		            break;

	            if (_updateTimeLimitStopwatch.ElapsedMilliseconds >= this.maxTimeToUpdatePerFrameMs)
		            break;

	            /*_areasToUpdate.CopyTo();
	            _areasToUpdate.Clear(); // very good
	            _areasToUpdate.ExceptWith(); // seems good
	            _areasToUpdate.UnionWith(); // catastrophic*/

	            if (_numElementsInBufferOfAreasToUpdate == 0)
	            {
		            Profiler.BeginSample("TakeFromSortedSet");

		            // we processed all areas from buffer
		            // take some more from SortedSet
		            int numToCopy = Mathf.Min(_bufferOfAreasToUpdate.Length, _areasToUpdate.Count);
		            _areasToUpdate.CopyTo(_bufferOfAreasToUpdate, 0, numToCopy);

		            for (int i = 0; i < numToCopy; i++)
		            {
			            if (!_areasToUpdate.Remove(_bufferOfAreasToUpdate[i]))
				            throw new Exception($"Failed to remove area {_bufferOfAreasToUpdate[i].area.Id} from SortedSet");
		            }

		            //_areasToUpdate.ExceptWith(new System.ArraySegment<AreaWithDistance>(_bufferOfAreasToUpdate, 0, numToCopy));

		            _indexOfBufferOfAreasToUpdate = 0;
		            _numElementsInBufferOfAreasToUpdate = numToCopy;

		            Profiler.EndSample();
	            }

	            // process 1 area from buffer

	            var areaWithDistance = _bufferOfAreasToUpdate[_indexOfBufferOfAreasToUpdate];
	            _indexOfBufferOfAreasToUpdate++;
	            _numElementsInBufferOfAreasToUpdate--;
				//更新区域
	            this.UpdateArea(areaWithDistance);

            }

            Profiler.EndSample();

        }

		/// <summary>
		/// 更新区域，显示或隐藏物体
		/// </summary>
		/// <param name="areaWithDistance"></param>
        void UpdateArea(AreaWithDistance areaWithDistance)
        {
	        var area = areaWithDistance.area;
	        bool visible = area.WasVisibleInLastUpdate;

	        for (int i = 0; i < area.ObjectsInside.Count; i++)
	        {
		        var obj = area.ObjectsInside[i];

		        if (visible == obj.IsVisibleInMapSystem)
			        continue;

		        F.RunExceptionSafe(() =>
		        {
					if (visible)
					{
						//显示物体
						obj.Show(areaWithDistance.distance);
					}
					else
					{
						//隐藏物体
						obj.UnShow();
					}
		        });
	        }

        }

		/// <summary>
		/// 导航网格注册物体
		/// </summary>
		/// <param name="mapObject"></param>
		public void RegisterNavMeshObject(MapObject mapObject)
        {
			if (null == _navMeshData) // nav mesh not initialized, generation can not be even unpaused
				return;

			_mapObjectsWithNavMeshToAdd.Add(mapObject);
		}

		/// <summary>
		/// 更新导航网格
		/// </summary>
		void UpdateNavMesh()
        {
			if (!m_generateNavMesh)
				return;

			if (null == _navMeshData)
				return;

			if (_navMeshUpdateAsyncOperation != null && !_navMeshUpdateAsyncOperation.isDone) // still in progress
				return;

			if (_mapObjectsWithNavMeshToAdd.Count == 0) // nothing changed
				return;

            NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByID(0);
			navMeshBuildSettings.minRegionArea = m_navMeshMinRegionArea;
			navMeshBuildSettings.maxJobWorkers = m_navMeshMaxJobWorkers;

			for (int i = 0; i < _mapObjectsWithNavMeshToAdd.Count; i++)
            {
				_mapObjectsWithNavMeshToAdd[i].AddNavMeshBuildSources(_navMeshBuildSources);
			}

			_mapObjectsWithNavMeshToAdd.Clear();

			_navMeshUpdateAsyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
				_navMeshData,
				navMeshBuildSettings,
				_navMeshBuildSources,
				new Bounds(this.transform.position, Vector3.one * this.WorldSize));

			_navMeshUpdateAsyncOperation.priority = m_navMeshAsyncOperationPriority;
		}

		public static List<NavMeshBuildSource> GetNavMeshBuildSources(Transform root, int area)
        {
			var list = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(root, -1, NavMeshCollectGeometry.PhysicsColliders, area, new List<NavMeshBuildMarkup>(), list);
			return list;
        }

		public List<NavMeshBuildSource> GetWaterNavMeshBuildSources()
        {
			if (null == this.Water)
				return new List<NavMeshBuildSource>();

			return GetNavMeshBuildSources(this.Water.transform, NavMeshNotWalkableArea);
		}

		/*public static bool GetNavMeshBuildSourceFromCollider(Collider collider, out NavMeshBuildSource source)
		{
			source = new NavMeshBuildSource();
			source.transform = collider.transform.localToWorldMatrix;

			if (collider is MeshCollider meshCollider)
			{
				source.shape = NavMeshBuildSourceShape.Mesh;
				source.sourceObject = meshCollider.sharedMesh;
			}
			else if (collider is BoxCollider boxCollider)
			{
				source.shape = NavMeshBuildSourceShape.Box;
				source.size = boxCollider.size;
			}
			else if (collider is CapsuleCollider capsuleCollider)
			{
				source.shape = NavMeshBuildSourceShape.Capsule;
				source.size = new Vector3(capsuleCollider.radius, capsuleCollider.height, capsuleCollider.radius);
			}
			else if (collider is SphereCollider sphereCollider)
			{
				source.shape = NavMeshBuildSourceShape.Sphere;
				source.size = Vector3.one * sphereCollider.radius;
			}
			else
			{
				return false;
			}

			return true;
		}*/
	}
}