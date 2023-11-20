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

    [Header("Explosion Shake Variables")]
    [Rename("Explosion Shake Amplitude")] public float f_explosionShakeAmplitude = 2;
    [Rename("Explosion Shake Frequency")] public float f_explosionShakeFrequency = 3;

    [Header("Gun Explosion Shake Variables")]
    [Rename("Gun Explosion Shake Amplitude")] public float f_gunExplosionShakeAmplitude = 1;
    [Rename("Gun Explosion Shake Frequency")] public float f_gunExplosionShakeFrequency = 5;

    [Header("Gunshot Shake Variables")]
    [Rename("Gunshot Shake Amplitude")] public float f_gunshotShakeAmplitude = 0.2f;
    [Rename("Gunshot Shake Frequency")] public float f_gunshotShakeFrequency = 15f;

    [Header("Player Hurt Shake Variables")]
    [Rename("Player Hurt Shake Amplitude")] public float f_playerHurtShakeAmplitude = 0.8f;
    [Rename("Player Hurt Shake Frequency")] public float f_playerHurtShakeFrequency = 6;



    [HideInInspector] public Camera C_camera;
    private Vector3 S_playerPosition;
    private Vector3 S_playerLookDirection;
    private Vector3 S_velocity = Vector3.zero;

    private Vector3 S_currentFocus;
    private Vector3 S_nextFocus;

    private bool b_shaking
    {
        get { return (f_shakeAmplitude == 0 && f_shakeFrequency == 0) ? false : true; }
    }

    private float f_shakeAmplitude = 0;
    private float f_shakeFrequency = 0;

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
            //grab pos + look direction
            S_playerPosition = C_targetPlayer.transform.position;
            S_playerLookDirection = C_targetPlayer.S_cameraDirection;


            //theoretical next camera point
            Vector3 playerLookOffset = S_playerPosition + (S_playerLookDirection * f_lookSrength);
            //actual next camera point
            Vector3 nextCameraPos = b_shaking ? S_offsetVector + playerLookOffset + new Vector3(f_shakeAmplitude * Mathf.Sin(Time.time * f_shakeFrequency), 0, -f_shakeAmplitude * Mathf.Cos(Time.time * f_shakeFrequency)) : S_offsetVector + playerLookOffset;

            float distanceAwayFromCenter = Vector3.Distance(S_nextFocus, S_currentFocus);
            S_currentFocus = transform.position - S_offsetVector;
            S_nextFocus = playerLookOffset;
            if (b_shaking)
            {
                MoveCameraShakeTowardsZero();
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime / 10.0f);
                GameManager.gameManager.b_cullLastFrame = GameManager.gameManager.b_cull;
                return;
            }
            //problem child
            else if (distanceAwayFromCenter > f_focusRadius)
            {
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime - ((distanceAwayFromCenter - f_focusRadius) * Time.deltaTime));
            }
            else
            {
                S_velocity = Vector3.MoveTowards(S_velocity, Vector3.zero, Time.deltaTime / 10.0f);
            }

        }
        GameManager.gameManager.b_cullLastFrame = GameManager.gameManager.b_cull;
    }

    public void ExplosionCameraShake()
    {
        ShakeCamera(f_explosionShakeAmplitude, f_explosionShakeFrequency);
    }
    public void GunExplosionCameraShake()
    {
        ShakeCamera(f_gunExplosionShakeAmplitude, f_gunExplosionShakeFrequency);
    }
    public void GunshotCameraShake()
    {
        ShakeCamera(f_gunshotShakeAmplitude, f_gunshotShakeFrequency);
    }
    public void PlayerHurtCameraShake()
    {
        ShakeCamera(f_playerHurtShakeAmplitude, f_playerHurtShakeFrequency);
    }


    private void ShakeCamera(float shakeAmplitude, float shakeFrequency)
    {
        if (f_shakeAmplitude > shakeAmplitude && f_shakeFrequency > shakeFrequency)
        {
            return;
        }
        if (f_shakeFrequency < shakeFrequency)
        {
            f_shakeFrequency += shakeFrequency;
            f_shakeFrequency = Mathf.Clamp(f_shakeFrequency, 0, shakeFrequency);
        }
        if (f_shakeAmplitude < shakeAmplitude)
        {
            f_shakeAmplitude += shakeAmplitude;
            f_shakeAmplitude = Mathf.Clamp(f_shakeAmplitude, 0, shakeAmplitude);
        }
    }

    public void MoveCameraShakeTowardsZero()
    {
        int negativeFrequency = Time.frameCount % 15 == 0 ? -1 : 1; 
        int negativeAmplitude = Time.frameCount % 30 == 0 ? -1 : 1; 
        if (f_shakeFrequency != 0)
        {
            f_shakeFrequency = Mathf.MoveTowards(f_shakeFrequency, 0, Mathf.Clamp(f_shakeFrequency, 1, f_shakeFrequency) * Time.deltaTime);
        }
        if (f_shakeAmplitude != 0)
        {
            f_shakeAmplitude = Mathf.MoveTowards(f_shakeAmplitude, 0, Mathf.Clamp(f_shakeAmplitude, 1, f_shakeAmplitude) * Time.deltaTime);
        }
    }

    public void SetCameraFocus(Vector3 position)
    {
        transform.position = position + S_offsetVector;
    }
}
