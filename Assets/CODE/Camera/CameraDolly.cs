using UnityEngine;
using Utility;
using Managers;

[RequireComponent(typeof(Camera))]
public class CameraDolly : MonoBehaviour
{
    [Header("Camera Target")]
    [Rename("Target Player")] private PlayerController C_targetPlayer = null;

    [Header("Modifier Varaibles")]
    [Rename("Look Strength")] public float f_lookSrength;
    [Rename("Focus Radius")] public float f_focusRadius;
    [Rename("Offset")] public Vector3 S_offsetVector;


    private Camera C_camera;
    private Vector3 S_playerPosition;
    private Vector3 S_playerLookDirection;


    // Start is called before the first frame update
    void Start()
    {
        C_camera = GetComponent<Camera>();
        C_targetPlayer = FindObjectOfType<PlayerController>();
        GameManager.SetCamera(this);
        DontDestroyOnLoad(transform.parent);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (C_targetPlayer != null)
        {
            S_playerPosition = C_targetPlayer.transform.position;
            S_playerLookDirection = C_targetPlayer.GetRotationDirection();
            RaycastHit hitInfo;
            Physics.Raycast(C_camera.transform.position, C_camera.transform.forward, out hitInfo, S_offsetVector.y * 1.2f);
            Vector3 cameraCenterPos = new Vector3(hitInfo.point.x, S_playerPosition.y, hitInfo.point.z);

            Vector3 lookOffset = S_playerLookDirection * f_lookSrength;
            Vector3 nextCameraPos = (S_playerPosition + S_offsetVector) + lookOffset;

            C_camera.transform.position = Vector3.Lerp(C_camera.transform.position, nextCameraPos, ExtraMaths.Map(0, f_lookSrength, 0,1, Vector3.Distance(S_playerPosition, lookOffset)));
        }
    }
}
