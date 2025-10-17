
using UnityEngine;
using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;



//using static UnityEditor.PlayerSettings;
public class DungeonGenerationScript : MonoBehaviour
{
    void Start()
    {
        CreateDungeon();
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            DrawRoom(dungeonRooms[i], dungeonRooms[i].x, dungeonRooms[i].y);
        }
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
    public GameObject corridorTilePrefab;
    public GameObject[,] DungeonRoomMap;
    public float GridSize;
    private List<Room> dungeonRooms = new List<Room>();
    public float roomDistance;
    public Transform OldWall;
    public GameObject OldRoom;
    Vector3 DoorPosition = Vector3.zero;
    Vector3 DoorDirection = Vector3.zero;

    
    //void PLaceNextRoom(Transform oldWall, GameObject room)
    //{


    //    Transform chosenWall = oldWall;

    //    while (chosenWall == oldWall)
    //    {
    //        chosenWall = room.transform.Find("walls").transform.GetChild(Random.Range(0, room.transform.Find("walls").transform.childCount));
    //        continue;
    //    }

    //    Debug.Log("anchor room updated");

    //    GameObject newRoom = Instantiate(roomPrefab[Random.Range(0, roomPrefab.Length)]);
    //    Transform newRoomWalls = newRoom.transform.Find("walls");
    //    Transform oppositeWall = GetOppositeWall(newRoomWalls, chosenWall.name);

    //    Debug.Log("new room placed and updated");




    //    //Quaternion rotationTomatch = Quaternion.FromToRotation(oppositeFwrd, chosenFwrd);
    //    //Quaternion target = Quaternion.LookRotation(-chosenFwrd, Vector3.up) * Quaternion.Inverse(oppositeWall.localRotation);
    //    //newRoom.transform.rotation =  newRoom.transform.rotation * rotationTomatch ;

    //    Debug.Log(newRoom.transform.position);
    //    //newRoom.transform.position = newRoom.GetComponentInChildren<Renderer>().bounds.center;
    //    // Vector3 offset = newRoom.transform.position - chosenWall.position;
    //    // offset.y = 0;
    //    // offset.x *= MyForward(chosenWall.gameObject).x;
    //    // offset.z *= MyForward(chosenWall.gameObject).z;
    //    // float f = Vector3.Dot(chosenWall.position, oppositeWall.position);
    //    //Vector3 newPos = offset;
    //    // newPos.y = 0;
    //    // newRoom.transform.position = oppositeWall.position - offset;
    //    //Vector3 worldOppositePos = newRoom.transform.TransformPoint(oppositeWall.localPosition);
    //    Vector3 meshWorldCenter = newRoom.GetComponentInChildren<Renderer>().bounds.center;
    //    Vector3 offset = meshWorldCenter - newRoom.transform.position;
    //    //newRoom.transform.position += offset;
    //    Vector3 deltaCenter = OldRoom.transform.GetComponentInChildren<Renderer>().bounds.center - newRoom.transform.GetComponentInChildren<Renderer>().bounds.center;

    //    //deltaCenter /= 2;



    //    Vector3 newPos = chosenWall.position - (oppositeWall.position - (newRoom.transform.position + newRoom.transform.GetComponentInChildren<Renderer>().bounds.center));
    //    Debug.Log((oppositeWall.position - newRoom.transform.position));
    //    newPos -= deltaCenter;


    //    // Compute how far off the mesh center is from the object's transform

    //    //Debug.Log(offset);
    //    //offset.y = 0;
    //    //newPos.y = 0;
    //    //Debug.Log(newPos);


    //    //newRoom.transform.position = newPos;

    //    //OldWall = oppositeWall;
    //    //OldRoom = newRoom;
    //    //dungeonRooms.Add(OldRoom);
    //    //oppositeWall.gameObject.SetActive(false);
    //    //chosenWall.gameObject.SetActive(false);



    //}

    //Vector3 MyForward(GameObject obj)
    //{
    //    Vector3 f = obj.transform.forward;

    //    float dotForwardZ = Vector3.Dot(f, Vector3.forward); // +Z
    //    float dotBackZ = Vector3.Dot(f, Vector3.back);    // -Z
    //    float dotRightX = Vector3.Dot(f, Vector3.right);   // +X
    //    float dotLeftX = Vector3.Dot(f, Vector3.left);    // -X

