using UnityEngine;
using Utility;
using Managers;
using System.Collections;

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

    private bool b_cameraShaking;
    private Vector3 S_shakeOffset = Vector3.zero;

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
        if (!GameManager.gameManager.b_cull)
        {
            C_camera.cullingMatrix = Matrix4x4.Ortho(-99999, 99999, -99999, 99999, 0.001f, 99999) *
                                Matrix4x4.Translate(Vector3.forward * -99999 / 2f) *
                                C_camera.worldToCameraMatrix;
            C_camera.cullingMask = 0b0111111111111111111111111111111;
        }
        else
        {
            ResetFrustumCulling();
        }
        if (PauseMenu.pauseMenu.b_gamePaused)
        {
            return;
        }
        if (C_targetPlayer != null)
        {
            S_playerPosition = C_targetPlayer.transform.position;
            S_playerLookDirection = C_targetPlayer.S_cameraDirection;

            Vector3 lookOffset = S_playerPosition + (S_playerLookDirection * f_lookSrength);
            Vector3 nextCameraPos = b_cameraShaking ? S_offsetVector + lookOffset + S_shakeOffset : S_offsetVector + lookOffset;

            S_currentFocus = transform.position - S_offsetVector;
            S_nextFocus = lookOffset;
            if (b_cameraShaking)
            {
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime / 100.0f);
            }
            else if (Vector3.Distance(S_nextFocus, S_currentFocus) > f_focusRadius)
            {
                C_camera.transform.position = Vector3.SmoothDamp(C_camera.transform.position, nextCameraPos, ref S_velocity, f_smoothTime);
            }
            else
            {
                S_velocity = Vector3.MoveTowards(S_velocity, Vector3.zero, Time.deltaTime / 10.0f);
            }

        }
        GameManager.gameManager.b_cullLastFrame = GameManager.gameManager.b_cull;
    }

    void ResetFrustumCulling()
    {
        C_camera.ResetCullingMatrix();
        C_camera.cullingMask = i_originalCullingMask;
    }

    public void ShakeCamera(float shakeIntensity, float shakeTime)
    {
        StartCoroutine(ShakeCameraRoutine(shakeIntensity, shakeTime));
    }

    private IEnumerator ShakeCameraRoutine(float shakeIntensity, float shakeTime)
    {
        b_cameraShaking = true;
        int frameCounter = 0;
        float startTime = Time.time;
        float randomXStart = Random.Range(-shakeIntensity, shakeIntensity);
        float randomZStart = Random.Range(-shakeIntensity, shakeIntensity);
        S_shakeOffset = Vector3.zero;
        while (Time.time - startTime < shakeTime)
        {
            if(frameCounter % 30 == 0)
            {
                float xCopy = randomXStart;
                randomXStart = randomZStart;
                randomZStart = xCopy;
            }
            randomXStart = -randomXStart;
            randomZStart = -randomZStart;
            float timePercentage = (Time.time - startTime) / shakeTime;
            S_shakeOffset.x = randomXStart = Mathf.MoveTowards(S_shakeOffset.x, randomXStart * (1 - timePercentage), (shakeIntensity / 2.0f) * Time.deltaTime);
            S_shakeOffset.z = randomZStart = Mathf.MoveTowards(S_shakeOffset.z, randomZStart * (1 - timePercentage), (shakeIntensity / 2.0f) * Time.deltaTime);
            yield return 0;
            frameCounter++;
        }
        b_cameraShaking = false;
    }

}
