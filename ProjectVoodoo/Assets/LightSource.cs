using UnityEngine;

public class LightSource : MonoBehaviour
{
    public Transform player;
    public Material material;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            material.SetVector("_PlayerPos", player.position);
        }
        

    }
}
