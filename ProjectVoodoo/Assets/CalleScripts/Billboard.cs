using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera cam;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(cam.transform.position);
    }
}
