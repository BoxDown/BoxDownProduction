using System.Collections.Generic;
using UnityEngine;
using Gun;
using System.Linq;
using Utility;
using System.IO;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
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
    private List<string> ls_allGunModules = new List<string>();
    private List<string> ls_triggerGunModules = new List<string>();
    private List<string> ls_clipGunModules = new List<string>();
    private List<string> ls_barrelGunModules = new List<string>();
    private int i_currentRoom = 0;

    private List<string> ls_allLevels = new List<string>(); // all levels should not be used to load any scenes
    private List<string> ls_easyLevels = new List<string>();
    private List<string> ls_mediumLevels = new List<string>();
    private List<string> ls_hardLevels = new List<string>();



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
        GrabAllLevels();
        GroupLevels(ls_allLevels);

    }

    public void SpawnNextReward()
    {
        for (int i = 0; i < i_rewardsPerRoom; i++)
        {
            switch (e_currentRewardType)
            {
                case Door.RoomType.Trigger:
                    GunModuleSpawner.SpawnGunModule(ls_triggerGunModules[Random.Range(0, ls_triggerGunModules.Count)], S_rewardPoint);
                    break;
                case Door.RoomType.Clip:
                    GunModuleSpawner.SpawnGunModule(ls_clipGunModules[Random.Range(0, ls_clipGunModules.Count)], S_rewardPoint);
                    break;
                case Door.RoomType.Barrel:
                    GunModuleSpawner.SpawnGunModule(ls_barrelGunModules[Random.Range(0, ls_barrelGunModules.Count)], S_rewardPoint);
                    break;
                case Door.RoomType.RandomModule:
                    GunModuleSpawner.SpawnGunModule(ls_allGunModules[Random.Range(0, ls_allGunModules.Count)], S_rewardPoint);
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
        ls_allGunModules = GunModuleSpawner.GetAllGunModules().ToList();
    }

    private void GroupGunModules()
    {
        for (int i = 0; i < ls_allGunModules.Count; i++)
        {
            if (ls_allGunModules[i].Contains("Trigger"))
            {
                ls_triggerGunModules.Add(ls_allGunModules[i]);
            }
            else if (ls_allGunModules[i].Contains("Clip"))
            {
                ls_clipGunModules.Add(ls_allGunModules[i]);
            }
            else if (ls_allGunModules[i].Contains("Barrel"))
            {
                ls_barrelGunModules.Add(ls_allGunModules[i]);
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
        IncrementRoom();
        LoadNextRoom();
    }


    private void LoadNextRoom()
    {
        int roomNumberToLoad = 0;
        if (i_currentRoom < i_easyRooms)
        {
            while (ls_easyLevels[roomNumberToLoad] != SceneManager.GetActiveScene().name)
            {
                roomNumberToLoad = Random.Range(0, ls_easyLevels.Count());
            }
            SceneManager.LoadScene(ls_easyLevels[roomNumberToLoad]);
            return;
        }
        else if (i_currentRoom < i_mediumRooms)
        {
            while (ls_mediumLevels[roomNumberToLoad] != SceneManager.GetActiveScene().name)
            {
                roomNumberToLoad = Random.Range(0, ls_mediumLevels.Count());
            }
            SceneManager.LoadScene(ls_mediumLevels[roomNumberToLoad]);
            return;
        }
        else if (b_endlessMode || i_currentRoom < i_hardRooms)
        {
            while (ls_hardLevels[roomNumberToLoad] != SceneManager.GetActiveScene().name)
            {
                roomNumberToLoad = Random.Range(0, ls_hardLevels.Count());
            }
            SceneManager.LoadScene(ls_hardLevels[roomNumberToLoad]);
            return;
        }
        else
        {
            //TO DO ACTUALLY REPLACE THIS WITH THE SCENE NAME WITH THE BOSS
            //load boss scene after reaching the specified end
            SceneManager.LoadScene("BossSceneOrWhatever");
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
}