    //    if (dotForwardZ > 0.9f) return obj.transform.forward;
    //    else if (dotBackZ > 0.9f) return -obj.transform.forward;
    //    else if (dotRightX > 0.9f) return obj.transform.right;
    //    else if (dotLeftX > 0.9f) return -obj.transform.right;
    //    return Vector3.zero;
    //}

    // (bool isTrue,Vector2 anchorPoint) GetAvailableNeighbors(Vector2 curPos, Room room)
    //{
    //    List<Vector2> emptyNeighbors = new List<Vector2>();

    //    for (int y = -1; y <= 1; y++)
    //    {
    //        List<Vector2> availableSpaces = new List<Vector2>();

    //        Vector2 modPos = new Vector2(curPos.x , curPos.y + y);

    //        if (DungeonMap[(int)modPos.x, (int)modPos.y].isActive == 0)
    //        {

    //            for (int i = 0; i < room.GetRoomSizes().Depth; i++)
    //            {
    //                for (int j = 0; j < room.GetRoomSizes().Width; j++)
    //                {
    //                    if (DungeonMap[j, i].isActive == 0)
    //                    {
    //                        availableSpaces.Add(new Vector2(i, j));
    //                        Debug.Log("found empty space at" + new Vector2(i, j));
    //                    }

    //                }
    //            }



    //        }
    //        if (availableSpaces.Count >= room.GetRoomSizes().Width * room.GetRoomSizes().Depth)
    //        {

    //            for (int i = 0; i < room.GetRoomSizes().Depth; i++)
    //            {
    //                for (int j = 0; i < room.GetRoomSizes().Width; j++)
    //                {
    //                    DungeonMap[j, i].isActive = 1;
    //                }
    //            }
    //            return (true, modPos);
    //        }

    //    }
    //    for (int x = -1; x <= 1; x++)
    //        {
    //            List<Vector2> availableSpaces = new List<Vector2>();

    //            Vector2 modPos = new Vector2(curPos.x + x, curPos.y);

    //            if (DungeonMap[(int)modPos.x, (int)modPos.y].isActive == 0)
    //            {

    //                for (int i = 0; i < room.GetRoomSizes().Depth; i++)
    //                {
    //                    for (int j = 0; j < room.GetRoomSizes().Width; j++)
    //                    {
    //                        if (DungeonMap[j, i].isActive == 0)
    //                        {
    //                            availableSpaces.Add(new Vector2(i, j));

    //                        }

    //                    }
    //                }



    //            }
    //            if (availableSpaces.Count >= room.GetRoomSizes().Width * room.GetRoomSizes().Depth)
    //            {

    //                for (int i = 0; i < room.GetRoomSizes().Depth; i++)
    //                {
    //                    for (int j = 0; i < room.GetRoomSizes().Width; j++)
    //                    {
    //                        DungeonMap[j, i].isActive = 1;

    //                    }
    //                }
    //            return (true, modPos);
    //            }

    //        }


    //    return (false, Vector2.zero);
    //}
    //void GenerateDungeon()
    //{
    //    
    //    Vector2 startingRoom = new Vector2(width / 2, height / 2);
    //    GameObject oldRoom = Instantiate(roomPrefab[Random.Range(0, roomPrefab.Length)]);
    //    
    //    DungeonRoomMap = new GameObject[DungeonMap.GetLength(0), DungeonMap.GetLength(1)];
    //    //DungeonMap = DungeonMap[(int)startingRoom.x, (int)startingRoom.y].SetActive(DungeonMap, startingRoom);
    //    Vector2 oldPos = startingRoom;

