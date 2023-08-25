using Enemy;
using Utility;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Rename("List Of Enemy Waves")] public List<List<EnemyBase>> lC_enemyWaveList = new List<List<EnemyBase>>();
    [Rename("Reward Position")] public Vector3 S_rewardPosition;
    [Rename("Spawn Position")] public Vector3 S_spawnPosition;
    [HideInInspector] public PolyBrushManager C_manager;

    private int i_currentWave = 0;
    private Door[] aC_doorsInLevel;

    private void Start()
    {
        GameManager.gameManager.UpdateRewardPoint(S_rewardPosition);
        FindObjectOfType<PlayerController>().transform.position = S_spawnPosition;
        aC_doorsInLevel = FindObjectsOfType<Door>();
        if (C_manager)
        {
            C_manager.ResetGoo();
        }
    }

    private void FixedUpdate()
    {
        if (!CheckWaveDead())
        {
            return;
        }
        IncrementWave();
        if (i_currentWave > lC_enemyWaveList.Count)
        {
            SpawnReward();
            UnlockAllDoors();
            C_manager.RemoveSpaceGoo();
            return;
        }
        else
        {
            SpawnNextWave();
            return;
        }
    }

    private bool CheckWaveDead()
    {
        //if our wave is higher than the count out
        if (i_currentWave > lC_enemyWaveList.Count)
        {
            return false;
        }
        for (int i = 0; i < lC_enemyWaveList[i_currentWave].Count; i++)
        {
            if (!lC_enemyWaveList[i_currentWave][i].b_isDead)
            {
                return false;
            }
        }
        return true;
    }

    private void SpawnNextWave()
    {
        //enable all enemy objects in next list,
        //on enable spawn them in
    }

    private void SpawnReward()
    {
        GameManager.gameManager.SpawnNextReward();
    }
    private void UnlockAllDoors()
    {
        for (int i = 0; i < aC_doorsInLevel.Length; i++)
        {
            aC_doorsInLevel[i].Unlock();
        }
    }

    private void IncrementWave()
    {
        i_currentWave++;
    }

}






