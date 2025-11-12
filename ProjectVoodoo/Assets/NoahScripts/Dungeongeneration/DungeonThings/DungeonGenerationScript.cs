
using UnityEngine;
using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Unity.Mathematics;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;



//using static UnityEditor.PlayerSettings;
public class DungeonGenerationScript : MonoBehaviour
{
   

    public Loadingscreenscript loadingscreen;
    void Start()
    {
        
        loadingscreen.Show();
        CreateDungeon();
        StartCoroutine(DrawDungeon()); 
        








    }
    IEnumerator DrawDungeon()
    {
        
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            DrawRoom(i, dungeonRooms[i].x, dungeonRooms[i].y);
        }
        List<Doors> availableDoors = new List<Doors>();
        int iterationCounter = 0;
        for (int o = 0; o < roomObjects.Count; o++)
        {
            for (int p = 0; p < roomObjects.Count; p++)
            {
                availableDoors.Clear();
                if (o != p)
                {
                    List<GameObject> go1 = roomObjects[o];
                    List<GameObject> go2 = roomObjects[p];
                    for (int i = 0; i < go1.Count; i++)
                    {
                        GameObject g1 = go1[i];
                        for (int j = 0; j < go2.Count; j++)
                        {

                            GameObject g2 = go2[j];
                            for (int d = 0; d < g1.transform.childCount; d++)
                            {
                                if (g1.transform.GetChild(d) != g1.transform.Find("Roof") && g1.transform.GetChild(d) != g1.transform.Find("Breaker"))
                                {


                                    Transform door1 = g1.transform.GetChild(d).Find("Door").transform;
                                    Transform door2;
                                    switch (g1.transform.GetChild(d).name)
                                    {
                                        case "NorthernWall":
                                            door2 = g2.transform.Find("SouthernWall").Find("Door").transform;
                                            break;
                                        case "SouthernWall":
                                            door2 = g2.transform.Find("NorthernWall").Find("Door").transform;
                                            break;
                                        case "EasternWall":
                                            door2 = g2.transform.Find("WesternWall").Find("Door").transform;
                                            break;
                                        case "WesternWall":
                                            door2 = g2.transform.Find("EasternWall").Find("Door").transform;
                                            break;
                                        default:
                                            door2 = g2.transform;
                                            break;

                                    }


                                    //if (door1.position.x  +1+ door1.GetComponent<Renderer>().bounds.extents.x > door2.position.x &&
                                    //    door2.position.x + door2.GetComponent<Renderer>().bounds.extents.x > door1.position.x &&
                                    //    door1.position.y +1 + door1.GetComponent<Renderer>().bounds.extents.y > door2.position.y &&
                                    //    door2.position.y + door2.GetComponent<Renderer>().bounds.extents.y > door1.position.y)
                                    Bounds b1 = door1.GetComponent<Renderer>().bounds;
                                    Bounds b2 = door2.GetComponent<Renderer>().bounds;
                                    b1.Expand(0.01f);

                                    if (b1.Intersects(b2))
                                    {
                                        Doors aDoor = new Doors();
                                        aDoor.g1 = door1.gameObject;
                                        aDoor.g2 = door2.gameObject;
                                        availableDoors.Add(aDoor);
                                    }
                                }
                                iterationCounter++;
                                if (iterationCounter % 1000 == 0)
                                    yield return null;
                            }
                            
                        }
                        
                    }
                    if (availableDoors.Count > 0)
                    {
                        Doors doorsToRemove = availableDoors[UnityEngine.Random.Range(0, availableDoors.Count)];
                        doorsToRemove.g1.SetActive(false);
                        doorsToRemove.g2.SetActive(false);
                        doorsToRemove.g1.GetComponent<MeshCollider>().gameObject.SetActive(false);
                        doorsToRemove.g2.GetComponent<MeshCollider>().gameObject.SetActive(false);
                    }

                }
                if ((p % 5) == 0)
                {
                    yield return null;
                }
                    
            }

            float progress = (float)o / roomObjects.Count;
            loadingscreen.UpdateProgress(progress);
            yield return null;

        }



        yield return null; 

        
        GetComponent<NavMeshSurface>().BuildNavMesh();

        
        loadingscreen.UpdateProgress(1f);
        yield return null;


        loadingscreen.Hide();
        
