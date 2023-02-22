using SanAndreasUnity.Importing.RenderWareStream;
using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace SanAndreasUnity.Importing.Archive
{
    /// <summary>
    /// 文件档案接口，获取文件列表，读取文件
    /// </summary>
    public interface IArchive
    {
        /// <summary>
        /// 获取所有文件列表
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllFiles();

        /// <summary>
        /// 获取指定后缀的文件列表
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        IEnumerable<string> GetFileNamesWithExtension(string ext);

        /// <summary>
        /// 是否包含指定文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ContainsFile(string name);

        /// <summary>
        /// 读取指定文件，返回文件流
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Stream ReadFile(string name);

        /// <summary>
        /// 加载到的文件数量
        /// </summary>
        int NumLoadedEntries { get; }
    }

	/// <summary>
	/// 处理档案加载和读取管理器，用于不要手动读取文件，只用该类读取，因为它是线程安全的.
	/// </summary>
    public static class ArchiveManager
    {
        /// <summary>
        /// 模型文件夹
        /// </summary>
        public static string ModelsDir { get { return Path.Combine(Config.GamePath, "models"); } }
        /// <summary>
        /// 数据文件夹
        /// </summary>
        public static string DataDir { get { return Path.Combine(Config.GamePath, "data"); } }

        public static string GetPath(params string[] relative)
        {
            return relative.Aggregate(Config.GamePath, Path.Combine).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 根据文件名获取已经加载的完整文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetCaseSensitiveFilePath(string fileName)
        {
            string filePath = null;
            foreach(var archive in _sLoadedArchives.OfType<LooseArchive>())
            {
                if (archive.GetFilePath(fileName, ref filePath))
                    return filePath;
            }
            throw new FileNotFoundException(fileName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string PathToCaseSensitivePath(string path)
        {
            return ArchiveManager.GetCaseSensitiveFilePath(Path.GetFileName(path));
        }

        /// <summary>
        /// 已经加载的档案文件
        /// </summary>
        private static readonly List<IArchive> _sLoadedArchives = new List<IArchive>();

        /// <summary>
        /// 获取已加载的文件档案数量
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetNumArchives()
        {
            return _sLoadedArchives.Count;
        }

        /// <summary>
        /// 所有已加载的入口
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetTotalNumLoadedEntries()
        {
            return _sLoadedArchives.Sum(a => a.NumLoadedEntries);
        }

        /// <summary>
        /// 所有文件
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static List<string> GetAllEntries()
        {
            return _sLoadedArchives
                .SelectMany(a => a.GetAllFiles())
                .ToList();
        }

        /// <summary>
        /// 加载游戏目录下的所有档案文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
        public static LooseArchive LoadLooseArchive(string dirPath)
        {
            LooseArchive arch = LooseArchive.Load(dirPath);
            _sLoadedArchives.Add(arch);
            Debug.Log("_sLoadedArchives.Add: " +_sLoadedArchives.Count);
            return arch;
        }

        /// <summary>
        /// 加载img文件档案
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
        public static ImageArchive LoadImageArchive(string filePath)
        {
            var arch = ImageArchive.Load(filePath);
            _sLoadedArchives.Add(arch);
            Debug.Log("_sLoadedArchives.Add: " + _sLoadedArchives.Count);

            return arch;
        }

        /// <summary>
        /// 是否包含指定文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
        public static bool FileExists(string name)
        {
            return _sLoadedArchives.Any(x => x.ContainsFile(name));
        }

        /// <summary>
        /// 获取指定后缀名的文件
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="fileNames"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void GetFileNamesWithExtension(string ext, List<string> fileNames)
        {
            foreach (var archive in _sLoadedArchives)
                fileNames.AddRange(archive.GetFileNamesWithExtension(ext));
        }
        /// <summary>
        /// 获取指定后缀名的文件列表
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static List<string> GetFileNamesWithExtension(string ext)
        {
            var list = new List<string>();
            GetFileNamesWithExtension(ext, list);
            return list;
        }

        /// <summary>
        /// 获取指定后缀的文件列表
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static List<string> GetFilePathsFromLooseArchivesWithExtension(string ext)
        {
            var list = new List<string>();

            foreach (var archive in _sLoadedArchives.OfType<LooseArchive>())
            {
                foreach (string fileName in archive.GetFileNamesWithExtension(ext))
                {
                    string filePath = null;
                    archive.GetFilePath(fileName, ref filePath);
                    list.Add(filePath);
                }
            }

            return list;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
		[MethodImpl(MethodImplOptions.Synchronized)]
        public static Stream ReadFile(string name)
        {
            var arch = _sLoadedArchives.FirstOrDefault(x => x.ContainsFile(name));
            Debug.Log($"gcj: ArchiveManager ReadFile: {name} arch == null:  {arch == null}");
            if (arch == null) throw new FileNotFoundException(name);

			// get a stream and build memory stream out of it - this will ensure thread safe access

			var stream = arch.ReadFile(name);

			byte[] buffer = new byte[stream.Length];
			stream.Read (buffer, 0, (int) stream.Length);

			stream.Dispose ();

			return new MemoryStream (buffer);
        }

		// this method should not be synchronized, because thread would block while 
		// archive is being read, but the thread only wants to register a job and continue
	//	[MethodImpl(MethodImplOptions.Synchronized)]
		public static void ReadFileAsync(string name, float loadPriority, System.Action<Stream> onFinish)
		{
			LoadingThread.RegisterJob (new BackgroundJobRunner.Job<Stream> () {
                priority = loadPriority,
				action = () => ReadFile( name ),
				callbackFinish = (stream) => { onFinish(stream); },
			});
		}

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <typeparam name="TSection"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
		[MethodImpl(MethodImplOptions.Synchronized)]	// ensure section is read, before another thread can read archives
        public static TSection ReadFile<TSection>(string name)
            where TSection : SectionData
        {
            using (var stream = ReadFile(name))
            {
                var section = Section<SectionData>.ReadData(stream) as TSection;
                if (section == null)
                {
                    throw new ArgumentException(string.Format("File \"{0}\" is not a {1}!", name, typeof(TSection).Name), "name");
                }

                return section;
            }
        }
    }
}