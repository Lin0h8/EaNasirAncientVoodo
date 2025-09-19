using System.Data;
using UnityEngine;

public class DungeonGenerationScript : MonoBehaviour
{
    public GameObject[] walls;
    public GameObject[] doors;
    public bool[] testStatus;
    int[,] Map = new int[1000, 1000];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateRoom(testStatus);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void UpdateRoom(bool[] status)
    {
        for (int y = 0; y < Map.GetLength(1); y++)
        {
            for (int x = 0; x < Map.GetLength(0); x++)
            {

            }
        }
    }
}
