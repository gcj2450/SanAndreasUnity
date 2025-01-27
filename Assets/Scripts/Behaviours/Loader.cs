﻿using System.IO;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Collision;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Vehicles;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.GXT;
using System.Net.Security;

/*
cfg：配置文件，存放用户设置、配置数据，游戏里用于存放各种参数，例如载具速度等；

ide：功能同cfg，存储数据参数，例如武器伤害值等；

dat：一般系统会把它识别成媒体文件，这是因为它大多是MPG格式转换成的。
但SA的dat文件也只是一种广义的配置文件，同cfg和ide，存储数据或记录；

ifp：专属于R星公司开发游戏的文件，存储动作（动画）信息，例如人物走路姿势；

img：对OS熟悉的应该知道，这是一种镜像文件，可以理解成把光盘软盘上的内容压缩归档，变成了这种数字格式。
SA的img文件有压缩模型文件的，也有压缩动画文件的，可以理解成一些同类文件的压缩文件；（不要忘记SA最早是PS2平台的，有这种奇怪格式的东西并不奇怪）

ico/icn：顾名思义图标文件；
ipl文件：全称Item Placement File，专属于R星开发游戏的格式，存储各种位置信息，
例如加特林的刷新地点，一般轮不着我们直接修改，一些地图MOD才会用到；

fxp：这个我没怎么看懂，不过它是用来存储一些特效的，例如开枪的火光和爆炸等；

gxt：字体文件；

dff：一般系统会把它识别成音乐，但SA的dff是3D模型文件，像obj, ply等文件一样存储3D模型信息；

txd：与dff文件配套的贴图文件
 */

namespace SanAndreasUnity.Behaviours
{

    public class Loader : StartupSingleton<Loader>
    {

        public static bool HasLoaded { get; private set; }
        public static bool IsLoading { get; private set; }

        public static string LoadingStatus { get; private set; }
        public static string LastLoadingStatusWhenErrorHappened { get; private set; }

        private static CoroutineInfo s_coroutine;

        private static int m_currentStepIndex = 0;

        private static float m_totalEstimatedLoadingTime = 0;

        private static bool m_hasErrors = false;
        private static System.Exception m_loadException;

        public class LoadingStep
        {
            public IEnumerator Coroutine { get; private set; }
            public System.Action LoadFunction { get; private set; }
            public string Description { get; set; }
            public float TimeElapsed { get; internal set; }
            public float EstimatedTime { get; private set; }
            public bool CompletedSuccessfully { get; set; } = false;

            public LoadingStep(System.Action loadFunction, string description, float estimatedTime = 0f)
            {
                this.LoadFunction = loadFunction;
                this.Description = description;
                this.EstimatedTime = estimatedTime;
            }

            public LoadingStep(IEnumerator coroutine, string description, float estimatedTime = 0f)
            {
                this.Coroutine = coroutine;
                this.Description = description;
                this.EstimatedTime = estimatedTime;
            }

        }

        private static List<LoadingStep> m_loadingSteps = new List<LoadingStep>();

        public static Texture2D CurrentSplashTex { get; set; }
        public static Texture2D SplashTex1 { get; set; }
        public static Texture2D SplashTex2 { get; set; }

        private static bool m_showFileBrowser = false;
        private static FileBrowser m_fileBrowser = null;

        public static event System.Action onLoadSpecialTextures = delegate { };

        // also called for failures
        public static event System.Action onLoadingFinished = delegate { };



        protected override void OnSingletonStart()
        {

        }