    //    for (int i = 0; i < roomCount; i++)
    //    {
    //        //Vector2 doorPos = DungeonMap[(int)oldPos.x, (int)oldPos.y].GetDoorPos();
    //        //Debug.Log(DungeonMap[(int)oldPos.x, (int)oldPos.y].roomType);
    //        //if (GetAvailableNeighbors(doorPos, DungeonMap[(int)oldPos.x, (int)oldPos.y]).isTrue)
    //        //{
    //        //    oldPos = GetAvailableNeighbors(doorPos, DungeonMap[(int)oldPos.x, (int)oldPos.y]).anchorPoint;
    //        //    GameObject newRoom = Instantiate(roomPrefab[Random.Range(0, roomPrefab.Length)]);
    //        //    Vector3 newPos = Vector3.zero;
    //        //    Renderer[] renderers = oldRoom.transform.GetComponentsInChildren<Renderer>();
    //        //    int x = (int)oldPos.x;
    //        //    int y = (int)oldPos.y;
    //        //    float width = 0;
    //        //    float depth = 0;






    //        //   




    //        //    newPos.x = x * width;
    //        //    newPos.z = y * depth;

    //        //    Debug.Log(x * height);
    //        //    newRoom.transform.position = newPos;
    //        //    OldRoom = newRoom;
    //        //    DungeonRoomMap[x, y] = newRoom;
    //        //}
    //        GameObject newRoom = Instantiate(roomPrefab[0]);
    //        Renderer[] renderers = oldRoom.GetComponentsInChildren<Renderer>();
    //        Bounds combinedBounds = renderers[0].bounds;
    //        float width = 0;
    //        float depth = 0;
    //        for (int j = 0; j < renderers.Length; j++)
    //        {
    //            combinedBounds.Encapsulate(renderers[i].bounds);
    //        }

    //        width += combinedBounds.size.x;
    //        depth += combinedBounds.size.z;
    //        newRoom.transform.position = new Vector3(width, 0, depth);
    //        //else
    //        //{

    //        //    availableRoomPos = GetAvailableNeighbors(oldPos);
    //        //   if(availableRoomPos.Count > 0)
    //        //    {
    //        //        int roomToChoose = Random.Range(0, availableRoomPos.Count);

    //        //        Debug.Log(roomToChoose);
    //        //        Vector2 newRoomPos = availableRoomPos[roomToChoose];

    //        //        DungeonMap[(int)newRoomPos.x, (int)newRoomPos.y].isActive = 1;
    //        //        Debug.Log(newRoomPos);
    //        //        oldPos = startingRoom;
    //        //        startingRoom = newRoomPos;
    //        //    }
    //        //    else
    //        //    {
    //        //        for (int y = 0; y< DungeonMap.GetLength(1); y++)
    //        //        {
    //        //            for (int x = 0; x< DungeonMap.GetLength(0); x++)
    //        //            {
    //        //                DungeonMap[x, y].isActive = 0;
    //        //            }
    //        //        }
    //        //        GenerateDungeon();
    //        //    }
    //        //}


    //    }

    //    for (int y = 0; y < DungeonMap.GetLength(1); y++)
    //    {
    //        for (int x = 0; x < DungeonMap.GetLength(0); x++)
    //        {
    //            if (DungeonMap[x, y].isActive == 1)
    //            {



    //            }
    //        }
    //    }











    //}
    #endregion
    void CreateDungeon()
    {
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
                Room baseRoom = dungeonRooms[Random.Range(0, dungeonRooms.Count)];
                
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
                        newY = baseRoom.y + newRoom.GetRoomSizes().Depth;
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
                        newX = baseRoom.x- newRoom.GetRoomSizes().Width;
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
                    //if (mapX == posX && mapY == posY)
                    //{
                    //    DungeonMap[mapX, mapY].isActive= 2;
                    //}
                    DungeonMap[mapX, mapY].isActive = 1;

                }
            }
            room.x = posX;
            room.y = posY;

            dungeonRooms.Add(room);
        }
    }
    void DrawRoom(Room room, int posX, int posY)
    {
        
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
                    if (room.roomTiles[(int)curPos.x, (int)curPos.y] == 2)
                    {
                        for (int d = 0; d < room.transform.childCount; d++)
                        {
                            room.transform.GetChild(d).transform.Find("Door").gameObject.SetActive(false);
                        }
                    }
                }


                }
            
            

            
        }

       





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
                if (DungeonMap[mapX, mapY].isActive == 1 || DungeonMap[mapY, mapY].isActive == 2)
                {
                    return false;
                }
            }
        }
        return true;
    }
 
}
