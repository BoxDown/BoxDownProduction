using UnityEngine;
using Utility;

public class Pickup : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, (Mathf.Sin(Time.time * 3) / 6.0f) + 0.5f, transform.position.z);
        transform.Rotate(new Vector3(0, 90 * Time.deltaTime, 0));
    }
}
