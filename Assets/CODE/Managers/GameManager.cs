using System.Collections.Generic;
using UnityEngine;
using Gun;
using System.Linq;
using Utility;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
        [Rename("Amount Of Easy Rooms"), SerializeField] private int i_easyRooms = 3;
        [Rename("Amount Of Medium Rooms"), SerializeField] private int i_mediumRooms = 3;
        [Rename("Amount Of Hard Rooms"), SerializeField] private int i_hardRooms = 5;
        [Rename("Endless Mode"), SerializeField] private bool b_endlessMode = false;

        [Rename("Player Input")] private PlayerInput C_playerInput;

        [Space(10)]
        [Rename("Debug Game"), SerializeField] public bool b_debugMode;

        [Rename("All Levels Document")] public TextAsset C_allLevels;
        [Rename("All Modules Document")] public TextAsset C_allGunModules;

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
        public GunModuleUIAnimations C_gunModuleUI
        {
            get;
            private set;
        }


        private void FixedUpdate()
        {
            if (C_playerInput != null)
            {
                ControlManager.ChangeInputDevice(C_playerInput.currentControlScheme);
            }
        }


        #region GamePlayFunctons
        // Start is called before the first frame update
        void Awake()
        {
            if (!b_debugMode)
            {
                StartCoroutine(StartUp());
            }
            else
            {
                Initialise();
            }
        }

        public void SpawnNextReward()
        {
            for (int i = 0; i < i_rewardsPerRoom; i++)
            {
                switch (e_currentRewardType)
                {
                    // For Debug without weapon modules in project COMMENT THESE OUT
                    case Door.RoomType.Trigger:
                        GetRandomModule(e_currentRewardType);
                        break;
                    case Door.RoomType.Clip:
                        GetRandomModule(e_currentRewardType);
                        break;
                    case Door.RoomType.Barrel:
                        GetRandomModule(e_currentRewardType);
                        break;
                    case Door.RoomType.RandomModule:
                        GetRandomModule(e_currentRewardType);
                        break;
                }
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
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                while (ls_easyLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                }
                IncrementRoom();
                SceneManager.LoadScene(ls_easyLevels[roomNumberToLoad]);
                return;
            }
            else if (i_currentRoom < i_easyRooms + 1 + i_mediumRooms)
            {
                if (i_currentRoom == i_easyRooms)
                {
                    SceneManager.LoadScene("EasyBreakRoom");
                    IncrementRoom();
                    return;
                }
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                while (ls_mediumLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_mediumLevels.Count());
                }
                IncrementRoom();
                SceneManager.LoadScene(ls_mediumLevels[roomNumberToLoad]);
                return;
            }
            else if (b_endlessMode || i_currentRoom < i_easyRooms + 1 + i_mediumRooms + 1 + i_hardRooms + 1)
            {
                if (i_currentRoom == i_easyRooms + 1 + i_mediumRooms)
                {
                    SceneManager.LoadScene("MediumBreakRoom");
                    IncrementRoom();
                    return;
                }
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                while (ls_hardLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_hardLevels.Count());
                }
                IncrementRoom();
                SceneManager.LoadScene(ls_hardLevels[roomNumberToLoad]);
                return;
            }
            else if (i_currentRoom == i_easyRooms + 1 + i_mediumRooms + 1 + i_hardRooms)
            {
                SceneManager.LoadScene("HardBreakRoom");
                IncrementRoom();
                return;
            }
            else
            {
                //load boss scene after reaching the specified end
                SceneManager.LoadScene("BossRoom");
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
        #endregion

        #region UIFunctions
        public static void StartGame()
        {
            gameManager.b_endlessMode = false;
            gameManager.i_currentRoom = 0;
            gameManager.e_currentRewardType = Door.RoomType.None;
            DeactivateMainMenu();
            InGameUI.ActivateInGameUI();
            SceneManager.LoadScene("StartBreakRoom");
        }
        public static void StartGameEndless()
        {
            gameManager.b_endlessMode = true;
            gameManager.ResetAllStats();
            gameManager.i_currentRoom = 0;
            gameManager.e_currentRewardType = Door.RoomType.None;
            DeactivateMainMenu();
            InGameUI.ActivateInGameUI();
            SetStartTime();
            SceneManager.LoadScene("StartBreakRoom");
        }
        public static void OpenOptionsMenu()
        {
            OptionsMenu.Activate();
            DeactivateMainMenu();
        }
        public static void OpenCreditsMenu()
        {
            CreditsMenu.Activate();
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
            gameManager.RemovePlayer();
            gameManager.RemoveCamera();
            ResultsUI.DeactivateLose();
            gameManager.ResetAllStats();
            gameManager.i_currentRoom = 0;
            gameManager.e_currentRewardType = Door.RoomType.None;
            SetStartTime();
            SceneManager.LoadScene("StartBreakRoom");
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
            gameManager.RemoveCamera();
            gameManager.RemovePlayer();
            ResultsUI.DeactivateLose();
            InGameUI.DeactivateInGameUI();
            CreditsMenu.Deactivate();
            SceneManager.LoadScene("MainMenu");
            ActivateMainMenu();
            SwitchToUIActions();
        }

        public static void ExitGame()
        {
            Application.Quit();
        }

        private System.Collections.IEnumerator StartUp()
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
            }
            else
            {
                gameManager = this;
            }
            C_playerInput = GetComponent<PlayerInput>();
            C_gunModuleUI = FindObjectOfType<GunModuleUIAnimations>();
            if (b_debugMode)
            {
                FindObjectOfType<PlayerController>().Initialise();
                PauseMenu.DeactivatePause();
                CreditsMenu.Deactivate();
                WeaponsSwapUI.Deactivate();
                ResultsUI.DeactivateLose();
                DeactivateMainMenu();
            }
            else
            {
                CreditsMenu.Deactivate();
                PauseMenu.DeactivatePause();
                InGameUI.DeactivateInGameUI();
                ResultsUI.DeactivateLose();
                WeaponsSwapUI.Deactivate();
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

            i_currentRoom = 0;

        }

        #endregion

        #region PlayerFunctions

        public static void SwitchToUIActions()
        {
            if (gameManager.b_usingUIActions)
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
            gameManager.b_usingUIActions = true;
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
            if (!gameManager.b_usingUIActions)
            {
                return;
            }
            SwitchOffUIActions();
            gameManager.C_playerInput.SwitchCurrentActionMap("PlayerControl");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Enable();
            actionMap.FindAction("Movement").performed += gameManager.C_player.MoveInput;
            actionMap.FindAction("Movement").canceled += gameManager.C_player.StopMove;
            actionMap.FindAction("Rotate").performed += gameManager.C_player.RotationSet;
            actionMap.FindAction("Dodge").performed += gameManager.C_player.Dodge;
            actionMap.FindAction("Interact").performed += gameManager.C_player.Interact;
            actionMap.FindAction("Fire").performed += gameManager.C_player.Fire;
            actionMap.FindAction("Fire").canceled += gameManager.C_player.CancelFire;
            actionMap.FindAction("Reload").performed += gameManager.C_player.Reload;
            actionMap.FindAction("Pause").performed += gameManager.C_player.Pause;
            gameManager.b_usingUIActions = false;
        }
        public static void SwitchOffInGameActions()
        {
            gameManager.C_playerInput.SwitchCurrentActionMap("PlayerControl");
            InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
            actionMap.Disable();
            actionMap.FindAction("Movement").performed -= gameManager.C_player.MoveInput;
            actionMap.FindAction("Movement").canceled -= gameManager.C_player.StopMove;
            actionMap.FindAction("Rotate").performed -= gameManager.C_player.RotationSet;
            actionMap.FindAction("Dodge").performed -= gameManager.C_player.Dodge;
            actionMap.FindAction("Interact").performed -= gameManager.C_player.Interact;
            actionMap.FindAction("Fire").performed -= gameManager.C_player.Fire;
            actionMap.FindAction("Fire").canceled -= gameManager.C_player.CancelFire;
            actionMap.FindAction("Reload").performed -= gameManager.C_player.Reload;
            actionMap.FindAction("Pause").performed -= gameManager.C_player.Pause;
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

        public void RemovePlayer()
        {
            if (C_player != null)
            {
                InputActionMap actionMap = gameManager.C_playerInput.currentActionMap;
                actionMap.FindAction("Pause").performed -= gameManager.C_player.Pause;
                DestroyImmediate(C_player.C_ownedGun.C_bulletPool.gameObject);
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