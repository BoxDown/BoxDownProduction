using System.Collections.Generic;
using UnityEngine;
using Gun;
using System.Linq;
using Utility;
using System.IO;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public enum UIState
        {
            Main,
            Pause,
            Swap,
            InGame,
            Options,
            Credits
        }
        public static UIState e_currentUIState
        {
            get;
            private set;
        }

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



        private Door.RoomType e_currentRewardType;
        private Vector3 S_rewardPoint;
        private List<string> ls_allGunModulesNames = new List<string>();
        private List<string> ls_triggerGunModulesNames = new List<string>();
        private List<string> ls_clipGunModulesNames = new List<string>();
        private List<string> ls_barrelGunModulesNames = new List<string>();

        private List<GunModule> lC_allGunModules = new List<GunModule>();
        private List<GunModule> lC_triggerGunModules = new List<GunModule>();
        private List<GunModule> lC_clipGunModules = new List<GunModule>();
        private List<GunModule> lC_barrelGunModules = new List<GunModule>();
        private int i_currentRoom = 0;

        private List<string> ls_allLevels = new List<string>(); // all levels should not be used to load any scenes
        private List<string> ls_easyLevels = new List<string>();
        private List<string> ls_mediumLevels = new List<string>();
        private List<string> ls_hardLevels = new List<string>();


        #region GamePlayFunctons
        // Start is called before the first frame update
        void Awake()
        {
            DontDestroyOnLoad(this);
            if (gameManager != null && gameManager != this)
            {
                Destroy(this);
            }
            else
            {
                gameManager = this;
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
                        return;
                    case Door.RoomType.Clip:
                        GetRandomModule(e_currentRewardType);
                        return;
                    case Door.RoomType.Barrel:
                        GetRandomModule(e_currentRewardType);
                        return;
                    case Door.RoomType.RandomModule:
                        GetRandomModule(e_currentRewardType);
                        return;
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
            ls_allGunModulesNames = GunModuleSpawner.GetAllGunModules().ToList();
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

        private void IncrementRoom()
        {
            i_currentRoom++;
        }

        //what doors call
        public void MoveToNextRoom()
        {
            LoadNextRoom();
        }


        private void LoadNextRoom()
        {
            int roomNumberToLoad = 0;
            if (i_currentRoom < i_easyRooms)
            {
                while (ls_easyLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
                }
                Debug.Log("Moved To Easy Room");
                IncrementRoom();
                SceneManager.LoadScene(ls_easyLevels[roomNumberToLoad]);
                return;
            }
            else if (i_currentRoom < i_easyRooms + 1 + i_mediumRooms)
            {
                if (i_currentRoom == i_easyRooms + 1)
                {
                    SceneManager.LoadScene("EasyBreakRoom");
                    return;
                }
                while (ls_mediumLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_mediumLevels.Count());
                }
                Debug.Log("Moved To Medium Room");
                IncrementRoom();
                SceneManager.LoadScene(ls_mediumLevels[roomNumberToLoad]);
                return;
            }
            else if (b_endlessMode || i_currentRoom < i_easyRooms + 1 + i_mediumRooms + 1 + i_hardRooms + 1)
            {
                if (i_currentRoom == i_easyRooms + 1 + i_mediumRooms + 1)
                {
                    SceneManager.LoadScene("MediumBreakRoom");
                    return;
                }
                while (ls_hardLevels[roomNumberToLoad] == SceneManager.GetActiveScene().name)
                {
                    roomNumberToLoad = Random.Range(0, ls_hardLevels.Count());
                }
                Debug.Log("Moved To Hard Room");
                IncrementRoom();
                SceneManager.LoadScene(ls_hardLevels[roomNumberToLoad]);
                return;
            }
            else if (i_currentRoom == i_easyRooms + 1 + i_mediumRooms + 1 + i_hardRooms)
            {
                SceneManager.LoadScene("HardBreakRoom");
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

            TextDocumentReadWrite.FileWrite(levelFileDirectory + "\\allLevels.txt", allLevelsInProject.ToArray());
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
            string filePath = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Levels\\allLevels.txt";
            ls_allLevels = TextDocumentReadWrite.FileRead(filePath).ToList();
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
                            GunModuleSpawner.SpawnGunModule(ls_triggerGunModulesNames[i], S_rewardPoint);
                            Debug.Log($"{ls_triggerGunModulesNames[i]} module spawned, this module had a {(Mathf.Clamp(lC_triggerGunModules[i].f_moduleStrength, 1, 100) / totalChance) * 100}% chance to spawn");
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
                            GunModuleSpawner.SpawnGunModule(ls_clipGunModulesNames[i], S_rewardPoint);
                            Debug.Log($"{ls_clipGunModulesNames[i]} module spawned, this module had a {(Mathf.Clamp(lC_clipGunModules[i].f_moduleStrength, 1, 100) / totalChance) * 100}% chance to spawn");
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
                            GunModuleSpawner.SpawnGunModule(ls_barrelGunModulesNames[i], S_rewardPoint);
                            Debug.Log($"{ls_barrelGunModulesNames[i]} module spawned, this module had a {(Mathf.Clamp(lC_barrelGunModules[i].f_moduleStrength, 1, 100) / totalChance) * 100}% chance to spawn");
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
                            GunModuleSpawner.SpawnGunModule(ls_allGunModulesNames[i], S_rewardPoint);
                            Debug.Log($"{ls_allGunModulesNames[i]} module spawned, this module had a {(Mathf.Clamp(lC_allGunModules[i].f_moduleStrength, 1, 100) / totalChance) * 100}% chance to spawn");
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
            SceneManager.LoadScene("StartBreakRoom");
        }
        public static void StartGameEndless()
        {
            gameManager.b_endlessMode = true;
            SceneManager.LoadScene("StartBreakRoom");
        }

        public static void OpenOptionsMenu()
        {
            OptionsMenu.Activate();
        }
        public static void BackToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public static void ExitGame()
        {
            Application.Quit();
        }
        #endregion
    }
}