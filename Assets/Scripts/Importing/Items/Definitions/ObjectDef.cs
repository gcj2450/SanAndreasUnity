using System;

namespace SanAndreasUnity.Importing.Items.Definitions
{
    /// <summary>
    /// 物体标记
    /// </summary>
    [Flags]
    public enum ObjectFlag : uint
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 道路
        /// </summary>
        IsRoad = 1,
        /// <summary>
        /// 夜晚渲染
        /// </summary>
        RenderAtNight = 2, // ?
        /// <summary>
        /// 最后渲染
        /// </summary>
        DrawLast = 4,
        /// <summary>
        /// 可叠加
        /// </summary>
        Additive = 8,
        /// <summary>
        /// 白天渲染
        /// </summary>
        RenderAtDay = 16, // ?
        /// <summary>
        /// 家具，例如doors
        /// </summary>
        Interior = 32, // ? doors
        /// <summary>
        /// 无ZBufferWrite
        /// </summary>
        NoZBufferWrite = 64,
        /// <summary>
        /// 不接收阴影
        /// </summary>
        DontReceiveShadows = 128,
        /// <summary>
        /// 禁用绘制距离
        /// </summary>
        DisableDrawDist = 256, // ?
        /// <summary>
        /// 可破碎例如玻璃 IS_GLASS_TYPE_1
        /// </summary>
        Breakable = 512, // IS_GLASS_TYPE_1
        /// <summary>
        /// 可破碎块例如玻璃IS_GLASS_TYPE_2
        /// </summary>
        BreakableCrack = 1024, // IS_GLASS_TYPE_2
        /// <summary>
        /// 车库门
        /// </summary>
        GarageDoor = 2048,
        /// <summary>
        /// 可破坏
        /// </summary>
        IsDamagable = 4096,
        /// <summary>
        /// 树
        /// </summary>
        IsTree = 8192,
        /// <summary>
        /// 棕榈树
        /// </summary>
        IsPalm = 16384,
        /// <summary>
        /// 不和飞行的物体碰撞
        /// </summary>
        DoesNotCollideWithFlyer = 32768,
        /// <summary>
        /// 爆炸碰撞
        /// </summary>
        ExplodeHit = 65536,
        /// <summary>
        /// IsTag
        /// </summary>
        IsTag = 1048576,
        /// <summary>
        /// 无BackCull
        /// </summary>
        NoBackCull = 2097152,
        /// <summary>
        /// 可破坏雕塑
        /// </summary>
        IsBreakableStatue = 4194304,
    }

    /// <summary>
    /// 简单物体定义
    /// </summary>
    public interface ISimpleObjectDefinition : IObjectDefinition
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        string ModelName { get; }
        /// <summary>
        /// 纹理字典名称
        /// </summary>
        string TextureDictionaryName { get; }
        /// <summary>
        /// 绘制距离
        /// </summary>
        float DrawDist { get; }
        /// <summary>
        /// 物体标记
        /// </summary>
        ObjectFlag Flags { get; }
    }

    /// <summary>
    /// 物体类定义
    /// </summary>
    [Section("objs")]
    public class ObjectDef : Definition, ISimpleObjectDefinition
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; }
        /// <summary>
        /// 纹理字典名称
        /// </summary>
        public string TextureDictionaryName { get; }
        /// <summary>
        /// 绘制距离
        /// </summary>
        public float DrawDist { get; }
        /// <summary>
        /// 物体标记
        /// </summary>
        public ObjectFlag Flags { get; }

        public ObjectDef(string line) : base(line)
        {
            Id = GetInt(0);
            ModelName = GetString(1);
            TextureDictionaryName = GetString(2);
            DrawDist = GetSingle(3);
            Flags = (ObjectFlag)GetInt(4);
        }

        public bool HasFlag(ObjectFlag flag)
        {
            return (Flags & flag) == flag;
        }
    }
}