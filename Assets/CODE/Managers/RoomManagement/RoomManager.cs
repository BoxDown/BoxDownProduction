using Enemy;
using Utility;
using System.Collections;
using UnityEngine;
using System;
using GunkManager;

namespace Managers
{
    [Serializable]
    public class EnemyWave
    {
        public EnemyBase[] aC_enemies;
    }
    public class RoomManager : MonoBehaviour
    {
        public enum EntranceDirection
        {
            North,
            South,
            East,
            West
        }

        [SerializeField] public EnemyWave[] aC_enemyWaveList;
        [Rename("Reward Position")] public Vector3 S_rewardPosition;
        [Rename("Entrance Direction")] public EntranceDirection e_entranceDirection;
        [Rename("Spawn Position")] public Vector3 S_spawnPosition;
        [Rename("Enemy Wave Delay (Seconds)")] public float f_spawnDelayTime;
        [HideInInspector] public PolyBrushManager C_manager;
        private bool b_endTriggered = false;

        private int i_currentWave = 0;
        private Door[] aC_doorsInLevel;

        private int i_currentEnemiesLeft = 0;

        private void Start()
        {
            GameManager.gameManager.UpdateRewardPoint(S_rewardPosition);
            PlayerController player = FindObjectOfType<PlayerController>();
            C_manager = GetComponent<PolyBrushManager>();
            //C_manager.InstanceMaterial();
            FindObjectOfType<CameraDolly>().SetCameraFocus(S_spawnPosition);
            player.SetPlayerPosition(S_spawnPosition);
            switch (e_entranceDirection)
            {
                case EntranceDirection.North:
                    player.SetPlayerRotation(180);

                    break;
                case EntranceDirection.South:
                    player.SetPlayerRotation(0);

                    break;
                case EntranceDirection.East:
                    player.SetPlayerRotation(90);

                    break;
                case EntranceDirection.West:
                    player.SetPlayerRotation(270);

                    break;
            }
            aC_doorsInLevel = FindObjectsOfType<Door>();
            if (C_manager != null)
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
            i_currentWave = 0;
            StartCoroutine(SpawnNextWave());
            AudioManager.SetBattleMusicHighIntensity();
        }

        private void FixedUpdate()
        {
            UpdateEnemiesAlive();
            if (!CheckWaveDead())
            {
                return;
            }
            if (i_currentWave >= aC_enemyWaveList.Length - 1)
            {
                if (!b_endTriggered)
                {
                    GameManager.IncrementRoomsCleared();
                    SpawnReward();
                    UnlockAllDoors();
                    if (C_manager != null)
                    {
                        C_manager.RemoveSpaceGoo();
                        AudioManager.SetBattleMusicLowIntensity();
                    }
                    b_endTriggered = true;
                }
                return;
            }
            else
            {
                IncrementWave();
                StartCoroutine(SpawnNextWave());
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
                return true;
            }
            if (aC_enemyWaveList[i_currentWave].aC_enemies.Length != 0)
            {
                for (int i = 0; i < aC_enemyWaveList[i_currentWave].aC_enemies.Length; i++)
                {
                    if (!aC_enemyWaveList[i_currentWave].aC_enemies[i].b_isDead)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private IEnumerator SpawnNextWave()
        {
            //enable all enemy objects in next list,
            //on enable spawn them in
            yield return new WaitForSeconds(f_spawnDelayTime);
            if (aC_enemyWaveList.Length != 0 && i_currentWave < aC_enemyWaveList.Length)
            {
                for (int i = 0; i < aC_enemyWaveList[i_currentWave].aC_enemies.Length; i++)
                {
                    aC_enemyWaveList[i_currentWave].aC_enemies[i].gameObject.SetActive(true);
                    aC_enemyWaveList[i_currentWave].aC_enemies[i].Spawn();
                }
                i_currentEnemiesLeft = aC_enemyWaveList[i_currentWave].aC_enemies.Length;
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
        public void LockAllDoors()
        {
            for (int i = 0; i < aC_doorsInLevel.Length; i++)
            {
                aC_doorsInLevel[i].Lock();
            }
        }

        private void IncrementWave()
        {
            i_currentWave++;
        }

        private void UpdateEnemiesAlive()
        {
            float enemyCount = 0;
            if(aC_enemyWaveList.Length == 0)
            {
                InGameUI.UpdateEnemyCountText(enemyCount);
                return;
            }
            else if(aC_enemyWaveList[i_currentWave].aC_enemies.Length == 0)
            {
                InGameUI.UpdateEnemyCountText(enemyCount);
                return;
            }
            for (int i = 0; i < aC_enemyWaveList[i_currentWave].aC_enemies.Length; i++)
            {
                if (!aC_enemyWaveList[i_currentWave].aC_enemies[i].b_isDead)
                {
                    enemyCount++;
                }
            }
            InGameUI.UpdateEnemyCountText(enemyCount);
        }

    }
}