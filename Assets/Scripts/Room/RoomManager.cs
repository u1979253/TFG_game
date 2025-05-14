using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace ggj25
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject roomPrefabLT;
        [SerializeField] private GameObject roomPrefabRT;
        [SerializeField] private GameObject roomPrefabLTR;
        [SerializeField] private GameObject roomPrefabL;
        [SerializeField] private GameObject roomPrefabR;
        [SerializeField] private GameObject roomPrefabT;
        [SerializeField] private GameObject roomPrefabLR;
        [SerializeField] private GameObject roomPrefabB;    // porta de sota
        [SerializeField] private GameObject roomPrefabLTB;
        [SerializeField] private GameObject roomPrefabRTB;
        [SerializeField] private GameObject roomPrefabLTRB;
        [SerializeField] private GameObject roomPrefabLB;
        [SerializeField] private GameObject roomPrefabRB;
        [SerializeField] private GameObject roomPrefabTB;
        [SerializeField] private GameObject roomPrefabLRB;

        [SerializeField] private int maxRooms = 15;
        //[SerializeField] private int minRooms = 8;

        int roomWidth = 30;
        int roomHeight = 20;

        int gridSizeX = 10;
        int gridSizeY = 10;

        private List<GameObject> roomObjects = new List<GameObject>();

        private Queue<Vector2Int> roomQueue = new Queue<Vector2Int>();

        private int[,] roomGrid;

        private int roomCount = 0;

        private bool generationComplete = false;

        private void Start()
        {
            roomGrid = new int[gridSizeX, gridSizeY];
            roomQueue = new Queue<Vector2Int>();

            Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
            StartRoomGenerationFromRoom(initialRoomIndex);
        }

        private void Update()
        {
            if (roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
            {
                Vector2Int roomIndex = roomQueue.Dequeue();
                int gridX = roomIndex.x;
                int gridY = roomIndex.y;


                TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
                TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
                TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
                TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
            }
            else if (!generationComplete)
            {

                generationComplete = true;
                Debug.Log($"Generation complete: {roomCount} rooms.");
                InstantiateAllRooms();
            }
        }


        private int CountAdjacentRooms(Vector2Int roomIndex)
        {

            int x = roomIndex.x;
            int y = roomIndex.y;
            int count = 0;
            if (x > 0 && roomGrid[x - 1, y] != 0) count++; // Vei esquerra
            if (x < gridSizeX - 1 && roomGrid[x + 1, y] != 0) count++; // Vei dreta
            if (y > 0 && roomGrid[x, y - 1] != 0) count++; // Vei abaix
            if (y < gridSizeY - 1 && roomGrid[x, y + 1] != 0) count++; // Vei amunt

            return count;
        }

        private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
        {
            roomQueue.Clear();
            roomQueue.Enqueue(roomIndex);

            int x = roomIndex.x; int y = roomIndex.y;
            roomGrid[x, y] = 1;
            roomCount++;

        }

        private bool TryGenerateRoom(Vector2Int roomIndex)
        {
            int x = roomIndex.x;
            int y = roomIndex.y;

            // 1. Está dentro de la grilla?
            if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
                return false;

            // 2. ¿Ya existe una sala ahí?
            if (roomGrid[x, y] != 0)
                return false;

            // 3. ¿Hemos llegado al máximo de salas?
            if (roomCount >= maxRooms)
                return false;

            // 4. Aleatorio para ramificar (salta un 50% de las veces, excepto la primera)
            if (roomQueue.Count > 0 && Random.value < 0.5f)
                return false;

            // 5. No queremos más de 1 sala adyacente (evita corredores anchos)
            if (CountAdjacentRooms(roomIndex) > 1)
                return false;

            // --- PASA TODOS LOS FILTROS: generamos aquí ---
            roomQueue.Enqueue(roomIndex);
            roomGrid[x, y] = 1;    // marca la casilla como ocupada
            roomCount++;           // aumenta el contador

            return true;
        }



        private void InstantiateAllRooms()
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (roomGrid[x, y] == 1)
                    {
                        Vector2Int idx = new Vector2Int(x, y);
                        GameObject prefab = GetRoomPrefabFor(idx);
                        Vector3 pos = GetPositionFromGridIndex(idx);
                        GameObject room = Instantiate(prefab, pos, Quaternion.identity);
                        room.name = $"Room-{x}_{y}";
                        roomObjects.Add(room);
                    }
                }
            }
        }


        private GameObject GetRoomPrefabFor(Vector2Int roomIndex)
        {
            int x = roomIndex.x;
            int y = roomIndex.y;

            bool hasLeft = x > 0 && roomGrid[x - 1, y] != 0;
            bool hasRight = x < gridSizeX - 1 && roomGrid[x + 1, y] != 0;
            bool hasTop = y < gridSizeY - 1 && roomGrid[x, y + 1] != 0;
            bool hasBottom = y > 0 && roomGrid[x, y - 1] != 0;

            // 4 conexiones
            if (hasLeft && hasTop && hasRight && hasBottom) return roomPrefabLTRB;

            // 3 conexiones
            if (hasLeft && hasTop && hasRight) return roomPrefabLTR;
            if (hasLeft && hasTop && hasBottom) return roomPrefabLTB;
            if (hasRight && hasTop && hasBottom) return roomPrefabRTB;
            if (hasLeft && hasRight && hasBottom) return roomPrefabLRB;

            // 2 conexiones
            if (hasLeft && hasTop) return roomPrefabLT;
            if (hasRight && hasTop) return roomPrefabRT;
            if (hasLeft && hasBottom) return roomPrefabLB;
            if (hasRight && hasBottom) return roomPrefabRB;
            if (hasLeft && hasRight) return roomPrefabLR;
            if (hasTop && hasBottom) return roomPrefabTB;

            // 1 conexión
            if (hasLeft) return roomPrefabL;
            if (hasRight) return roomPrefabR;
            if (hasTop) return roomPrefabT;
            if (hasBottom) return roomPrefabB;

            // Ninguna conexión
            return roomPrefabT;
        }



        private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
        {

            int gridX = gridIndex.x;
            int gridY = gridIndex.y;
            return new Vector3(roomWidth * (gridX - gridSizeX / 2),
               roomHeight * (gridY - gridSizeY / 2));
        }



        private void OnDrawGizmos()
        {
            Color gizmosColor = new Color(0, 1, 1, 0.05f);
            Gizmos.color = gizmosColor;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(position, new Vector3(roomWidth, roomHeight, 1f));
                }
            }
        }
    }
}