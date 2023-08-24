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

    //TO DO MOVE THIS TO THE DOOR SCRIPT
    public enum RewardType
    {
        Trigger,
        Clip,
        Barrel,
        RandomModule,
    }



    [Rename("Rewards Per Room"), SerializeField] private int i_rewardsPerRoom = 1;



    private RewardType e_currentRewardType;
    private Vector3 S_rewardPoint;
    private List<string> ls_allGunModules = new List<string>();
    private List<string> ls_triggerGunModules = new List<string>();
    private List<string> ls_clipGunModules = new List<string>();
    private List<string> ls_barrelGunModules = new List<string>();
    private int i_currentRoom = 0;

    private List<string> ls_allLevels = new List<string>();
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

    }

    private void SpawnNextReward()
    {
        for (int i = 0; i < i_rewardsPerRoom; i++)
        {
            switch (e_currentRewardType)
            {
                case RewardType.Trigger:
                    GunModuleSpawner.SpawnGunModule(ls_triggerGunModules[Random.Range(0, ls_triggerGunModules.Count)], S_rewardPoint);
                    break;
                case RewardType.Clip:
                    GunModuleSpawner.SpawnGunModule(ls_clipGunModules[Random.Range(0, ls_clipGunModules.Count)], S_rewardPoint);
                    break;
                case RewardType.Barrel:
                    GunModuleSpawner.SpawnGunModule(ls_barrelGunModules[Random.Range(0, ls_barrelGunModules.Count)], S_rewardPoint);
                    break;
                case RewardType.RandomModule:
                    GunModuleSpawner.SpawnGunModule(ls_allGunModules[Random.Range(0, ls_allGunModules.Count)], S_rewardPoint);
                    break;
            }
        }
    }

    public void UpdateRewardPoint(Vector3 point)
    {
        S_rewardPoint = point;
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

    public void IncrementRoom()
    {
        i_currentRoom++;
    }

    public void LoadNextRoom()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        GroupLevels(allLevelsInProject);
        for (int i = 0; i < allLevelsInProject.Count(); i++)
        {
            allLevelsInProject[i] = allLevelsInProject[i].Split("\\")[1];
        }
        TextDocumentReadWrite.FileWrite(levelFileDirectory + "\\allLevels.txt", allLevelsInProject.ToArray());
    }

    private void GroupLevels(List<string> levelsList)
    {
        for (int i = 0; i < levelsList.Count(); i++)
        {
            if (levelsList[i].Contains("Easy"))
            {
                ls_easyLevels.Add(levelsList[i].Split("Easy\\")[1]);
            }
            if (levelsList[i].Contains("Medium"))
            {
                ls_mediumLevels.Add(levelsList[i].Split("Medium\\")[1]);
            }
            if (levelsList[i].Contains("Hard"))
            {
                ls_hardLevels.Add(levelsList[i].Split("Hard\\")[1]);
            }
        }
    }

    private void GrabAllLevels()
    {
        string filePath = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Levels\\allLevels.txt";
        ls_allLevels = TextDocumentReadWrite.FileRead(filePath).ToList();
    }
}