        Player.transform.position = new Vector3(roomObjects[0][roomObjects[0].Count / 2].transform.GetComponent<Renderer>().bounds.center.x,
        roomObjects[0][roomObjects[0].Count / 2].transform.position.y + Player.transform.GetComponent<Renderer>().bounds.extents.y,
        roomObjects[0][roomObjects[0].Count / 2].transform.GetComponent<Renderer>().bounds.center.z);
        isDone = true;
        Debug.Log("Dungeon generated successfully");
        for (int j = 1; j< roomObjects.Count; j++)
        {
            for (int i = 0; i < roomObjects[roomObjects.Count - j].Count; i++)
            {
                if (roomObjects[roomObjects.Count - j][i].transform.Find("EasternWall").gameObject.activeInHierarchy && roomObjects[roomObjects.Count - j][i].transform.Find("EasternWall").transform.Find("Door").gameObject.activeInHierarchy)
                {
                    roomObjects[roomObjects.Count - j][i].transform.Find("Breaker").gameObject.SetActive(true);
                    j = roomObjects.Count;
                    break;
                }
            }
        }
        
    }
    struct Doors
    {
        public GameObject g1;
        public GameObject g2;
    }
    #region Bös
    [Header("Dungeon Settings")]
    public int width = 50;
    public int height = 50;
    public int roomCount = 10;
    public int roomMaxSize = 10;
    public int roomMinSize = 5;
    public Room[,] DungeonMap;
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject[] roomPrefab;
    public GameObject wallPrefab;
    public GameObject[] Enemies;
    public GameObject Player;
    public GameObject corridorTilePrefab;
    public GameObject[,] DungeonRoomMap;
    public float GridSize;
    private List<Room> dungeonRooms = new List<Room>();
    public float roomDistance;
    public Transform OldWall;
    public GameObject OldRoom;
    Vector3 DoorPosition = Vector3.zero;
    Vector3 DoorDirection = Vector3.zero;
    public List<List<GameObject>> roomObjects = new List<List<GameObject>>();
    public bool isDone = false;
   
    #endregion
    void CreateDungeon()
    {
        width *= (7);
        height *= (7);
        DungeonMap = new Room[width, height];

        for (int i = 0; i < DungeonMap.GetLength(0); i++)
        {
            for (int j = 0; j < DungeonMap.GetLength(1); j++)
            {
                DungeonMap[i, j] = new Room();
            }
        }
        for (int i = 0; i < roomCount; i++)
        {
            
            Room newRoom = new Room();
           
            newRoom.GeneratePrefab();
            
            if (dungeonRooms.Count == 0)
            {
                int startX = DungeonMap.GetLength(0) / 2 - newRoom.GetRoomSizes().Width / 2;
                int startY = DungeonMap.GetLength(1) / 2 - newRoom.GetRoomSizes().Depth / 2;
                //newRoom.GeneratePrefab();
                if (CanRoomBePlaced(newRoom,startX, startY))
                {
                    PlaceRoom(newRoom, startX, startY);
                }
                

            }
            else
            {
                Room baseRoom = dungeonRooms[UnityEngine.Random.Range(0, dungeonRooms.Count)];
                
                string direction = new[] { "N", "S", "E", "W" }[UnityEngine.Random.Range(0, 4)];

                int newX = 0;
                int newY = 0;
                int doorX = 0;
                int doorY = 0;

                switch (direction)
                {
                    case "N":
                        newX = baseRoom.x;
                        newY = baseRoom.y - newRoom.GetRoomSizes().Depth;
                        doorX = newX;
                        doorY = newY;
                        break;
                    case "S":
                        newX = baseRoom.x;
                        newY = baseRoom.y + baseRoom.GetRoomSizes().Depth;
                        doorX = newX;
                        doorY = newY;
                        break;
                    case "E":
                        newX = baseRoom.x + baseRoom.GetRoomSizes().Width;
                        newY = baseRoom.y;
                        doorX = newX;
                        doorY = newY;
                        break;
                    case "W":
                        newX = baseRoom.x - newRoom.GetRoomSizes().Width;
                        newY = baseRoom.y;
                        doorX = newX;
                        doorY = newY;
                        break;

                }

                //newRoom.GeneratePrefab();
                if (CanRoomBePlaced(newRoom, newX, newY))
                {
                    PlaceRoom(newRoom, newX, newY);
                }
                else
                {
      
                    i--;
                }
            }
        }
        //SetDoors();
       
        //PrintMapToConsole(DungeonMap);
    }
   
    void PlaceRoom(Room room, int posX, int posY)
    {
        {
            for (int i = 0; i < room.roomTiles.GetLength(0); i++)
            {
                for (int j = 0; j < room.roomTiles.GetLength(1); j++)
                {
                    
                    int mapX = i + posX;
                    int mapY = j + posY;

                    int shouldBeEnemySpawn = UnityEngine.Random.Range(0, 11);
                    DungeonMap[mapX, mapY].isActive = 1;
                    if (shouldBeEnemySpawn == 6 || shouldBeEnemySpawn == 7)
                    {
                        room.ShouldSpawnEnemy = true;
                    }
                    
                    
                        
                    
                        

                }
            }
            room.x = posX;
            room.y = posY;

            dungeonRooms.Add(room);

        }
    }
    void DrawRoom(int q, int posX, int posY)
    {
        Room room = dungeonRooms[q];
        List<GameObject> roomCollection = new List<GameObject>();
            for (int i = 0; i < room.roomTiles.GetLength(0); i++)
            {
                for (int j = 0; j < room.roomTiles.GetLength(1); j++)
                {
                    int mapX = i + posX;
                    int mapY = j + posY;
                    GameObject floor = Instantiate(roomPrefab[0]);
                    
                    floor.GetComponent<Room>().enabled = false;
                    Renderer[] renderers = floor.transform.GetComponentsInChildren<Renderer>();
                    float width = 0;
                    float depth = 0;

                    Bounds combinedBounds = renderers[0].bounds;
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        combinedBounds.Encapsulate(renderers[r].bounds);
                    }
                    width += combinedBounds.size.x;
                    depth += combinedBounds.size.z;

                    floor.transform.position = new Vector3(mapX * width, 0, mapY * depth);
                
                    for (int Y = -1; Y <= 1; Y++)
                    {
                        Vector2 curPos = new Vector2(i, j);
                        Vector2 modPos = new Vector2(curPos.x, curPos.y + Y);
                        if (modPos.y >= 0 && modPos.y <= room.roomTiles.GetLength(1)-1)
                        {

                            
                            if (room.roomTiles[(int)modPos.x, (int)modPos.y] == 1 || room.roomTiles[(int)modPos.x, (int)modPos.y] == 2)
                            {

                                switch (Y)
                                {
                                    case -1:

                                        floor.transform.Find("SouthernWall").gameObject.SetActive(false);
                                        break;
                                    case 1:
                                        floor.transform.Find("NorthernWall").gameObject.SetActive(false);
                                        break;

                                }



                            }
                        
                    }
                    }
                    for (int X = -1; X <= 1; X++)
                    
                    {
                        Vector2 curPos = new Vector2(i, j);
                        Vector2 modPos = new Vector2(curPos.x + X, curPos.y);
                        if (modPos.x >= 0 && modPos.x <= room.roomTiles.GetLength(0) - 1)
                        {


                            if (room.roomTiles[(int)modPos.x, (int)modPos.y] == 1 || room.roomTiles[(int)modPos.x, (int)modPos.y] == 2)
                            {
                                //Transform nRoom = roomTIles[(int)modPos.x, (int)modPos.y].transform.Find("walls").transform;
                                switch (X)
                                {
                                    case -1:
                                        floor.transform.Find("WesternWall").gameObject.SetActive(false);

                                        break;
                                    case 1:
                                        floor.transform.Find("EasternWall").gameObject.SetActive(false);

                                        break;




                                }
                            }
                        }
                    
                }
                    roomCollection.Add(floor);
                    floor.transform.SetParent(transform);
                    
                }

            }
            if (room.ShouldSpawnEnemy)
        {
            Debug.Log("EnemyShouldSpanw");
            int r = UnityEngine.Random.Range(0, Enemies.Length);
            int s = UnityEngine.Random.Range(0, roomCollection.Count);
            GameObject g = Instantiate(Enemies[r]);
            g.transform.SetParent(null);
            
            Vector3 center = roomCollection[s].transform.GetComponent<Renderer>().bounds.center;
            g.transform.position =  new Vector3(center.x, roomCollection[s].transform.position.y + g.transform.GetComponent<Renderer>().bounds.extents.y, center.z);

        }
        
        roomObjects.Add(roomCollection);

       

       





    }
    bool CanRoomBePlaced(Room room, int posX, int posY)
    {
        for (int i = 0; i < room.roomTiles.GetLength(0); i++)
        {
            for (int j = 0; j < room.roomTiles.GetLength(1); j++)
            {
                int mapX = i + posX;
                int mapY = j + posY;
                if (mapX < 0 || mapY < 0 || mapX > DungeonMap.GetLength(0) || mapY > DungeonMap.GetLength(1))
                {
                    return false;
                }
                if (DungeonMap[mapX, mapY].isActive == 1)
                {
                    return false;
                }
            }
        }
        return true;
    }
 
}
