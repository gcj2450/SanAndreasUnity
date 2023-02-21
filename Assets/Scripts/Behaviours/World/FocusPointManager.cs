using System;
using System.Collections.Generic;
using SanAndreasUnity.Behaviours.WorldSystem;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{

    /// <summary>
    /// 注视点参数
    /// </summary>
    [Serializable]
    public struct FocusPointParameters
    {
        /// <summary>
        /// 是否有暴露范围
        /// </summary>
        public bool hasRevealRadius;
        /// <summary>
        /// 暴露范围
        /// </summary>
        public float revealRadius;
        /// <summary>
        /// 移除后保持暴露时间
        /// </summary>
        public float timeToKeepRevealingAfterRemoved;

        public FocusPointParameters(bool hasRevealRadius, float revealRadius, float timeToKeepRevealingAfterRemoved)
        {
            this.hasRevealRadius = hasRevealRadius;
            this.revealRadius = revealRadius;
            this.timeToKeepRevealingAfterRemoved = timeToKeepRevealingAfterRemoved;
        }

        /// <summary>
        /// 默认注视点参数
        /// </summary>
        public static FocusPointParameters Default => new FocusPointParameters(true, 150f, 3f);
    }

    /// <summary>
    /// Handles registration, unregistration, and updating of focus points for a world system.
    /// </summary>
    public class FocusPointManager<T>
    {
        private IWorldSystem<T> _worldSystem;

        private float _defaultRevealRadius;

        /// <summary>
        /// 注视点信息
        /// </summary>
        public struct FocusPointInfo
        {
            /// <summary>
            /// 注视点
            /// </summary>
            public WorldSystem.FocusPoint focusPoint;
            /// <summary>
            /// Transform
            /// </summary>
            public Transform transform;
            /// <summary>
            /// 移除后保持暴露的时间长度
            /// </summary>
            public float timeToKeepRevealingAfterRemoved;
            /// <summary>
            /// 移除后的时间
            /// </summary>
            public double timeWhenRemoved;
            /// <summary>
            /// 是否具有暴露范围
            /// </summary>
            public bool hasRevealRadius;
        }

        private List<FocusPointInfo> _focusPoints = new List<FocusPointInfo>();
        /// <summary>
        /// 注视点信息列表
        /// </summary>
        public IReadOnlyList<FocusPointInfo> FocusPoints => _focusPoints;

        private List<FocusPointInfo> _focusPointsToRemoveAfterTimeout = new List<FocusPointInfo>();


        public FocusPointManager(
            IWorldSystem<T> worldSystem,
            float defaultRevealRadius)
        {
            _worldSystem = worldSystem;
            _defaultRevealRadius = defaultRevealRadius;
        }

        /// <summary>
        /// 注册关注点
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="parameters"></param>
        public void RegisterFocusPoint(Transform tr, FocusPointParameters parameters)
        {
            if (!_focusPoints.Exists(f => f.transform == tr))
            {
                float revealRadius = parameters.hasRevealRadius ? parameters.revealRadius : _defaultRevealRadius;
                var registeredFocusPoint = _worldSystem.RegisterFocusPoint(revealRadius, tr.position);
                _focusPoints.Add(new FocusPointInfo
                {
                    focusPoint = registeredFocusPoint,
                    transform = tr,
                    timeToKeepRevealingAfterRemoved = parameters.timeToKeepRevealingAfterRemoved,
                    hasRevealRadius = parameters.hasRevealRadius,
                });
            }
        }

        /// <summary>
        /// 取消注册关注点
        /// </summary>
        /// <param name="tr"></param>
        public void UnRegisterFocusPoint(Transform tr)
        {
            int index = _focusPoints.FindIndex(f => f.transform == tr);
            if (index < 0)
                return;

            // maybe we could just set transform to null, so it gets removed during next update ?

            var focusPoint = _focusPoints[index];

            if (focusPoint.timeToKeepRevealingAfterRemoved > 0)
            {
                focusPoint.timeWhenRemoved = Time.timeAsDouble;
                _focusPointsToRemoveAfterTimeout.Add(focusPoint);
                _focusPoints.RemoveAt(index);
                return;
            }

            _worldSystem.UnRegisterFocusPoint(focusPoint.focusPoint);
            _focusPoints.RemoveAt(index);
        }

        public void Update()
        {
            double timeNow = Time.timeAsDouble;

            UnityEngine.Profiling.Profiler.BeginSample("Update focus points");
            this._focusPoints.RemoveAll(f =>
            {
                if (null == f.transform)
                {
            	    if (f.timeToKeepRevealingAfterRemoved > 0f)
            	    {
            		    f.timeWhenRemoved = timeNow;
            		    _focusPointsToRemoveAfterTimeout.Add(f);
            		    return true;
            	    }

            	    UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.UnRegisterFocusPoint()");
            	    _worldSystem.UnRegisterFocusPoint(f.focusPoint);
            	    UnityEngine.Profiling.Profiler.EndSample();
            	    return true;
                }

                _worldSystem.FocusPointChangedPosition(f.focusPoint, f.transform.position);

                return false;
            });
            UnityEngine.Profiling.Profiler.EndSample();

            bool hasElementToRemove = false;
            _focusPointsToRemoveAfterTimeout.ForEach(_ =>
            {
                if (timeNow - _.timeWhenRemoved > _.timeToKeepRevealingAfterRemoved)
                {
            	    hasElementToRemove = true;
            	    UnityEngine.Profiling.Profiler.BeginSample("WorldSystem.UnRegisterFocusPoint()");
            	    _worldSystem.UnRegisterFocusPoint(_.focusPoint);
            	    UnityEngine.Profiling.Profiler.EndSample();
                }
            });

            if (hasElementToRemove)
                _focusPointsToRemoveAfterTimeout.RemoveAll(_ => timeNow - _.timeWhenRemoved > _.timeToKeepRevealingAfterRemoved);

        }

        /// <summary>
        /// 修改默认暴露半径
        /// </summary>
        /// <param name="newDefaultRevealRadius"></param>
        public void ChangeDefaultRevealRadius(float newDefaultRevealRadius)
        {
            _defaultRevealRadius = newDefaultRevealRadius;

            for (int i = 0; i < _focusPoints.Count; i++)
            {
                var focusPoint = _focusPoints[i];
                if (!focusPoint.hasRevealRadius)
                {
                    _worldSystem.FocusPointChangedRadius(focusPoint.focusPoint, newDefaultRevealRadius);
                }
            }
        }
    }
}