        /// <summary>
        /// 添加加载步骤
        /// </summary>
        private static void AddLoadingSteps()
        {

            LoadingStep[] steps = new LoadingStep[] {
				//是否加载图片
				new LoadingStep ( StepConfigure, "Configuring", 0f ),
				//读取GTA游戏路径,写死路径了，注释掉：D:\Program Files (x86)\10150500
				//new LoadingStep ( StepSelectGTAPath(), "Select path to GTA", 0.0f ),
				//读取档案文件路径
				new LoadingStep ( StepLoadArchives, "Loading archives", 1.7f ),
				//加载启动画面
				new LoadingStep ( StepLoadSplashScreen, "Loading splash screen", 0.06f ),
				//设置第一个启动画面
				new LoadingStep ( StepSetSplash1, "Set splash 1" ),
				//加载声音
				new LoadingStep ( StepLoadAudio, "Loading audio" ),
				//加载字体文件，已经被官方注释掉了
				//new LoadingStep ( StepLoadFonts,"Loading fonts"),
				//加载碰撞器文件
				new LoadingStep ( StepLoadCollision, "Loading collision files", 0.9f ),
                new LoadingStep ( StepLoadItemInfo, "Loading item info", 2.4f ),
                new LoadingStep ( StepLoadHandling, "Loading handling", 0.01f ),
				//已经注释掉了，已经被官方注释掉了
				//new LoadingStep ( () => { throw new System.Exception ("testing error handling"); }, "testing error handling", 0.01f ),
				new LoadingStep ( StepLoadAnimGroups, "Loading animation groups", 0.02f ),
                new LoadingStep ( StepLoadCarColors, "Loading car colors", 0.04f ),
                new LoadingStep ( StepLoadWeaponsData, "Loading weapons data", 0.05f ),
				//设置第二个启动画面
				new LoadingStep ( StepSetSplash2, "Set splash 2" ),
                //加载小地图
                new LoadingStep ( StepLoadMap, "Loading map", 2.1f ),
                //加载鼠标图标
                new LoadingStep ( StepLoadSpecialTextures, "Loading special textures", 0.01f ),
				//已经被注释掉了
				//new LoadingStep ( StepLoadGXT, "Loading GXT", 0.15f),
                new LoadingStep ( StepLoadPaths, "Loading paths"),
            };


            for (int i = 0; i < steps.Length; i++)
            {
                AddLoadingStep(steps[i]);
            }

            //加载世界模型
            var worldSteps = new LoadingStep[]
            {
                new LoadingStep( () => Cell.Instance.CreateStaticGeometry (), "Creating static geometry", 5.8f ),
                new LoadingStep( () => Cell.Instance.InitStaticGeometry (), "Init static geometry", 0.35f ),
                new LoadingStep( () => Cell.Instance.LoadParkedVehicles (), "Loading parked vehicles", 0.2f ),
                new LoadingStep( () => Cell.Instance.CreateEnexes (), "Creating enexes", 0.1f ),
                new LoadingStep( () => Cell.Instance.LoadWater (), "Loading water", 0.08f ),
                new LoadingStep( () => Cell.Instance.FinalizeLoad (), "Finalize world loading", 0.01f ),
            };

            if (Cell.Instance != null)
                worldSteps.ForEach(AddLoadingStep);
            else
                worldSteps.ForEach(_ => RemoveLoadingStep(_.Description));


        }

        /// <summary>
        /// 添加加载步骤
        /// </summary>
        /// <param name="step"></param>
        private static void AddLoadingStep(LoadingStep step)
        {
            if (m_loadingSteps.Exists(_ => _.Description == step.Description))
                return;

            m_loadingSteps.Add(step);
        }
        /// <summary>
        /// 移除加载步骤
        /// </summary>
        /// <param name="stepName"></param>
        private static void RemoveLoadingStep(string stepName)
        {
            int index = m_loadingSteps.FindIndex(_ => _.Description == stepName);
            if (index >= 0)
                m_loadingSteps.RemoveAt(index);
        }

        /// <summary>
        /// 加载游戏主入口
        /// </summary>
        public static void StartLoading()
        {
            if (IsLoading)
                return;
            Debug.Log("AAAAAAAAA");
            CleanupState();
            IsLoading = true;
            //添加加载步骤
            AddLoadingSteps();
            //依次执行加载步骤
            s_coroutine = CoroutineManager.Start(LoadCoroutine(), OnLoadCoroutineFinishedOk, OnLoadCoroutineFinishedWithError);
        }

