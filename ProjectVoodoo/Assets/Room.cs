using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Room : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public enum RoomTypes
    {
        Corridor,
        Room,
        smallRoom,
        BigRoom
    }
    
    
        public int isActive = 0;
        public int[,] roomTiles;

        public Vector2 originTile = new Vector2(5, 5);
        public RoomTypes roomType;
    public int x;
    public int y;
    public int DoorX;
    public int DoorY;
    public int NewDoorX;
    public int NewDoorY;
        public Transform[] o;
        [SerializeField] public List<GameObject> TileObjects = new List<GameObject>();
        
        public void GeneratePrefab()
        {
            roomType = (Room.RoomTypes)Random.Range(0, Room.RoomTypes.GetNames(typeof(Room.RoomTypes)).Length);
            Debug.Log(roomType.ToString());
            switch (roomType)
            {
                case RoomTypes.Corridor:
                    int Length = Random.Range(1, 5);
                    roomTiles = new int[1, Length];
                    for (int i = 0; i < Length; i++)
                    {
                        roomTiles[0, i] = 1;
                    }
                    break;

                case RoomTypes.smallRoom:
                    int WidthSR = Random.Range(1, 3);
                    int LengthSR = Random.Range(1, 3);
                    roomTiles = new int[WidthSR, LengthSR];
                    for (int i = 0; i < LengthSR; i++)
                    {
                        for (int j = 0; j < WidthSR; j++)
                        {
                            roomTiles[j, i] = 1;
                        }
                    }
                    break;

                case RoomTypes.Room:
                    int WidthR = Random.Range(3, 5);
                    int LengthR = Random.Range(3, 5);
                    roomTiles = new int[WidthR, LengthR];
                    for (int i = 0; i < LengthR; i++)
                    {
                        for (int j = 0; j < WidthR; j++)
                        {
                            roomTiles[j, i] = 1;
                        }
                    }
                    break;


                case RoomTypes.BigRoom:
                    int WidthBR = Random.Range(5, 7);
                    int LengthBR = Random.Range(5, 7);
                    roomTiles = new int[WidthBR, LengthBR];
                    for (int i = 0; i < LengthBR; i++)
                    {
                        for (int j = 0; j < WidthBR; j++)
                        {
                            roomTiles[j, i] = 1;
                        }
                    }
                    break;

            }

           
        
    }

    public (int Width, int Depth) GetRoomSizes()
    {
        
        return (roomTiles.GetLength(0), roomTiles.GetLength(1));
    } 
    public int GetOriginTile()
    {
        int ot = Random.Range(0, roomTiles.GetLength(0));
        return ot;
    }
   public Room[,] SetActive(Room[,] map, Vector2 pos)
    {
        for (int y = (int)pos.y; y < GetRoomSizes().Depth; y++)
        {
            for (int x = (int)pos.x; x < GetRoomSizes().Width; x++)
            {
                map[x, y].isActive = 1;
            }
        }
        return map;
    }
    public Vector2 GetDoorPos()
    {
        List<Vector2> possibleDoorPositions = new List<Vector2>();
        Debug.Log(roomTiles.GetLength(0));
        for (int y = 0; y <= roomTiles.GetLength(1); y++)
        {
            for (int x = 0; x <= roomTiles.GetLength(0); x++)
            {

                if (x == roomTiles.GetLength(0) || x == 0)
                {
                    Debug.Log(new Vector2(x, y));
                    possibleDoorPositions.Add(new Vector2(x, y));
                    Debug.Log(new Vector2(x, y));
                }
                if (y == roomTiles.GetLength(1)-1 || y == 1)
                {
                    possibleDoorPositions.Add(new Vector2(x, y));
                    Debug.Log(new Vector2(x, y));
                }
            }
        }
        Debug.Log(possibleDoorPositions.Count);
        Vector2 doorPos = possibleDoorPositions[Random.Range(0, possibleDoorPositions.Count)];
        return doorPos;
    }
    void Start()
    {
        GeneratePrefab();
                
        Debug.Log(roomTiles);
    }

    // Update is called once per frame
   
}
