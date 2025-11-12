using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;

    private void Awake()
    {
        cam = FindFirstObjectByType<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 direction = cam.transform.position - transform.position;

        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
}