        /// <summary>
        /// 编辑器下的取消加载按钮点击
        /// </summary>
        public static void StopLoading()
        {
            if (!IsLoading)
                return;

            CleanupState();

            if (s_coroutine != null)
                CoroutineManager.Stop(s_coroutine);
            s_coroutine = null;

            F.InvokeEventExceptionSafe(onLoadingFinished);
        }

        /// <summary>
        /// 清理重置状态
        /// </summary>
        static void CleanupState()
        {
            IsLoading = false;
            HasLoaded = false;
            m_hasErrors = false;
            m_loadException = null;
            m_currentStepIndex = 0;
            LoadingStatus = "";
        }

        /// <summary>
        /// 加载成功执行完成
        /// </summary>
        static void OnLoadCoroutineFinishedOk()
        {
            CleanupState();
            HasLoaded = true;

            F.InvokeEventExceptionSafe(onLoadingFinished);
            // notify all scripts
            F.SendMessageToObjectsOfType<MonoBehaviour>("OnLoaderFinished");
        }

        /// <summary>
        /// 加载执行完成出错
        /// </summary>
        /// <param name="exception"></param>
        static void OnLoadCoroutineFinishedWithError(System.Exception exception)
        {
            LastLoadingStatusWhenErrorHappened = LoadingStatus;
            CleanupState();
            m_hasErrors = true;
            m_loadException = exception;

            F.InvokeEventExceptionSafe(onLoadingFinished);
        }

        /// <summary>
        /// 依次执行加载步骤
        /// </summary>
        /// <returns></returns>
        private static IEnumerator LoadCoroutine()
        {

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Debug.Log("Started loading GTA");

            // wait a few frames - to "unblock" the program, and to let other scripts initialize before
            // registering their loading steps
            yield return null;
            yield return null;

            // calculate total loading time
            m_totalEstimatedLoadingTime = m_loadingSteps.Sum(step => step.EstimatedTime);

            var stopwatchForSteps = new System.Diagnostics.Stopwatch();

            foreach (var step in m_loadingSteps)
            {

                // wait some more time before going to next step, because sometimes Unity does something
                // in the background at the end of a frame, eg. it updates Collider positions if you changed them
                yield return null;

                // update description
                LoadingStatus = step.Description;
                yield return null;

                if (step.CompletedSuccessfully)
                {
                    m_currentStepIndex++;
                    continue;
                }

                stopwatchForSteps.Restart();

                var en = step.Coroutine;

                if (en != null)
                {
                    // this step uses coroutine

                    bool hasNext = true;

                    while (hasNext)
                    {

                        UnityEngine.Profiling.Profiler.BeginSample($"Loading step: {step.Description}");
                        hasNext = en.MoveNext();
                        UnityEngine.Profiling.Profiler.EndSample();

                        // update description
                        LoadingStatus = step.Description;
                        yield return null;

                    }
                }
                else
                {
                    // this step uses a function

                    UnityEngine.Profiling.Profiler.BeginSample($"Loading step: {step.Description}");
                    step.LoadFunction();
                    UnityEngine.Profiling.Profiler.EndSample();
                }

                // step finished it's work

                step.CompletedSuccessfully = true;
                step.TimeElapsed = stopwatchForSteps.ElapsedMilliseconds;

                m_currentStepIndex++;

                Debug.LogFormat("{0} - finished in {1} ms", step.Description, step.TimeElapsed);
            }

            // all steps finished loading

            Debug.Log("GTA loading finished in " + stopwatch.Elapsed.TotalSeconds + " seconds");

        }

        /// <summary>
        /// 配置是否加载图片
        /// </summary>
        private static void StepConfigure()
        {
            TextureDictionary.DontLoadTextures = Config.GetBool("dontLoadTextures");
            Debug.Log($"gcj: TextureDictionary.DontLoadTextures: {TextureDictionary.DontLoadTextures}");
        }

