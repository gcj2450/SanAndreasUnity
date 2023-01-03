namespace SanAndreasUnity.Importing.Items.Definitions
{
    /// <summary>
    /// 计时出现的物体
    /// </summary>
    [Section("tobj")]
    public class TimeObjectDef : Definition, ISimpleObjectDefinition
    {
        public int Id { get; }
        public string ModelName { get; }
        public string TextureDictionaryName { get; }
        public int MeshCount { get; }
        public float DrawDist { get; }
        public ObjectFlag Flags { get; }
        /// <summary>
        /// 显示的小时数
        /// </summary>
        public int TimeOnHours { get; }
        /// <summary>
        /// 隐藏的小时数
        /// </summary>
        public int TimeOffHours { get; }

        public TimeObjectDef(string line)
            : base(line)
        {
            int index = 0;

            Id = GetInt(index++);
            ModelName = GetString(index++);
            TextureDictionaryName = GetString(index++);
            if (Parts == 7)
                MeshCount = 1;
            else
                MeshCount = GetInt(index++);
            DrawDist = GetSingle(index++);
            for (int i = 0; i < MeshCount - 1; i++)
                GetSingle(index++);
            Flags = (ObjectFlag)GetInt(index++);
            TimeOnHours = GetInt(index++);
            TimeOffHours = GetInt(index++);
        }
    }
}
