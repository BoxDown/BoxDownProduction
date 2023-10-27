using UnityEngine;
using Utility;
using Managers;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class CameraDolly : MonoBehaviour
{
    [Header("Camera Target")]
    [Rename("Target Player")] private PlayerController C_targetPlayer = null;

    [Header("Modifier Varaibles")]
    [Rename("Look Strength")] public float f_lookSrength;
    [Rename("Smooth Time")] public float f_smoothTime = 0.2f;
    [Rename("Focus Radius")] public float f_focusRadius;
    [Rename("Offset")] public Vector3 S_offsetVector;



    [HideInInspector] public Camera C_camera;
    private Vector3 S_playerPosition;
    private Vector3 S_playerLookDirection;
    private Vector3 S_velocity = Vector3.zero;

    private Vector3 S_currentFocus;
    private Vector3 S_nextFocus;

    private int i_currentActiveCameraShakes = 0;
    private Vector3 S_shakeOffset = Vector3.zero;
    private List<Vector3> lS_shakeOffsets = new List<Vector3>();
    private bool b_shaking
    {
        get { return S_shakeOffset == Vector3.zero ? false : true; }
    }

    int i_originalCullingMask;


    // Start is called before the first frame update
    void Start()
    {
        C_camera = GetComponent<Camera>();
        C_targetPlayer = FindObjectOfType<PlayerController>();
        GameManager.SetCamera(this);
        DontDestroyOnLoad(transform.parent);
        i_originalCullingMask = C_camera.cullingMask;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (PauseMenu.pauseMenu.b_gamePaused)
        {
            return;
        }
        if (C_targetPlayer != null)
        {
            S_playerPosition = C_targetPlayer.transform.position;
            S_playerLookDirection = C_targetPlayer.S_cameraDirection;

            if (i_currentActiveCameraShakes > 0)
            {
                float highestMagnitude = -1;

                foreach (Vector3 offset in lS_shakeOffsets)
                {
                    if(offset.magnitude > highestMagnitude)
                    {
                        continue;
                    }
                    highestMagnitude = offset.magnitude;
                    S_shakeOffset = offset;
                }
            }

            Vector3 lookOffset = S_playerPosition + (S_playerLookDirection * f_lookSrength);
            Vector3 nextCameraPos = b_shaking ? S_offsetVector + lookOffset + S_shakeOffset : S_offsetVector + lookOffset;

            S_currentFocus = transform.position - S_offsetVector;
            S_nextFocus = lookOffset;
            if (b_shaking)
            {
                MoveCameraShakeTowardsZero();
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime / 10.0f);
                GameManager.gameManager.b_cullLastFrame = GameManager.gameManager.b_cull;
                return;
            }
            else if (Vector3.Distance(S_nextFocus, S_currentFocus) > f_focusRadius)
            {
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime);
                S_shakeOffset = Vector3.zero;
            }
            else
            {
                S_velocity = Vector3.MoveTowards(S_velocity, Vector3.zero, Time.deltaTime / 10.0f);
                S_shakeOffset = Vector3.zero;
            }

        }
        GameManager.gameManager.b_cullLastFrame = GameManager.gameManager.b_cull;
    }

    public void ShakeCamera(float shakeIntensity)
    {
        if(S_shakeOffset.magnitude > shakeIntensity)
        {
            return;
        }

        if(S_shakeOffset != Vector3.zero)
        {
            S_shakeOffset += S_shakeOffset.normalized * shakeIntensity;
            S_shakeOffset = Vector3.ClampMagnitude(S_shakeOffset, shakeIntensity);
        }
        else
        {
            float randomXStart = Random.Range(-shakeIntensity, shakeIntensity);
            float randomZStart = Random.Range(-shakeIntensity, shakeIntensity);
            S_shakeOffset = new Vector3(randomXStart, 0, randomZStart);
        }
    }

    public void MoveCameraShakeTowardsZero()
    {
        if (Time.frameCount % 3 == 0)
        {
            float xCopy = S_shakeOffset.x;
            S_shakeOffset.x = S_shakeOffset.z;
            S_shakeOffset.z = xCopy;
        }
        S_shakeOffset.x = -S_shakeOffset.x;
        S_shakeOffset.z = -S_shakeOffset.z;

        S_shakeOffset = Vector3.MoveTowards(S_shakeOffset, Vector3.zero, Time.deltaTime);
    }

    public void SetCameraFocus(Vector3 position)
    {
        transform.position = position + S_offsetVector;
    }
}