        /// <summary>
        /// 选择GTA路径，如果路径已经设置，会自动跳过
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.Exception"></exception>
        private static IEnumerator StepSelectGTAPath()
        {
            yield return null;
            
            string path = Config.GetPath(Config.const_game_dir);
            Debug.Log("gcj:AAAAAAAAAAAAAAApath: " + path);
            if (string.IsNullOrEmpty(path))
                path = Application.persistentDataPath + "/10150500/";
            if (string.IsNullOrEmpty(path))
            {
                // path is not set

                // if we can't show file browser, throw exception
                if (F.IsInHeadlessMode || F.IsAppInEditMode)
                    throw new System.InvalidOperationException("Game path is not set");

                // show file browser to user to select path
                m_showFileBrowser = true;
            }
            else
            {
                Debug.Log("gcj: Game path is already set");
                yield break;
            }

            // wait until user selects a path
            while (m_showFileBrowser)
            {
                yield return null;
            }

            // refresh path
            path = Config.GetPath(Config.const_game_dir);

            if (string.IsNullOrEmpty(path))
            {
                // path was not set
                throw new System.Exception("Path to GTA was not set");
            }

        }

        /// <summary>
        /// 检查游戏路径是否合法，会检查路径下是否存在models和data文件夹
        /// </summary>
        /// <param name="gamePath"></param>
        /// <exception cref="System.Exception"></exception>
        public static void CheckIfGamePathIsCorrect(string gamePath)
        {
            string[] directoriesToCheck = { "models", "data" };

            foreach (string directoryToCheck in directoriesToCheck)
            {
                string[] caseVariations =
                {
                    directoryToCheck,
                    directoryToCheck.FirstCharToUpper(),
                    directoryToCheck.ToUpperInvariant(),
                };

                if (caseVariations.All(d => !Directory.Exists(Path.Combine(gamePath, d))))
                    throw new System.Exception($"Game folder seems to be invalid - failed to find '{directoryToCheck}' folder inside game folder");
            }

        }

        /// <summary>
        /// 更改游戏路径的时候检查游戏路径是否合法
        /// </summary>
        /// <param name="gamePath"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool IsGamePathCorrect(string gamePath, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                CheckIfGamePathIsCorrect(gamePath);
                return true;
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 读取档案资源文件
        /// </summary>
        private static void StepLoadArchives()
        {
            CheckIfGamePathIsCorrect(Config.GamePath);

            //先加载所有的档案文件
            ArchiveManager.LoadLooseArchive(Config.GamePath);

            foreach (string imgFilePath in ArchiveManager.GetFilePathsFromLooseArchivesWithExtension(".img"))
            {
                Debug.Log("imgFilePath: " + imgFilePath);
                //再加载.img格式的文件
                ArchiveManager.LoadImageArchive(imgFilePath);
            }
            //这时候有9个IArchive文件，一个LooseArchive和8个ImageArchive
            Debug.Log($"gcj: num archives loaded: {ArchiveManager.GetNumArchives()}, num entries loaded: {ArchiveManager.GetTotalNumLoadedEntries()}");

        }

        /// <summary>
        /// 加载启动画面
        /// </summary>
        private static void StepLoadSplashScreen()
        {
            var txd = TextureDictionary.Load("LOADSCS");

            int index1 = Random.Range(1, 15);
            int index2 = Random.Range(1, 15);

            SplashTex1 = txd.GetDiffuse("loadsc" + index1).Texture;
            SplashTex2 = txd.GetDiffuse("loadsc" + index2).Texture;

        }

        /// <summary>
        /// 设置第一个启动画面
        /// </summary>
        private static void StepSetSplash1()
        {
            CurrentSplashTex = SplashTex1;
        }

        /// <summary>
        /// 设置第二个启动画面
        /// </summary>
        private static void StepSetSplash2()
        {
            CurrentSplashTex = SplashTex2;
        }

