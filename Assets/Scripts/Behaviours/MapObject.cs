using System.Collections.Generic;
using System.Diagnostics;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Profiler = UnityEngine.Profiling.Profiler;

namespace SanAndreasUnity.Behaviours
{
    /// <summary>
    /// 场景物体基类
    /// </summary>
    public abstract class MapObject : MonoBehaviour
    {
        private static Texture2D _sNoiseTex;

        /// <summary>
        /// 是否需要生成噪点纹理
        /// </summary>
        private static bool ShouldGenerateNoiseTex => _sNoiseTex == null || _sNoiseTex.width != Screen.width || _sNoiseTex.height != Screen.height;

        private static int _sBreakableLayer = -1;
        /// <summary>
        /// 可破碎层
        /// </summary>
        public static int BreakableLayer => _sBreakableLayer == -1 ? _sBreakableLayer = LayerMask.NameToLayer("Breakable") : _sBreakableLayer;

        private static int _sNoiseTexPropertyId = -1;
        /// <summary>
        /// 噪点纹理的属性ID
        /// </summary>
        protected static int NoiseTexPropertyId => _sNoiseTexPropertyId == -1 ? _sNoiseTexPropertyId = Shader.PropertyToID("_NoiseTex") : _sNoiseTexPropertyId;

        private static int _sFadePropertyId = -1;
        /// <summary>
        /// 透明渐变的属性ID
        /// </summary>
        protected static int FadePropertyId => _sFadePropertyId == -1 ? _sFadePropertyId = Shader.PropertyToID("_Fade") : _sFadePropertyId;

        /// <summary>
        /// 生成一个Noise纹理
        /// </summary>
        private static void GenerateNoiseTex()
        {
            var width = Screen.width;
            var height = Screen.height;

            var timer = new Stopwatch();
            timer.Start();

            if (_sNoiseTex == null)
            {
                _sNoiseTex = new Texture2D(width, height, TextureFormat.Alpha8, false);
                _sNoiseTex.filterMode = FilterMode.Bilinear;
            }
            else
            {
                _sNoiseTex.Reinitialize(width, height);
            }

            var rand = new System.Random(0x54e03b19);
            var buffer = new byte[width * height];
            rand.NextBytes(buffer);

            _sNoiseTex.LoadRawTextureData(buffer);
            _sNoiseTex.Apply(false, false);

            UnityEngine.Debug.LogFormat("Noise gen time: {0:F2} ms", timer.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// 用于透明渐变的噪点图
        /// </summary>
        protected static Texture2D NoiseTex
        {
            get
            {
                if (ShouldGenerateNoiseTex) GenerateNoiseTex();
                return _sNoiseTex;
            }
        }

        /// <summary>
        /// 在地图系统中是否可见
        /// </summary>
        public bool IsVisibleInMapSystem { get; private set; } = false;

        /// <summary>
        /// 加载优先级
        /// </summary>
        public float LoadPriority { get; private set; } = float.PositiveInfinity;

        private bool _loaded;
        /// <summary>
        /// 是否已经加载
        /// </summary>
        public bool HasLoaded => _loaded;

        /// <summary>
        /// 缓存的位置
        /// </summary>
        public Vector3 CachedPosition { get; private set; }

        /// <summary>
        ///Instantiate生成一个Cell的子物体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected static T Create<T>(GameObject prefab)
            where T : MapObject
        {
            GameObject go = Object.Instantiate(prefab, Cell.Instance.transform);
            return go.GetComponent<T>();
        }

        /// <summary>
        /// 初始化，设置位置和旋转
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        protected void Initialize(Vector3 pos, Quaternion rot)
        {
            this.transform.position = pos;
            this.transform.localRotation = rot;

            this.CachedPosition = pos;

            _loaded = false;
        }

        public void SetDrawDistance(float f)
        {

        }

        /// <summary>
        /// 显示物体
        /// </summary>
        /// <param name="loadPriority"></param>
        public void Show(float loadPriority)
        {
            if (this.IsVisibleInMapSystem)
                return;

            this.IsVisibleInMapSystem = true;

            this.LoadPriority = loadPriority;

            if (!_loaded)
            {
				_loaded = true;
                
				Profiler.BeginSample ("OnLoad", this);
				this.OnLoad();
				Profiler.EndSample ();
            }

			Profiler.BeginSample ("OnShow", this);
            this.OnShow();
			Profiler.EndSample ();
        }

       /// <summary>
       /// 隐藏物体
       /// </summary>
        public void UnShow()
        {
            if (!this.IsVisibleInMapSystem)
                return;

            this.IsVisibleInMapSystem = false;

            this.OnUnShow();
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// 显示事件，物体SetActive true
        /// </summary>
        protected virtual void OnShow()
        {
            UnityEngine.Debug.Log("gcj: MapObject.OnShow");
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏事件，物体SetActive false
        /// </summary>
        protected virtual void OnUnShow()
        {
            this.gameObject.SetActive(false);
        }

        public virtual void AddNavMeshBuildSources(List<NavMeshBuildSource> list)
        {
        }
    }
}