using Enemy;
using Utility;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Managers
{
    [Serializable]
    public class EnemyWave
    {
        public EnemyBase[] aC_enemies;
    }
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] public EnemyWave[] aC_enemyWaveList;
        [Rename("Reward Position")] public Vector3 S_rewardPosition;
        [Rename("Spawn Position")] public Vector3 S_spawnPosition;
        [HideInInspector] public PolyBrushManager C_manager;
        private bool b_endTriggered = false;

        private int i_currentWave = 0;
        private Door[] aC_doorsInLevel;

        private void Start()
        {
            GameManager.gameManager.UpdateRewardPoint(S_rewardPosition);
            FindObjectOfType<PlayerController>().SetPlayerPosition(S_spawnPosition);
            aC_doorsInLevel = FindObjectsOfType<Door>();
            if (C_manager)
            {
                C_manager.ResetGoo();
            }

            for (int i = 0; i < aC_enemyWaveList.Length; i++)
            {
                for (int j = 0; j < aC_enemyWaveList[i].aC_enemies.Length; j++)
                {
                    aC_enemyWaveList[i].aC_enemies[j].gameObject.SetActive(false);
                }
            }
            SpawnNextWave();
        }

        private void FixedUpdate()
        {
            if (!CheckWaveDead())
            {
                return;
            }
            if (i_currentWave > aC_enemyWaveList.Length)
            {
                if (!b_endTriggered)
                {
                    SpawnReward();
                    UnlockAllDoors();
                    if (C_manager)
                    {
                        C_manager.RemoveSpaceGoo();
                    }
                    b_endTriggered = true;
                }
                return;
            }
            else
            {
                IncrementWave();
                SpawnNextWave();
                return;
            }
        }

        private bool CheckWaveDead()
        {
            //if our wave is higher than the count out
            if (aC_enemyWaveList.Length == 0)
            {
                return true;
            }
            if (i_currentWave >= aC_enemyWaveList.Length)
            {
                return false;
            }
            if (aC_enemyWaveList[i_currentWave].aC_enemies.Length != 0)
            {
                for (int i = 0; i < aC_enemyWaveList[i_currentWave].aC_enemies.Length - 1; i++)
                {
                    if (!aC_enemyWaveList[i_currentWave].aC_enemies[i].b_isDead)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void SpawnNextWave()
        {
            //enable all enemy objects in next list,
            //on enable spawn them in
            for (int i = 0; i < aC_enemyWaveList[i_currentWave].aC_enemies.Length; i++)
            {
                aC_enemyWaveList[i_currentWave].aC_enemies[i].gameObject.SetActive(true);
            }
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
}