        /// <summary>
        /// 加载声音文件
        /// </summary>
        private static void StepLoadAudio()
        {
            Audio.AudioManager.InitFromLoader();
        }

        /// <summary>
        /// 加载字体文件
        /// </summary>
        private static void StepLoadFonts()
        {
            Importing.FontsImporter.LoadFonts();
        }

        /// <summary>
        /// 加载碰撞器步骤
        /// </summary>
        private static void StepLoadCollision()
        {

            int numCollisionFiles = 0;

            foreach (var colFile in ArchiveManager.GetFileNamesWithExtension(".col"))
            {
                //Debug.Log($"gcj: StepLoadCollision colFile: {colFile}");
                //这里会读取碰撞器文件，文件内包含了碰撞器的名称和它对应的物体名称等信息
                CollisionFile.Load(colFile);
                numCollisionFiles++;
            }

            Debug.Log("gcj: Number of collision files " + numCollisionFiles);

        }

        private static void StepLoadItemInfo()
        {
            //"item_paths": [
            //"${game_dir}/data/gta.dat",
            //"${game_dir}/data/vehicles.ide",
            //"${game_dir}/data/peds.ide",
            //"${game_dir}/data/default.ide"
            //]
            //循环gta.dat，vehicles.ide，peds.ide，default.ide四个文件
            foreach (var p in Config.GetPaths("item_paths"))
            {
                Debug.Log($"gcj: StepLoadItemInfo: {p}");
                string path = ArchiveManager.PathToCaseSensitivePath(p);
                var ext = Path.GetExtension(path).ToLower();
                switch (ext)
                {
                    case ".dat":
                        //读取gta.dat
                        Item.ReadLoadList(path);
                        break;

                    case ".ide":
                        //data/maps/country/countn2.ide中包含模型的信息：16000, drvin_screen, con_drivein, 150, 4
                        Item.ReadIde(path);
                        break;

                    case ".ipl":
                        Debug.Log($"gcj: StepLoadItemInfo Item.ReadIpl{path}");
                        Item.ReadIpl(path);
                        break;
                }
            }

        }

