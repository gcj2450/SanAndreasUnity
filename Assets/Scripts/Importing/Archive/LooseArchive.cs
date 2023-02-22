using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Importing.Archive
{
    /// <summary>
    /// 松散的档案文件包括12种格式的文件
    /// </summary>
    public class LooseArchive : IArchive
    {
        /// <summary>
        /// 松散的档案条目
        /// </summary>
        private struct LooseArchiveEntry
        {
            /// <summary>
            /// 文件路径
            /// </summary>
            public readonly string FilePath;
            /// <summary>
            /// 文件名带后缀
            /// </summary>
            public readonly string Name;

            public LooseArchiveEntry(string filePath)
            {
                FilePath = filePath;
                Name = Path.GetFileName(filePath);
            }
        }

        /// <summary>
        /// 12个指定的文件类型
        /// </summary>
        private static readonly HashSet<string> _sValidExtensions
            = new HashSet<string> {
                ".txd",
                ".gxt",
                ".col",
                ".dff",
                ".fxp",
                ".ifp",
                ".ide",
                ".ipl",
                ".zon",
                ".img",
                ".dat",
                ".cfg",
            };

        /// <summary>
        /// 所有的游戏档案文件，共329个文件
        /// </summary>
        private readonly Dictionary<String, LooseArchiveEntry> _fileDict;
        /// <summary>
        /// 12个指定后缀名的文件列表
        /// </summary>
        private readonly Dictionary<String, List<String>> _extDict;

        /// <summary>
        /// 文件数
        /// </summary>
        public int NumLoadedEntries => _fileDict.Count;

        /// <summary>
        /// 加载所有的松散的档案文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static LooseArchive Load(string dirPath)
        {
            return new LooseArchive(dirPath);
        }

        /// <summary>
        /// 加载游戏目录下的所有_sValidExtensions格式列表下的文件
        /// </summary>
        /// <param name="dirPath"></param>
        private LooseArchive(string dirPath)
        {
            Debug.Log("Loading loose archive: " + dirPath);

            //不区分大小写的字典，key如果是：Candy和candy会被认为是同一个
            _fileDict = new Dictionary<string, LooseArchiveEntry>(StringComparer.InvariantCultureIgnoreCase);

            //每种后缀名下的文件列表
            _extDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            
            Debug.Log("AllFilesCount:" + Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories).Length);
            //游戏文件夹下所有文件，过滤出带有_sValidExtensions其中一个后缀的所有文件
            foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file);

                ext = ext.ToLower();

                if (!_sValidExtensions.Contains(ext)) continue;

                var entry = new LooseArchiveEntry(file);

                if (_fileDict.ContainsKey(entry.Name))
                {
                    Debug.LogWarningFormat("Already loaded {0}", entry.Name);
                    continue;
                }

                Debug.Log ("Adding loose archive entry: " + entry.Name);

                _fileDict.Add(entry.Name, entry);

                if (ext == null) continue;

                if (!_extDict.ContainsKey(ext))
                {
                    _extDict.Add(ext, new List<string>());
                }

                _extDict[ext].Add(entry.Name);
            }
            //329个文件
            Debug.Log($"gcj: _fileDict count: {_fileDict.Count},_extDict count: {_extDict.Count}");
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllFiles()
        {
            return _fileDict.Keys;
        }

        /// <summary>
        /// 获取指定后缀下的文件列表
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFileNamesWithExtension(string ext)
        {
            return _extDict.ContainsKey(ext) ? _extDict[ext] : Enumerable.Empty<string>();
        }

        /// <summary>
        /// 是否包含指定名称的文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsFile(string name)
        {
            return _fileDict.ContainsKey(name);
        }

        /// <summary>
        /// 读取指定文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public System.IO.Stream ReadFile(string name)
        {
            return File.OpenRead(_fileDict[name].FilePath);
        }

        /// <summary>
        /// 获取指定文件名的文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool GetFilePath(string fileName, ref string filePath)
        {
            if (_fileDict.TryGetValue(fileName, out LooseArchiveEntry entry))
            {
                filePath = entry.FilePath;
                return true;
            }
            return false;
        }

    }
}