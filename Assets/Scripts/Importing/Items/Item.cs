using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Importing.RenderWareStream;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace SanAndreasUnity.Importing.Items
{
    public static class Item
    {
        private static readonly List<Placements.Zone> _zones = new List<Placements.Zone>();

        private static readonly List<EntranceExit> _enexes = new List<EntranceExit>();
        public static IReadOnlyList<EntranceExit> Enexes => _enexes;

        private static readonly Dictionary<int, IObjectDefinition> _definitions
            = new Dictionary<int, IObjectDefinition>();

        private static readonly Dictionary<int, List<Placement>> _placements
            = new Dictionary<int, List<Placement>>();

        public static void ReadLoadList(string path)
        {
            var ws = new[] { ' ', '\t' };

            using (var reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Length == 0) continue;
                    if (line.StartsWith("#")) continue;

                    var index = line.IndexOfAny(ws);
                    if (index == -1) continue;

                    var type = line.Substring(0, index);
                    var args = line.Substring(index).TrimStart();

                    args = args.Replace("DATA\\MAPS\\", "data/maps/");
                    args = args.Replace(".IDE", ".ide");
                    args = args.Replace(".IPL", ".ipl");
                    args = args.Replace('\\', Path.DirectorySeparatorChar);

                    switch (type.ToLower())
                    {
                        case "ide":
                            ReadIde(args);
                            break;

                        case "ipl":
                            //country.ipl数据：18395, cs_lodroads04, 0, -2930.242188, -1548.007813, 10.25, 0, 0, -0.7064119577, 0.7078009248, -1
                            Debug.Log($"gcj: Item.ReadLoadList ReadIpl: {args}");
                            ReadIpl(args);
                            break;
                    }
                }
            }
        }

        public static void ReadIde(string path)
        {
            var file = new ItemFile<Definition>(path);
            foreach (var obj in file.GetItems<Definition>().OfType<IObjectDefinition>())
            {
                if (_definitions.ContainsKey(obj.Id))
                {
                    Debug.LogWarning($"Definition with id {obj.Id} already exists, skipping it");
                }
                else
                {
                    _definitions.Add(obj.Id, obj);
                }
            }
        }

        public static void ReadIpl(string path)
        {
            //读取data/maps/下所有的.ipl文件
            Debug.Log($"gcj: Readlpl {path}");
            
            //path的值示例：data/maps/country\countN2.ipl
            //Path.GetFileName(path)：countN2.ipl
            //ArchiveManager.GetCaseSensitiveFilePath(Path.GetFileName(path))：D:\Program Files (x86)\10150500\data\maps\country\countn2.ipl
            
            //Debug.Log($"Path.GetFileName(path):{Path.GetFileName(path)},  ArchiveManager.GetCaseSensitiveFilePath: {ArchiveManager.GetCaseSensitiveFilePath(Path.GetFileName(path))}");

            var file = new ItemFile<Placement>(ArchiveManager.GetCaseSensitiveFilePath(Path.GetFileName(path)));

            //DATA/MAP.ZON
            //DATA/INFO.ZON
            foreach (var zone in file.GetSection<Placements.Zone>("zone"))
            {
                Debug.Log($"gcj: Readipl: _zones.Add zone: {zone.Name}");
                _zones.Add(zone);
            }

            //商场等传送点
            foreach (var enex in file.GetSection<EntranceExit>("enex"))
            {
                Debug.Log($"gcj: Readipl: _enexes.Add enex: {enex.Name}");
                _enexes.Add(enex);
            }

            var insts = file.GetSection<Instance>("inst");

            var list = new List<Instance>();
            list.AddRange(insts);

            var cars = new List<ParkedVehicle>(file.GetSection<ParkedVehicle>("cars"));

            string streamFormat = Path.GetFileNameWithoutExtension(path).ToLower() + "_stream{0}.ipl";
            int missed = 0;
            for (int i = 0; ; ++i)
            {
                string streamFileName = string.Format(streamFormat, i);
                if (!ArchiveManager.FileExists(streamFileName))
                {
                    ++missed;

                    if (missed > 10) break;
                    continue;
                }

                file = new ItemFile<Placement>(ArchiveManager.ReadFile(streamFileName));
                list.AddRange(file.GetSection<Instance>("inst"));
                cars.AddRange(file.GetSection<ParkedVehicle>("cars"));
            }

            list.ResolveLod();
            Debug.Log($"gcj:Item ReadIpl Instance list: " + list.Count);
            int lastCell = -1;
            foreach (var inst in list)
            {
                int cell = inst.InteriorLevel;
                if (lastCell != cell && !_placements.ContainsKey(lastCell = cell))
                {
                    _placements.Add(cell, new List<Placement>());
                }

                _placements[cell].Add(inst);
            }

            if (!_placements.ContainsKey(0))
            {
                _placements.Add(0, new List<Placement>());
            }

            _placements[0].AddRange(cars.Cast<Placement>());

        }

        public static TDefinition GetDefinition<TDefinition>(int id)
            where TDefinition : class, IObjectDefinition
        {
            return _definitions.TryGetValue(id, out IObjectDefinition objectDefinition)
                ? (TDefinition) objectDefinition
                : null;
        }

        public static TDefinition GetDefinitionOrThrow<TDefinition>(int id)
            where TDefinition : Definition, IObjectDefinition
        {
            var def = GetDefinition<TDefinition>(id);
            if (null == def)
                throw new System.Exception($"Failed to find definition of type {typeof(TDefinition).Name} with id {id}");
            return def;
        }

        public static IEnumerable<TDefinition> GetDefinitions<TDefinition>()
            where TDefinition : Definition
        {
            return _definitions.Values.OfType<TDefinition>();
        }

		public static int GetNumDefinitions<TDefinition>()
			where TDefinition : Definition
		{
			return _definitions.Count (pair => pair.Value is TDefinition);
		}

        public static IEnumerable<TPlacement> GetPlacements<TPlacement>(params int[] cellIds)
            where TPlacement : Placement
        {
            return cellIds.SelectMany(x => _placements.ContainsKey(x)
                ? _placements[x].OfType<TPlacement>() : Enumerable.Empty<TPlacement>());
        }
    }
}