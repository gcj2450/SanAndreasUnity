using IMGSharp;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// IMG sharp unit test namespace
/// </summary>
namespace IMGSharpUnitTest
{
    /// <summary>
    /// IMG unit test
    /// </summary>
    public class IMGUnitTest:MonoBehaviour
    {
        private void Awake()
        {
            InitArchives();
        }

        /// <summary>
        /// Initialize archives
        /// </summary>
        private static void InitArchives()
        {
            string rootDir= Application.dataPath + "/GTA_SA/IMGSharp/";
            string rootDirimg= Application.dataPath + "/GTA_SA/";
            if (!(File.Exists(rootDirimg + "test1.img")))
            {
                IMGFile.CreateFromDirectory(rootDir + "test", rootDirimg + "test1.img");
            }
            if (!(File.Exists(rootDirimg + "test2.img")))
            {
                //unity 下添加 true 参数报错，输出不了img文件
                IMGFile.CreateFromDirectory(rootDir + "test", rootDirimg + "test2.img", true);
            }
        }

        /// <summary>
        /// Create and read IMG files
        /// </summary>
        public void CreateReadIMGFiles()
        {
            string rootDir = Application.dataPath + "/GTA_SA/IMGSharp/";
            string rootDirimg = Application.dataPath + "/GTA_SA/";
            InitArchives();
            using (IMGArchive archive = IMGFile.Open(rootDirimg + "test1.img", EIMGArchiveMode.Read))
            {
                Debug.Log($"archive test1.img==null: {archive == null}");
                Debug.Log($"archive test1.img.Entries.Length:{archive.Entries.Length}");
            }
            using (IMGArchive archive = IMGFile.Open(rootDirimg + "test2.img", EIMGArchiveMode.Read))
            {
                Debug.Log($"archive test2.img==null: {archive == null}");
                Debug.Log($"archive test2.img.Entries.Length:{archive.Entries.Length}");
            }
        }

        /// <summary>
        /// Commit to IMG file
        /// </summary>
        public void CommitToIMGFile()
        {
            string rootDir = Application.dataPath + "/GTA_SA/IMGSharp/";
            string rootDirimg = Application.dataPath + "/GTA_SA/";
            InitArchives();
            if (File.Exists(rootDirimg + "test3.img"))
            {
                File.Delete(rootDirimg + "test3.img");
            }
            File.Copy(rootDirimg + "test1.img", rootDirimg + "test3.img");
            using (IMGArchive archive = IMGFile.Open(rootDirimg + "test3.img", EIMGArchiveMode.Update))
            {
                Debug.Log($"archive test2.img==null: {archive == null}");
                IMGArchiveEntry[] entries = archive.Entries;
                int entry_count = entries.Length;
                Debug.Log($"entry_count: {entry_count}");
                IMGArchiveEntry entry = entries[0];
                string entry_name = entry.FullName;
                Debug.Log("Unpacking file \"" + entries[0].FullName + "\"");
                if (!(Directory.Exists(rootDir + "test")))
                {
                    Directory.CreateDirectory(rootDir + "test");
                }
                long entry_size = 0;
                using (Stream entry_stream = entry.Open())
                {
                    Debug.Log($"entry_stream==null: {entry_stream == null}");
                    entry_size = entry_stream.Length;
                    Debug.Log("entry_size==entry.Length: "+entry_size +"_"+ (long)(entry.Length));
                    entry_stream.Seek(0L, SeekOrigin.End);
                    for (int i = 0; i < 2048; i++)
                    {
                        entry_stream.WriteByte(0);
                    }
                    entry_size += 2048;
                }
                entries = archive.Entries;
                Debug.Log("entry_count== entries.Length: "+entry_count+"__"+ entries.Length);
                entry = archive.GetEntry(entry_name);
                Debug.Log($"entry==null {entry==null}");
                using (Stream entry_stream = entry.Open())
                {
                    Debug.Log("entry_size==entry_stream.Length: "+entry_size+"__"+ entry_stream.Length);
                    Debug.Log("entry_size==entry.Length: " + entry_size+"__"+ entry.Length);
                    if (entry_size >= 2048)
                    {
                        entry_size -= 2048;
                        entry_stream.SetLength(entry_size);
                    }
                }
            }
        }

        /// <summary>
        /// Extract to test directory
        /// </summary>
        public void ExtractToTestDirectory()
        {
            string rootDir = Application.dataPath + "/GTA_SA/IMGSharp/";
            string rootDirimg = Application.dataPath + "/GTA_SA/";
            int entry_count = 0;
            InitArchives();
            using (IMGArchive archive = IMGFile.Open(rootDirimg + "test1.img", EIMGArchiveMode.Read))
            {
                entry_count = archive.Entries.Length;
            }
            IMGFile.ExtractToDirectory(rootDirimg + "test1.img", rootDir + "test1");
            Debug.Log(entry_count <= Directory.GetFiles(rootDir+"test1", "*", SearchOption.AllDirectories).Length);
            using (IMGArchive archive = IMGFile.Open(rootDirimg + "test2.img", EIMGArchiveMode.Read))
            {
                entry_count = archive.Entries.Length;
            }
            IMGFile.ExtractToDirectory(rootDirimg + "test2.img", rootDir + "test2");
            Debug.Log(entry_count <= Directory.GetFiles(rootDir + "test2", "*", SearchOption.AllDirectories).Length);
        }
    }
}
