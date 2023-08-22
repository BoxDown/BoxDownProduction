using Enemy;
using Utility;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Rename("List Of Enemies")] public List<EnemyBase> lC_enemylist = new List<EnemyBase>();
    [HideInInspector] public PolyBrushManager C_manager;
    public bool b_endTriggered = false;
    public bool b_endTriggeredLastFrame = false;

    private void Start()
    {
        if (C_manager)
        {
            C_manager.ResetGoo();

        }
    }

    private void Update()
    {
        if (C_manager)
        {

            foreach (EnemyBase e in lC_enemylist)
            {
                if (!e.b_isDead)
                {
                    b_endTriggered = false;
                    C_manager.ResetGoo();
                    break;
                }
                b_endTriggered = true;
            }
            if (b_endTriggered && !b_endTriggeredLastFrame)
            {
                C_manager.RemoveSpaceGoo();
            }
            b_endTriggeredLastFrame = b_endTriggered;
        }
    }
}


