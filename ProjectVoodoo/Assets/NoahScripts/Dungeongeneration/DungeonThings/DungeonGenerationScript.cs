//using System;
//using System.Data;
//using UnityEngine;

//public class DungeonGenerationScript : MonoBehaviour
//{
//    public GameObject[] walls;
//    public GameObject[] doors;
//    public GameObject[] rooms;
//    public GameObject tile;
//    public bool[] testStatus;
//    System.Random _rng = new System.Random();
//    int[,] Map = new int[1000, 1000];
//    Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        UpdateRoom(testStatus);
//    }

//    Update is called once per frame
//    void Update()
//    {

//    }
//    void UpdateRoom(bool[] status)
//    {
//        for (int i = 0; i < Map.GetLength(1); i++)
//        {
//            for (int x = 0; x < Map.GetLength(0); x++)
//            {
//                int ShouldExist = _rng.Next(0, 1000);
//                if (ShouldExist >= 50 && ShouldExist <= 60)
//                {
//                    Map[x, i] = 1;
//                }
//            }
//        }
//        for (int i = 0; i < Map.GetLength(1); i++)
//        {
//            for (int x = 0; x < Map.GetLength(0); x++)
//            {

//                if (Map[x, i] == 1)
//                {
//                    Debug.Log(x);
//                    GameObject tile2 = new GameObject();
//                    Plane m = tile2.transform.GetComponent<Plane>();
//                    tile2 = tile;
//                    tile2.transform.position = new Vector3(x * m..x, 0, i * m.bounds.size.z);
//                }
//            }
//        }
//    }
//}
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
//using static UnityEditor.PlayerSettings;

public class DungeonGenerationScript : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int width = 50;
    public int height = 50;
    public int roomCount = 10;
    public int roomMaxSize = 10;
    public int roomMinSize = 5;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject[] roomPrefab;
    public GameObject wallPrefab;
    public GameObject corridorTilePrefab;
    
    public float GridSize;
    private List<GameObject> dungeonRooms = new List<GameObject>();
    public float roomDistance;

    void Start()
    {
        
        GenerateDungeon();
        
        updateDungeonRoomPositions();
        
    }

    void GenerateDungeon()
    {
        
        for (int i = 0; i <  roomCount; i++)
        {
            
            GameObject room = Instantiate(roomPrefab[Random.Range(0, roomPrefab.Length)]);
           
            GridSize = (room.transform.transform.GetComponent<Renderer>().bounds.extents.x * 2);
            float x = Random.Range(0, width);
            float y = Random.Range(0, height);
            Vector3 pos = new Vector3(x * room.transform.localScale.x, 0, y * room.transform.localScale.z);
            float rotationY = Random.Range(1, 4);
            pos = SnapToGrid(pos, GridSize);
            room.transform.position = pos;

            room.transform.rotation = Quaternion.Euler(new Vector3(-90, rotationY * 90,0 ));
            
            dungeonRooms.Add(room);
            UpdateWalls(room.transform);
        }
        

    }
    void setCorridors(Transform t)
    {
        
    }
    void updateDungeonRoomPositions()
    {
       
        for (int k = 0; k < 5; k++)
        {


            for (int i = 0; i < roomCount; i++)
            {
                for (int j = 0; j < roomCount; j++)
                {
                    GameObject r1 = dungeonRooms[i];
                    GameObject r2 = dungeonRooms[j];
                    //Debug.Log(dungeonRooms[i], dungeonRooms[j]);
                    if (r1 != r2)
                    {
                        Renderer r1M = r1.GetComponentInChildren<Renderer>();
                        Renderer r2M = r2.GetComponentInChildren<Renderer>();
                        //Rect mainRoom = new Rect(r1.transform.localScale.x, r1.transform.localScale.z, r1M.bounds.Intersects)
                        float meshDistance = Vector3.Distance(r1M.bounds.center, r2M.bounds.center);
                        float overlapZone = r1M.bounds.extents.magnitude + r2M.bounds.extents.magnitude + roomDistance;
                        if (meshDistance < overlapZone)
                        {
                            
                            Vector3 deltaCenter = (r1.transform.position - r2.transform.position).normalized;
                            if (deltaCenter == Vector3.zero)
                            {
                                deltaCenter = Random.insideUnitCircle;
                            }
                            float distance = Random.Range(20, 100);
                            deltaCenter *= distance;
                            r1.transform.position -= deltaCenter;
                            r2.transform.position += deltaCenter;
                            r1.transform.position = SnapToGrid(r1.transform.position, GridSize);
                            r2.transform.position = SnapToGrid(r2.transform.position, GridSize);
                            i = 0;
                            j = 0;
                        }
                    }

                }
            } 
        }
           
        
        
    }
    
    void UpdateWalls(Transform t)
    {
        int walls = 0;
        for (int i = 0; i< t.childCount; i++)
        {
            for (int j = 0; j< t.GetChild(i).transform.childCount; j++)
            {
                Debug.Log(t.GetChild(i).transform.GetChild(j).tag);
                
                if (t.GetChild(i).transform.GetChild(j).CompareTag("Wall"))
                {
                    Debug.Log(t.GetChild(i).transform.GetChild(j).tag);
                    
                    
                    
                    
                }

            }
            int AmountOfwallsToDoorify = Random.Range(1, walls);
            for (int j = 0; j < AmountOfwallsToDoorify; j++)
            {
                int walltoDoorify = Random.Range(0, t.Find("walls").transform.childCount);
                if (t.Find("walls").transform.GetChild(walltoDoorify).gameObject)
                {
                    t.Find("walls").transform.GetChild(walltoDoorify).gameObject.SetActive(false);
                    int corridorTiles = Random.Range(1, 6);
                    for (int q = 0; q < corridorTiles; q++)
                    {

                        GameObject c = Instantiate(corridorTilePrefab);
                        Vector3 pos = t.Find("walls").transform.GetChild(walltoDoorify).transform.position;
                        
                        pos += t.Find("walls").transform.GetChild(walltoDoorify).transform.forward * c.transform.GetComponent<Renderer>().bounds.extents.x * 2 * q;
                        
                        pos.y = t.Find("walls").transform.GetChild(walltoDoorify).transform.position.y;
                        
                        c.transform.position = pos;

                    }
                }
                
               
            }
        }
    }
    Vector3 SnapToGrid( Vector3 position, float gridSize)
    {
        Vector3 snapPos = Vector3.zero;
        snapPos.x = Mathf.Round(position.x * gridSize) / gridSize;
        snapPos.z = Mathf.Round(position.z * gridSize) / gridSize;
        return snapPos;
    }
}
