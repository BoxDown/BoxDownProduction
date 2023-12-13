using System.Collections.Generic;
using UnityEngine;
using Gun;
using System.Linq;
using System.Collections;
using Utility;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Managers
{
    [RequireComponent(typeof(PlayerInput))]
    public class GameManager : MonoBehaviour
    {
        [Rename("Main Menu Holder"), SerializeField] Transform C_mainMenu;

        public static GameManager gameManager
        {
            get;
            private set;
        }

        [Rename("Rewards Per Room"), SerializeField] private int i_rewardsPerRoom = 1;
        [Rename("Amount Of Easy Rooms"), SerializeField] private int i_easyRooms = 2;
        [Rename("Amount Of Medium Rooms"), SerializeField] private int i_mediumRooms = 2;
        [Rename("Amount Of Hard Rooms"), SerializeField] private int i_hardRooms = 2;

        [Rename("Player Input")] private PlayerInput C_playerInput;

        [Space(10)]
        [Rename("Debug Game"), SerializeField] public bool b_debugMode;
        [Rename("Spawn All Modules"), SerializeField] public bool b_spawnAllModules;
        [Rename("Music On/Off"), SerializeField] public bool b_musicOnOff;
        [Rename("Music Volume"), Range(0, 1), SerializeField] private float f_musicVolume = 0.1f;
        [Rename("Sound Volume"), Range(0, 1), SerializeField] private float f_soundVolume = 0.8f;

        [Space(10)]
        [Rename("Bullet Object Pool Size"), SerializeField] private int i_bulletCount = 500;
        [Rename("Bullet Game Prefab"), SerializeField] private GameObject C_bulletPrefab;
        private BulletObjectPool C_bulletPool;

        [Space(10)]
        [Header("Plug in documents from assets folder")]
        [Rename("All Levels Document")] public TextAsset C_allLevels;
        [Rename("All Modules Document")] public TextAsset C_allGunModules;

        [Space(10)]
        [Header("UI Default Buttons")]
        [Rename("Default Main Menu Button")] public GameObject C_defaultMainMenuButton;
        [Rename("Default Pause Button")] public GameObject C_defaultPauseButton;
        [Rename("Default Swap Button")] public GameObject C_defaultSwapButton;
        [Rename("Default Results Button")] public GameObject C_defaultResultsButton;
        [Rename("Default Credits Button")] public GameObject C_defaultCreditsButton;

        [Space(10)]
        [Header("Gun Module Spawn VFX")]
        [Rename("Trigger Module Spawn Effect"), SerializeField] private GameObject C_triggerSpawnVFX;
        [Rename("Clip Module Spawn Effect"), SerializeField] private GameObject C_clipSpawnVFX;
        [Rename("Barrel Module Spawn Effect"), SerializeField] private GameObject C_barrelSpawnVFX;

        [Space(10)]
        [Rename("Outline Material"), SerializeField] private Material C_outlineMaterial;



        private EventSystem C_eventSystem;

        private Door.RoomType e_currentRewardType;
        private GunModule[] aC_previousModules = new GunModule[3];
        private Vector3 S_rewardPoint;
        private List<string> ls_allGunModulesNames = new List<string>();
        private List<string> ls_triggerGunModulesNames = new List<string>();
        private List<string> ls_clipGunModulesNames = new List<string>();
        private List<string> ls_barrelGunModulesNames = new List<string>();

        private List<GunModule> lC_allGunModules = new List<GunModule>();
        private List<GunModule> lC_triggerGunModules = new List<GunModule>();
        private List<GunModule> lC_clipGunModules = new List<GunModule>();
        private List<GunModule> lC_barrelGunModules = new List<GunModule>();
        public int i_currentRoom
        {
            get;
            private set;
        }

        private List<string> ls_allLevels = new List<string>(); // all levels should not be used to load any scenes
        private List<string> ls_easyLevels = new List<string>();
        private List<string> ls_mediumLevels = new List<string>();
        private List<string> ls_hardLevels = new List<string>();

        private PlayerController C_player;
        private CameraDolly C_camera;
        private bool b_usingUIActions = true;

        private BulletObjectPool C_bulletObjectPool;

        public GunModuleUIAnimations C_gunModuleUI
        {
            get;
            private set;
        }

        private void Update()
        {
            if (C_playerInput != null)
            {
                ControlManager.ChangeInputDevice(C_playerInput.currentControlScheme);
            }
            CurrentSelectionCheck();
        }

        #region GamePlayFunctons
        // Start is called before the first frame update
        void Awake()
        {
#if !UNITY_EDITOR
        // fixes new Unity2021 bug where the secondary monitor was being chosen when the game launches in a release build
        StartCoroutine("MoveToPrimaryDisplay");
#endif
            if (gameManager != null && gameManager != this)
            {
                Destroy(gameObject);
                return;
            }
            int targetHeight = Screen.width * 9 / 16;

            // Set the game's resolution to match the target width and height
            Screen.SetResolution(Screen.width, targetHeight, true);
            if (!b_debugMode)
            {
                StartCoroutine(StartUp());
            }
            else
            {
                Initialise();
            }
            AudioManager.SetMusicVolume(f_musicVolume);
            AudioManager.SetSoundVolume(f_soundVolume);
        }

        public static void SetOutlineMaterialColour(Color color)
        {
            gameManager.C_outlineMaterial.SetColor("_OuterColor", color);
        }

        private static void CreateBulletPool()
        {
            GameObject bulletPool = new GameObject();
            DontDestroyOnLoad(bulletPool);
            bulletPool.name = "Bullet Pool";
            gameManager.C_bulletPool = bulletPool.AddComponent<BulletObjectPool>();
            bulletPool.GetComponent<BulletObjectPool>().CreatePool(gameManager.i_bulletCount, gameManager.C_bulletPrefab);
        }

        public static BulletObjectPool GetBulletPool()
        {
            return gameManager.C_bulletPool;
        }

        public void SpawnNextReward()
        {
            for (int i = 0; i < i_rewardsPerRoom; i++)
            {
                GetRandomModule(e_currentRewardType);
            }
        }

        public void UpdateRewardPoint(Vector3 point)
        {
            S_rewardPoint = point;
        }
        public void UpdateRewardType(Door.RoomType type)
        {
            e_currentRewardType = type;
        }

        public void GrabAllGunModules()
        {
            ls_allGunModulesNames = TextDocumentReadWrite.ReadTextAsset(C_allGunModules).ToList();
        }

        private void GroupGunModules()
        {
            for (int i = 0; i < ls_allGunModulesNames.Count; i++)
            {
                if (ls_allGunModulesNames[i].Contains("Trigger"))
                {
                    ls_triggerGunModulesNames.Add(ls_allGunModulesNames[i]);
                }
                else if (ls_allGunModulesNames[i].Contains("Clip"))
                {
                    ls_clipGunModulesNames.Add(ls_allGunModulesNames[i]);
                }
                else if (ls_allGunModulesNames[i].Contains("Barrel"))
                {
                    ls_barrelGunModulesNames.Add(ls_allGunModulesNames[i]);
                }
            }
        }

        public void SetPreviousModules()
        {
            for (int i = 0; i < aC_previousModules.Count(); i++)
            {
                aC_previousModules[i] = C_player.C_ownedGun.aC_moduleArray[i];
            }
        }

        private void CompareCurrentAndPreviousModules()
        {
            if (aC_previousModules[0] != C_player.C_ownedGun.aC_moduleArray[0])
            {
                IncrementTriggerSwap();
            }
            if (aC_previousModules[1] != C_player.C_ownedGun.aC_moduleArray[1])
            {
                IncrementClipSwap();
            }
            if (aC_previousModules[2] != C_player.C_ownedGun.aC_moduleArray[2])
            {
                IncrementBarrelSwap();
            }
        }

        private void IncrementRoom()
        {
            i_currentRoom++;
        }

        //what doors call
        public void MoveToNextRoom()
        {
            CompareCurrentAndPreviousModules();
            LoadNextRoom();
            SetPreviousModules();
        }
        private void LoadNextRoom()
        {
            int roomNumberToLoad = 0;
            if (i_currentRoom < i_easyRooms)
            {
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count() - 1);
                while (ls_easyLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                }
                IncrementRoom();
                StartCoroutine(SceneTransition(ls_easyLevels[roomNumberToLoad]));
                return;
            }
            else if (i_currentRoom < i_easyRooms + i_mediumRooms)
            {
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count() - 1);
                while (ls_mediumLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_mediumLevels.Count());
                }
                IncrementRoom();
                StartCoroutine(SceneTransition(ls_mediumLevels[roomNumberToLoad]));

                return;
            }
            else if (i_currentRoom < i_easyRooms + i_mediumRooms + i_hardRooms)
            {

                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count() - 1);
                while (ls_hardLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_hardLevels.Count());
                }
                IncrementRoom();
                StartCoroutine(SceneTransition(ls_hardLevels[roomNumberToLoad]));
                return;
            }
            else if (i_currentRoom == i_easyRooms + i_mediumRooms + i_hardRooms)
            {
                IncrementRoom();
                StartCoroutine(SceneTransition("HardBreakRoom"));
                return;
            }
            else
            {
                IncrementRoom();
                StartCoroutine(SceneTransition("FinalLevel"));
            }
        }
        private void DeclareAllLevels()
        {
            List<string> allLevelsInProject = new List<string>();
            string levelFileDirectory = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Levels";

            string[] directories = Directory.GetDirectories(levelFileDirectory);

            for (int i = 0; i < directories.Length; i++)
            {
                string[] filesInDirectory = Directory.GetFiles(directories[i]);
                for (int j = 0; j < filesInDirectory.Length; j++)
                {
                    if (filesInDirectory[j].Contains(".meta"))
                    {
                        continue;
                    }

                    //split name from GunModules Folder
                    string[] stringSplit = filesInDirectory[j].Split("Levels\\");
                    //remove ".asset"
                    string levelName = stringSplit[1].Remove(stringSplit[1].Length - 6);

                    //add name to list
                    allLevelsInProject.Add(levelName);
                }
            }

            TextDocumentReadWrite.FileWrite(Directory.GetCurrentDirectory() + "\\Assets" + "\\allLevels.txt", allLevelsInProject.ToArray());
        }

        private void GroupLevels(List<string> levelsList)
        {
            for (int i = 0; i < levelsList.Count(); i++)
            {
                if (levelsList[i].Contains("Easy"))
                {
                    ls_easyLevels.Add(levelsList[i].Split("\\")[1]);
                }
                if (levelsList[i].Contains("Medium"))
                {
                    ls_mediumLevels.Add(levelsList[i].Split("\\")[1]);
                }
                if (levelsList[i].Contains("Hard"))
                {
                    ls_hardLevels.Add(levelsList[i].Split("\\")[1]);
                }
            }
        }
        private void GrabAllLevels()
        {
            ls_allLevels = TextDocumentReadWrite.ReadTextAsset(C_allLevels).ToList();
        }
        private void LoadGunModuleStringsAsModules()
        {
            foreach (string gunModuleName in ls_allGunModulesNames)
            {
                GunModule moduleToLoad = Resources.Load<GunModule>(GunModuleSpawner.GetGunModuleResourcesPath(gunModuleName));
                lC_allGunModules.Add(moduleToLoad);
            }
            foreach (string gunModuleName in ls_triggerGunModulesNames)
            {
                GunModule moduleToLoad = Resources.Load<GunModule>(GunModuleSpawner.GetGunModuleResourcesPath(gunModuleName));
                lC_triggerGunModules.Add(moduleToLoad);
            }
            foreach (string gunModuleName in ls_clipGunModulesNames)
            {
                GunModule moduleToLoad = Resources.Load<GunModule>(GunModuleSpawner.GetGunModuleResourcesPath(gunModuleName));
                lC_clipGunModules.Add(moduleToLoad);
            }
            foreach (string gunModuleName in ls_barrelGunModulesNames)
            {
                GunModule moduleToLoad = Resources.Load<GunModule>(GunModuleSpawner.GetGunModuleResourcesPath(gunModuleName));
                lC_barrelGunModules.Add(moduleToLoad);
            }
        }

        private void GetRandomModule(Door.RoomType rewardType)
        {
            switch (e_currentRewardType)
            {
                case Door.RoomType.Trigger:
                    float totalChance = 0;
                    foreach (GunModule gM in lC_triggerGunModules)
                    {
                        float chanceToAdd = Mathf.Clamp(gM.f_moduleStrength, 1, 100);
                        totalChance += 1 / chanceToAdd;
                    }
                    float randomNumber = Random.Range(0, totalChance);
                    for (int i = 0; i < lC_triggerGunModules.Count; i++)
                    {
                        float chanceToTake = Mathf.Clamp(lC_triggerGunModules[i].f_moduleStrength, 1, 100);
                        randomNumber -= 1 / chanceToTake;
                        if (randomNumber <= 0)
                        {
                            if (ls_triggerGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[0].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            GunModuleSpawner.SpawnGunModule(ls_triggerGunModulesNames[i], S_rewardPoint);
                            break;
                        }
                    }
                    break;

                case Door.RoomType.Clip:
                    totalChance = 0;
                    foreach (GunModule gM in lC_clipGunModules)
                    {
                        float chanceToAdd = Mathf.Clamp(gM.f_moduleStrength, 1, 100);
                        totalChance += 1 / chanceToAdd;
                    }
                    randomNumber = Random.Range(0, totalChance);
                    for (int i = 0; i < lC_clipGunModules.Count; i++)
                    {
                        float chanceToTake = Mathf.Clamp(lC_clipGunModules[i].f_moduleStrength, 1, 100);
                        randomNumber -= 1 / chanceToTake;
                        if (randomNumber <= 0)
                        {
                            if (ls_clipGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[1].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            GunModuleSpawner.SpawnGunModule(ls_clipGunModulesNames[i], S_rewardPoint);
                            break;
                        }
                    }
                    break;
                case Door.RoomType.Barrel:
                    totalChance = 0;
                    foreach (GunModule gM in lC_barrelGunModules)
                    {
                        float chanceToAdd = Mathf.Clamp(gM.f_moduleStrength, 1, 100);
                        totalChance += 1 / chanceToAdd;
                    }
                    randomNumber = Random.Range(0, totalChance);
                    for (int i = 0; i < lC_barrelGunModules.Count; i++)
                    {
                        float chanceToTake = Mathf.Clamp(lC_barrelGunModules[i].f_moduleStrength, 1, 100);
                        randomNumber -= 1 / chanceToTake;
                        if (randomNumber <= 0)
                        {
                            if (ls_barrelGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[2].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            GunModuleSpawner.SpawnGunModule(ls_barrelGunModulesNames[i], S_rewardPoint);
                            break;
                        }
                    }
                    break;
                case Door.RoomType.RandomModule:
                    totalChance = 0;
                    foreach (GunModule gM in lC_allGunModules)
                    {
                        float chanceToAdd = Mathf.Clamp(gM.f_moduleStrength, 1, 100);
                        totalChance += 1 / chanceToAdd;
                    }
                    randomNumber = Random.Range(0, totalChance);
                    for (int i = 0; i < lC_allGunModules.Count; i++)
                    {
                        float chanceToTake = Mathf.Clamp(lC_allGunModules[i].f_moduleStrength, 1, 100);
                        randomNumber -= 1 / chanceToTake;
                        if (randomNumber <= 0)
                        {
                            if (ls_allGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[0].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            if (ls_allGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[1].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            if (ls_allGunModulesNames[i].Split("\\")[1] == C_player.C_ownedGun.aC_moduleArray[2].name)
                            {
                                GetRandomModule(e_currentRewardType);
                                return;
                            }
                            GunModuleSpawner.SpawnGunModule(ls_allGunModulesNames[i], S_rewardPoint);
                            break;
                        }
                    }
                    break;
            }
        }

        public static void SpawnModuleVFX(GunModule.ModuleSection moduleType, Vector3 position)
        {
            switch (moduleType)
            {
                case GunModule.ModuleSection.Trigger:
                    gameManager.SpawnTriggerVFX(position);
                    break;
                case GunModule.ModuleSection.Clip:
                    gameManager.SpawnClipVFX(position);
                    break;
                case GunModule.ModuleSection.Barrel:
                    gameManager.SpawnBarrelVFX(position);
                    break;
            }
        }

        public void SpawnTriggerVFX(Vector3 position)
        {
            Destroy(Instantiate(gameManager.C_triggerSpawnVFX, position, Quaternion.identity), 5);
        }
        public void SpawnClipVFX(Vector3 position)
        {
            Destroy(Instantiate(gameManager.C_clipSpawnVFX, position, Quaternion.identity), 5);
        }
        public void SpawnBarrelVFX(Vector3 position)
        {
            Destroy(Instantiate(gameManager.C_barrelSpawnVFX, position, Quaternion.identity), 5);
        }
        #endregion

        #region UIFunctions
        [HideInInspector] public bool b_cull = true;
        [HideInInspector] public bool b_cullLastFrame = true;
        public void SetCulling(bool cullingOnOff)
        {
            b_cull = cullingOnOff;
        }

        public static void StartGame()
        {
            gameManager.StartCoroutine(gameManager.StartGameCoroutine());
        }

        public IEnumerator StartGameCoroutine()
        {
            gameManager.ResetAllStats();
            gameManager.i_currentRoom = 0;
            gameManager.e_currentRewardType = Door.RoomType.RandomModule;
            DeactivateMainMenu();
            ResultsUI.DeactivateResults();
            SetStartTime();
            gameManager.StartCoroutine(gameManager.SceneTransition("StartBreakRoom"));
            AudioManager.TransitionToBattleTheme();
            InGameUI.ResetLevelProgression();
            ResultsUI.ResetResults();
            yield return new WaitForSeconds(f_sceneTransitionTime);
            InGameUI.ActivateInGameUI();
        }

        public static void OpenCreditsMenu()
        {
            CreditsMenu.Activate();
            CurrentSelectionCreditsMenu();
            DeactivateMainMenu();
        }
        public static void ActivateMainMenu()
        {
            gameManager.C_mainMenu.gameObject.SetActive(true);
        }
        public static void DeactivateMainMenu()
        {
            gameManager.C_mainMenu.gameObject.SetActive(false);
        }
        public static void RestartGame()
        {
            //AudioManager.StartMusicLoop();
            AudioManager.TransitionToBattleTheme();
            InGameUI.ResetLevelProgression();
            gameManager.RemovePlayer();
            gameManager.RemoveCamera();
            ResultsUI.ResetResults();
            StartGame();
        }
        //deactivate all menus then back to main menu scene to have an empty scene with nothing but the menu
        public static void BackToMainMenu()
        {
            if (PauseMenu.pauseMenu.b_gamePaused)
            {
                PauseMenu.DeactivatePause();
                PauseMenu.pauseMenu.b_gamePaused = false;
                Time.timeScale = 1;
            }
            WeaponsSwapUI.swapUI.TurnOff();
            ResultsUI.DeactivateResults();
            InGameUI.DeactivateInGameUI();
            CreditsMenu.Deactivate();
            gameManager.RemovePlayer();
            //AudioManager.StopMusicLoop();
            gameManager.RemoveCamera();
            SceneManager.LoadScene("MainMenu");
            ActivateMainMenu();
            gameManager.SetCulling(true);
            AudioManager.TransitionToMainMenu();
            SwitchToUIActions();
            CurrentSelectionMainMenu();
        }

        public static void ExitGame()
        {
            Application.Quit();
        }

        private IEnumerator StartUp()
        {
            yield return 0;
            Initialise();
        }

        private void Initialise()
        {
            DontDestroyOnLoad(this);
            if (gameManager != null && gameManager != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                gameManager = this;
            }
            CreateBulletPool();
            C_playerInput = GetComponent<PlayerInput>();
            C_gunModuleUI = FindObjectOfType<GunModuleUIAnimations>();
            if (b_debugMode)
            {
                PauseMenu.DeactivatePause();
                CreditsMenu.Deactivate();
                WeaponsSwapUI.Deactivate();
                ResultsUI.DeactivateResults();
                DeactivateMainMenu();
                FindObjectOfType<PlayerController>().Initialise();
                if (b_musicOnOff)
                {
                    AudioManager.StartMusicLoop();
                    AudioManager.TransitionToBattleTheme();
                    AudioManager.SetBattleMusicLowIntensity();
                    AudioManager.SetMusicVolume(0.1f);
                    AudioManager.SetSoundVolume(1.0f);
                }
            }
            else
            {
                CreditsMenu.Deactivate();
                PauseMenu.DeactivatePause();
                InGameUI.DeactivateInGameUI();
                ResultsUI.DeactivateResults();
                WeaponsSwapUI.Deactivate();
                if (b_musicOnOff)
                {
                    AudioManager.StartMusicLoop();
                }
            }

            // make module lists
#if UNITY_EDITOR
            GunModuleSpawner.DeclareAllGunModules();
            DeclareAllLevels();
#endif


            GrabAllGunModules();
            GroupGunModules();
            LoadGunModuleStringsAsModules();
            GrabAllLevels();
            GroupLevels(ls_allLevels);

            if (b_debugMode && b_spawnAllModules)
            {
                SpawnAllGunModules();
            }



            i_currentRoom = 0;

            C_eventSystem = GetComponentInChildren<EventSystem>();
            CurrentSelectionMainMenu();
        }

        private IEnumerator SetCurrentSelected(GameObject newSelection)
        {
            yield return 0;
            C_eventSystem.SetSelectedGameObject(newSelection);
        }

        public static void CurrentSelectionMainMenu()
        {
            gameManager.StartCoroutine(gameManager.SetCurrentSelected(gameManager.C_defaultMainMenuButton));
        }
        public static void CurrentSelectionPauseMenu()
        {
            gameManager.StartCoroutine(gameManager.SetCurrentSelected(gameManager.C_defaultPauseButton));
        }
        public static void CurrentSelectionSwapMenu()
        {
            gameManager.StartCoroutine(gameManager.SetCurrentSelected(gameManager.C_defaultSwapButton));
        }
        public static void CurrentSelectionResultsMenu()
        {
            gameManager.StartCoroutine(gameManager.SetCurrentSelected(gameManager.C_defaultResultsButton));
        }
        public static void CurrentSelectionCreditsMenu()
        {
            gameManager.StartCoroutine(gameManager.SetCurrentSelected(gameManager.C_defaultCreditsButton));
        }

        private void CurrentSelectionCheck()
        {
            if (C_eventSystem == null)
            {
                return;
            }
            if (ControlManager.GetControllerType() == ControlManager.ControllerType.KeyboardMouse)
            {
                C_eventSystem.SetSelectedGameObject(null);
                return;
            }
            if (C_eventSystem.currentSelectedGameObject != null || !b_usingUIActions)
            {
                return;
            }
            if (C_mainMenu.gameObject.activeInHierarchy)
            {
                CurrentSelectionMainMenu();
                return;
            }
            if (PauseMenu.pauseMenu.gameObject.activeInHierarchy)
            {
                CurrentSelectionPauseMenu();
                return;
            }
            if (WeaponsSwapUI.swapUI.gameObject.activeInHierarchy)
            {
                CurrentSelectionSwapMenu();
                return;
            }
            if (ResultsUI.resultsUI.gameObject.activeInHierarchy)
            {
                CurrentSelectionResultsMenu();
                return;
            }
            if (CreditsMenu.creditsUI.gameObject.activeInHierarchy)
            {
                CurrentSelectionCreditsMenu();
                return;
            }
        }

        public static void SpawnAllGunModules()
        {
            for (int i = 0; i < gameManager.ls_allGunModulesNames.Count(); i++)
            {
                GunModuleSpawner.SpawnGunModule(gameManager.ls_allGunModulesNames[i], new Vector3((i % 6) * 1.5f, 0, (i / 6) * 1.5f));
            }
        }


        #endregion


        #region SceneTransitionAnimation
        [Header("Scene Transition Animations")]
        [Rename("Scene Transition Animator"), SerializeField] Animator C_sceneTransitionAnimator;
        [Rename("Scene Transition Time"), SerializeField] float f_sceneTransitionTime = 1;

        public static void StartTransitionAnimation()
        {
            if (gameManager.C_sceneTransitionAnimator != null)
            {
                gameManager.C_sceneTransitionAnimator.SetTrigger("Start");
            }
        }
        public static void FinishTransitionAnimation()
        {
            if (gameManager.C_sceneTransitionAnimator != null)
            {
                gameManager.C_sceneTransitionAnimator.SetTrigger("Stop");
            }
        }

        public IEnumerator SceneTransition(string sceneName)
        {
            StartTransitionAnimation();
            AudioManager.PlayFmodEvent("SFX/DoorClose", Vector3.zero);
            if (C_player != null)
            {
                SwitchOffInGameActions();
            }
            yield return new WaitForSeconds(f_sceneTransitionTime * 1.05f);
            SceneManager.LoadScene(sceneName);
            yield return new WaitForSeconds(f_sceneTransitionTime * 1.05f);
            AudioManager.PlayFmodEvent("SFX/DoorOpen", Vector3.zero);
            FinishTransitionAnimation();
            if (C_player != null)
            {
                SwitchToInGameActions();
                C_player.C_ownedGun.HardReload();
            }
        }

        #endregion

        #region PlayerFunctions

        public static void SwitchToUIActions()
        {
            if (gameManager.C_playerInput.currentActionMap.name == "PlayerControl")
            {
                SwitchOffInGameActions();
            }
            else if (gameManager.C_playerInput.currentActionMap.name == "SwapUI")
            {
                SwitchOffSwapUIActions();
            }
            else
            {
                return;
            }
            SwitchOffInGameActions();
            gameManager.C_playerInput.SwitchCurrentActionMap("UI");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            if (gameManager.C_player != null)
            {
                actionMap.FindAction("Pause").performed += gameManager.C_player.Pause;
            }
        }
        public static void SwitchOffUIActions()
        {
            gameManager.C_playerInput.SwitchCurrentActionMap("UI");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            if (gameManager.C_player != null)
            {
                actionMap.FindAction("Pause").performed -= gameManager.C_player.Pause;
            }
        }

        public static void SwitchToInGameActions()
        {
            if (gameManager.C_player == null)
            {
                return;
            }
            if (gameManager.C_playerInput.currentActionMap.name == "UI")
            {
                SwitchOffUIActions();
            }
            else if (gameManager.C_playerInput.currentActionMap.name == "SwapUI")
            {
                SwitchOffSwapUIActions();
            }
            gameManager.C_playerInput.SwitchCurrentActionMap("PlayerControl");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Enable();
            actionMap.FindAction("Movement").performed += gameManager.C_player.MoveInput;
            actionMap.FindAction("Movement").canceled += gameManager.C_player.StopMove;
            actionMap.FindAction("Rotate").performed += gameManager.C_player.RotationSet;
            actionMap.FindAction("Rotate").canceled += gameManager.C_player.RotationStop;
            actionMap.FindAction("Dodge").performed += gameManager.C_player.Dodge;
            actionMap.FindAction("Interact").performed += gameManager.C_player.Interact;
            actionMap.FindAction("Fire").performed += gameManager.C_player.Fire;
            actionMap.FindAction("Fire").canceled += gameManager.C_player.CancelFire;
            actionMap.FindAction("Reload").performed += gameManager.C_player.Reload;
            actionMap.FindAction("Pause").performed += gameManager.C_player.Pause;
        }
        public static void SwitchOffInGameActions()
        {
            if(gameManager.C_player == null)
            {
                return;
            }
            gameManager.C_playerInput.SwitchCurrentActionMap("PlayerControl");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Disable();
            actionMap.FindAction("Movement").performed -= gameManager.C_player.MoveInput;
            actionMap.FindAction("Movement").canceled -= gameManager.C_player.StopMove;
            actionMap.FindAction("Rotate").performed -= gameManager.C_player.RotationSet;
            actionMap.FindAction("Rotate").canceled -= gameManager.C_player.RotationStop;
            actionMap.FindAction("Dodge").performed -= gameManager.C_player.Dodge;
            actionMap.FindAction("Interact").performed -= gameManager.C_player.Interact;
            actionMap.FindAction("Fire").performed -= gameManager.C_player.Fire;
            actionMap.FindAction("Fire").canceled -= gameManager.C_player.CancelFire;
            actionMap.FindAction("Reload").performed -= gameManager.C_player.Reload;
            actionMap.FindAction("Pause").performed -= gameManager.C_player.Pause;
        }

        public static void SwitchToSwapUIActions()
        {

            if (gameManager.C_playerInput.currentActionMap.name == "PlayerControl")
            {
                SwitchOffInGameActions();
            }
            else if (gameManager.C_playerInput.currentActionMap.name == "UI")
            {
                SwitchOffUIActions();
            }

            gameManager.C_playerInput.SwitchCurrentActionMap("SwapUI");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Enable();
            actionMap.FindAction("Swap").performed += WeaponsSwapUI.SwapAction;
            actionMap.FindAction("Keep").performed += WeaponsSwapUI.BackAction;
        }
        public static void SwitchOffSwapUIActions()
        {
            gameManager.C_playerInput.SwitchCurrentActionMap("SwapUI");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Disable();
            actionMap.FindAction("Swap").performed -= WeaponsSwapUI.SwapAction;
            actionMap.FindAction("Keep").performed -= WeaponsSwapUI.BackAction;
        }

        public static void SetPlayer(PlayerController newPlayer)
        {
            gameManager.C_player = newPlayer;
            SwitchToInGameActions();
            gameManager.SetPreviousModules();
        }
        public static PlayerController GetPlayer()
        {
            return gameManager.C_player;
        }
        public static void SetCamera(CameraDolly camera)
        {
            gameManager.C_camera = camera;
        }
        public static CameraDolly GetCamera()
        {
            return gameManager.C_camera;
        }

        public void RemovePlayer()
        {
            if (C_player != null)
            {
                InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
                actionMap.FindAction("Pause").performed -= gameManager.C_player.Pause;
                DestroyImmediate(C_player.gameObject);
                C_player = null;
            }

        }
        public void RemoveCamera()
        {
            if (C_camera != null)
            {
                DestroyImmediate(C_camera.gameObject);
                C_camera = null;
            }
        }
        #endregion

        //fix unity bug not targeting main monitor
        IEnumerable MoveToPrimaryDisplay()
        {
            List<DisplayInfo> displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);
            if (displays?.Count > 0)
            {
                var moveOperation = Screen.MoveMainWindowTo(displays[0], new Vector2Int(displays[0].width / 2, displays[0].height / 2));
                yield return moveOperation;
            }
        }

        #region Stats
        int i_spidersKilled = 0;
        int i_mitesKilled = 0;
        int i_slugsKilled = 0;
        int i_waspsKilled = 0;

        int i_triggerSwaps = 0;
        int i_clipSwaps = 0;
        int i_barrelSwaps = 0;

        int i_roomsCleared = 0;
        int i_explosionCount = 0;
        float f_damageTaken = 0;
        float f_healthRegained = 0;
        int i_dodges = 0;
        int i_bulletsFired = 0;
        int i_bulletsHit = 0;
        int i_environmentDestroyed = 0;
        float f_runStartTime = 0;
        float f_runEndTime = 0;
        float f_averageTimePerRoom = 0;

        //set stats
        public static void IncrementSpiderKill()
        {
            gameManager.i_spidersKilled += 1;
        }
        public static void IncrementMiteKill()
        {
            gameManager.i_mitesKilled += 1;
        }
        public static void IncrementSlugKill()
        {
            gameManager.i_slugsKilled += 1;
        }
        public static void IncrementWaspKill()
        {
            gameManager.i_waspsKilled += 1;
        }
        public static void IncrementTriggerSwap()
        {
            gameManager.i_triggerSwaps += 1;
        }
        public static void IncrementClipSwap()
        {
            gameManager.i_clipSwaps += 1;
        }
        public static void IncrementBarrelSwap()
        {
            gameManager.i_barrelSwaps += 1;
        }
        public static void IncrementRoomsCleared()
        {
            gameManager.i_roomsCleared += 1;
            InGameUI.UpdateRoomCountText();
        }
        public static void IncrementExplosionCount()
        {
            gameManager.i_explosionCount += 1;
        }
        public static void IncrementBulletsFired()
        {
            gameManager.i_bulletsFired += 1;
        }
        public static void IncrementBulletsHit()
        {
            gameManager.i_bulletsHit += 1;
        }
        public static void IncrementEnvironmentDestroyed()
        {
            gameManager.i_environmentDestroyed += 1;
        }
        public static void IncrementDamageTaken(float damage)
        {
            gameManager.f_damageTaken += damage;
        }
        public static void IncrementDamageHealed(float heal)
        {
            gameManager.f_healthRegained += heal;
        }
        public static void IncrementDodges()
        {
            gameManager.i_dodges += 1;
        }

        public static void SetStopTime()
        {
            gameManager.f_runEndTime = Time.time;
        }
        public static void SetStartTime()
        {
            gameManager.f_runStartTime = Time.time;
        }

        void ResetAllStats()
        {
            gameManager.i_spidersKilled = 0;
            gameManager.i_mitesKilled = 0;
            gameManager.i_slugsKilled = 0;
            gameManager.i_waspsKilled = 0;
            gameManager.i_triggerSwaps = 0;
            gameManager.i_clipSwaps = 0;
            gameManager.i_barrelSwaps = 0;
            gameManager.i_roomsCleared = 0;
            gameManager.i_explosionCount = 0;
            gameManager.f_damageTaken = 0;
            gameManager.f_healthRegained = 0;
            gameManager.i_dodges = 0;
            gameManager.i_bulletsFired = 0;
            gameManager.i_bulletsHit = 0;
            gameManager.i_environmentDestroyed = 0;
            gameManager.f_runStartTime = 0;
            gameManager.f_runEndTime = 0;
            gameManager.f_averageTimePerRoom = 0;
        }

        //get stats
        public static int GetSpidersKilled()
        {
            return gameManager.i_spidersKilled;
        }
        public static int GetMitesKilled()
        {
            return gameManager.i_mitesKilled;
        }
        public static int GetSlugsKilled()
        {
            return gameManager.i_slugsKilled;
        }
        public static int GetWaspsKilled()
        {
            return gameManager.i_waspsKilled;
        }
        public static int GetTriggerSwaps()
        {
            return gameManager.i_triggerSwaps;
        }
        public static int GetClipSwaps()
        {
            return gameManager.i_clipSwaps;
        }
        public static int GetBarrelSwaps()
        {
            return gameManager.i_barrelSwaps;
        }
        public static int GetRoomsCleared()
        {
            return gameManager.i_roomsCleared;
        }
        public static int GetExplosionCount()
        {
            return gameManager.i_explosionCount;
        }
        public static float GetDamageTaken()
        {
            return gameManager.f_damageTaken;
        }
        public static float GetHealthRegained()
        {
            return gameManager.f_healthRegained;
        }
        public static int GetDodgeCount()
        {
            return gameManager.i_dodges;
        }
        public static int GetBulletsFired()
        {
            return gameManager.i_bulletsFired;
        }
        public static int GetBulletsHit()
        {
            return gameManager.i_bulletsHit;
        }
        public static float GetHitRate()
        {
            return ((float)gameManager.i_bulletsHit / (float)gameManager.i_bulletsFired) * 100f;
        }
        public static int GetEnvironmentDestroyed()
        {
            return gameManager.i_environmentDestroyed;
        }
        public static float GetTimeTaken()
        {
            return gameManager.f_runEndTime - gameManager.f_runStartTime;
        }
        public static float GetAverageTimePerRoom()
        {
            return (GetTimeTaken() / (float)gameManager.i_roomsCleared);
        }
        #endregion
    }
}