        private static void StepLoadHandling()
        {
            Handling.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("handling_path")));
        }

        private static void StepLoadAnimGroups()
        {
            AnimationGroup.Load("animgrp.dat");

            // load custom anim groups from resources
            TextAsset textAsset = Resources.Load<TextAsset>("Data/auxanimgrp");
            AnimationGroup.LoadFromStreamReader(new StreamReader(new MemoryStream(textAsset.bytes)));

        }

        private static void StepLoadCarColors()
        {
            CarColors.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("car_colors_path")));
        }

        private static void StepLoadWeaponsData()
        {
            Importing.Weapons.WeaponData.Load(ArchiveManager.PathToCaseSensitivePath(Config.GetPath("weapons_path")));
        }

        /// <summary>
        /// 加载小地图
        /// </summary>
        private static void StepLoadMap()
        {
            MiniMap.Instance.Load();
        }

        /// <summary>
        /// 加载鼠标Icon
        /// </summary>
        private static void StepLoadSpecialTextures()
        {

            // Load mouse cursor texture
            F.RunExceptionSafe(() =>
            {
                Texture2D mouse = TextureDictionary.Load("fronten_pc").GetDiffuse("mouse",
                    new TextureLoadParams() { makeNoLongerReadable = false }).Texture;
                Texture2D mouseFix = new Texture2D(mouse.width, mouse.height);

                for (int x = 0; x < mouse.width; x++)
                    for (int y = 0; y < mouse.height; y++)
                        mouseFix.SetPixel(x, mouse.height - y - 1, mouse.GetPixel(x, y));

                mouseFix.Apply();

                if (!F.IsInHeadlessMode)
                    Cursor.SetCursor(mouseFix, Vector2.zero, CursorMode.Auto);
            });

            // fist texture
            Weapon.FistTexture = TextureDictionary.Load("hud").GetDiffuse("fist").Texture;


            onLoadSpecialTextures();

        }

        private static void StepLoadGXT()
        {
            GXT.Load();
        }

        private static void StepLoadPaths()
        {
            Importing.Paths.NodeReader.Load();
        }


        public static float GetProgressPerc()
        {
            if (m_currentStepIndex <= 0)
                return 0f;

            if (m_currentStepIndex >= m_loadingSteps.Count)
                return 1f;

            float estimatedTimePassed = 0f;
            for (int i = 0; i < m_currentStepIndex; i++)
            {
                estimatedTimePassed += m_loadingSteps[i].EstimatedTime;
            }

            return Mathf.Clamp01(estimatedTimePassed / m_totalEstimatedLoadingTime);
        }


        private void Update()
        {

        }

        private void OnGUI()
        {
            if (HasLoaded)
                return;
            if (!m_hasErrors && !IsLoading)
                return;

            // background

            if (CurrentSplashTex != null)
            {
                GUIUtils.DrawTextureWithYFlipped(new Rect(0, 0, Screen.width, Screen.height), CurrentSplashTex);
            }
            else
            {
                GUIUtils.DrawRect(new Rect(0, 0, Screen.width, Screen.height), Color.black);
            }

            // display loading progress

            GUILayout.BeginArea(new Rect(10, 5, 400, Screen.height - 5));

            // current status
            GUILayout.Label("<size=25>" + (IsLoading ? LoadingStatus : LastLoadingStatusWhenErrorHappened) + "</size>");

            // progress bar
            GUILayout.Space(10);
            DisplayProgressBar();

            // display error
            if (m_hasErrors)
            {
                GUILayout.Space(20);
                GUILayout.Label("<size=20>" + "The following error occured during the current step:" + "</size>");
                GUILayout.TextArea(m_loadException.ToString());
                GUILayout.Space(30);
                if (GUIUtils.ButtonWithCalculatedSize("Exit", 80, 30))
                {
                    GameManager.ExitApplication();
                }
                GUILayout.Space(5);
            }

            // display all steps
            //			GUILayout.Space (10);
            //			DisplayAllSteps ();

            GUILayout.EndArea();

            DisplayFileBrowser();

        }

        /// <summary>
        /// 显示所有步骤，官方已注释掉，未使用
        /// </summary>
        private static void DisplayAllSteps()
        {

            int i = 0;
            foreach (var step in m_loadingSteps)
            {
                GUILayout.Label(step.Description + (m_currentStepIndex > i ? (" - " + step.TimeElapsed + " ms") : ""));
                i++;
            }

        }
        /// <summary>
        /// 显示进度条
        /// </summary>
        private static void DisplayProgressBar()
        {
            float width = 200;
            float height = 12;

            //			Rect rect = GUILayoutUtility.GetLastRect ();
            //			rect.position += new Vector2 (0, rect.height);
            //			rect.size = new Vector2 (width, height);

            Rect rect = GUILayoutUtility.GetRect(width, height);
            rect.width = width;

            float progressPerc = GetProgressPerc();
            GUIUtils.DrawBar(rect, progressPerc, new Vector4(149, 185, 244, 255) / 256.0f, new Vector4(92, 147, 237, 255) / 256.0f, 2f);

        }

        /// <summary>
        /// 显示设置GTA路径的对话框
        /// </summary>
        private static void DisplayFileBrowser()
        {
            if (!m_showFileBrowser)
                return;

            if (null == m_fileBrowser)
            {
                Rect rect = GUIUtils.GetCenteredRect(FileBrowser.GetRecommendedSize());

                m_fileBrowser = new FileBrowser(rect, "Select path to GTA", GUI.skin.window, (string path) =>
                {
                    m_showFileBrowser = false;
                    Config.SetString(Config.const_game_dir, path);
                    Config.SaveUserConfigSafe();
                });
                m_fileBrowser.BrowserType = FileBrowserType.Directory;
            }

            m_fileBrowser.OnGUI();

        }

    }
}
