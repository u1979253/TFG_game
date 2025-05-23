using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
        [SerializeField] private GameObject bossRoomPrefab;

        [SerializeField] private GameObject room2x1Prefab;

        [SerializeField] private int maxRooms = 15;
        //[SerializeField] private int minRooms = 8;

        int roomWidth = 30;
        int roomHeight = 20;

        int gridSizeX = 10;
        int gridSizeY = 10;
        [SerializeField] private Vector2Int forcedRoom = new Vector2Int(5, 5);
        private List<GameObject> roomObjects = new List<GameObject>();

        private Vector2Int? bossRoomIndex = null;
        private Vector2Int? belowBossIndex = null;

        private Queue<Vector2Int> roomQueue = new Queue<Vector2Int>();

        private HashSet<Vector2Int> _skipCells;
        private Dictionary<Vector2Int, GameObject> _multiCellRooms;

        private int[,] roomGrid;

        private int roomCount = 0;

        private int roomNumber = 1;

        private bool generationComplete = false;

        public static Dictionary<Vector2Int, RoomController> RoomLookup;

        private HashSet<Vector2Int> forcedTopDoor = new HashSet<Vector2Int>();

        [SerializeField] private float bossRoomYOffset = 1.5f;

        [Header("Enemy Spawns")]
        [SerializeField] private List<GameObject> _enemyPrefabs;
        [SerializeField] private int _minEnemiesPerRoom = 1;
        [SerializeField] private int _maxEnemiesPerRoom = 4;


        private void Start()
        {
            roomGrid = new int[gridSizeX, gridSizeY];
            roomQueue = new Queue<Vector2Int>();

            Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
            //StartRoomGenerationFromRoom(initialRoomIndex);
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
                if (GameManager.Instance?.LevelManager != null)
                    GameManager.Instance.LevelManager.Init();
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
            if (IsInsideGrid(forcedRoom))
            {
                roomGrid[forcedRoom.x, forcedRoom.y] = 1;
                roomCount++;
                roomQueue.Enqueue(forcedRoom);
            }

            int x = roomIndex.x; int y = roomIndex.y;
            roomGrid[x, y] = 1;
            roomCount++;

        }

        private bool IsInsideGrid(Vector2Int idx)
        {
            return idx.x >= 0 && idx.x < gridSizeX && idx.y >= 0 && idx.y < gridSizeY;
        }

        private bool TryGenerateRoom(Vector2Int roomIndex)
        {
            int x = roomIndex.x;
            int y = roomIndex.y;

            // 1. Est� dentro de la grilla?
            if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
                return false;

            // 2. �Ya existe una sala ah�?
            if (roomGrid[x, y] != 0)
                return false;

            // 3. �Hemos llegado al m�ximo de salas?
            if (roomCount >= maxRooms)
                return false;

            // 4. Aleatorio para ramificar (salta un 50% de las veces, excepto la primera)
            if (roomQueue.Count > 0 && Random.value < 0.5f)
                return false;

            // 5. No queremos m�s de 1 sala adyacente (evita corredores anchos)
            if (CountAdjacentRooms(roomIndex) > 1)
                return false;

            // --- PASA TODOS LOS FILTROS: generamos aqu� ---
            roomQueue.Enqueue(roomIndex);
            roomGrid[x, y] = 1;    // marca la casilla como ocupada
            roomCount++;           // aumenta el contador

            return true;
        }

        public void StartGeneration()
        {
            // limpia estado
            generationComplete = false;
            roomCount = 0;
            roomNumber = 1;
            roomGrid = new int[gridSizeX, gridSizeY];
            roomQueue.Clear();
            // inyecta la primera sala
            var center = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
            StartRoomGenerationFromRoom(center);
        }


        private void InstantiateAllRooms()
        {
            PlaceBossRoom();  // no lo tocamos

            RoomLookup = new Dictionary<Vector2Int, RoomController>();
            _skipCells = new HashSet<Vector2Int>();
            _multiCellRooms = new Dictionary<Vector2Int, GameObject>();

            // 1) Detectar pares 2�1 elegibles
            //    Recorremos toda la grilla salvo la �ltima columna
            for (int x = 0; x < gridSizeX - 1; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    var a = new Vector2Int(x, y);
                    var b = new Vector2Int(x + 1, y);

                    // 1. Ambos deben existir
                    if (roomGrid[x, y] != 1 || roomGrid[x + 1, y] != 1)
                        continue;

                    // 2. Debe haber sala a la izquierda de 'a' y a la derecha de 'b'
                    bool hasLeft = x > 0 && roomGrid[x - 1, y] == 1;
                    bool hasRight = (x + 1) < gridSizeX - 1 && roomGrid[x + 2, y] == 1;
                    if (!hasLeft || !hasRight)
                        continue;

                    // 3. Ninguno de los dos puede tener un vecino arriba o abajo
                    bool aVert = (y > 0 && roomGrid[x, y - 1] == 1)
                              || (y < gridSizeY - 1 && roomGrid[x, y + 1] == 1);
                    bool bVert = (y > 0 && roomGrid[x + 1, y - 1] == 1)
                              || (y < gridSizeY - 1 && roomGrid[x + 1, y + 1] == 1);
                    if (aVert || bVert)
                        continue;

                    // � Si pasamos todos los filtros, marcamos el par �
                    _skipCells.Add(b);                 // saltamos la segunda casilla
                    _multiCellRooms[a] = room2x1Prefab; // en la primera instanciamos el 2�1
                }
            }

            // 2) Instanciaci�n real
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    var idx = new Vector2Int(x, y);
                    if (roomGrid[x, y] != 1) continue;   // no hay sala
                    if (_skipCells.Contains(idx)) continue;   // parte alta de un par 2�1

                    // Decide qu� prefab usar
                    GameObject prefab;
                    Vector3 spawnPos = GetPositionFromGridIndex(idx);

                    if (bossRoomIndex.HasValue && bossRoomIndex.Value == idx)
                    {
                        prefab = bossRoomPrefab;
                        spawnPos = spawnPos + Vector3.up * bossRoomYOffset;
                    }
                    else if (_multiCellRooms.TryGetValue(idx, out var multi))
                    {
                        // Cabecera del par 2�1
                        prefab = multi;
                        spawnPos = spawnPos + new Vector3(4.7f, 1f, 0f);
                    }
                    else if (belowBossIndex.HasValue && belowBossIndex.Value == idx)
                    {
                        prefab = GetRoomPrefabFor(idx, forceTopDoor: true);
                    }
                    else
                    {
                        prefab = GetRoomPrefabFor(idx, forceTopDoor: false);
                    }

                    // Instancia y registra
                    var roomGO = Instantiate(prefab, spawnPos, Quaternion.identity, this.transform);
                    roomGO.name = (bossRoomIndex == idx) ? "BossRoom" : $"Room-{roomNumber++}";

                    var ctrl = roomGO.GetComponent<RoomController>();
                    ctrl.RoomIndex = idx;
                    RoomLookup[idx] = ctrl;
                    if (_multiCellRooms.ContainsKey(idx))
                    {
                        var rightCell = idx + Vector2Int.right;
                        RoomLookup[rightCell] = ctrl;
                    }
                }
            }

            // 3) Enemies y notificaciones
            SpawnAllEnemies();
            GameManager.Instance.LevelManager.Init();
        }

        private void PlaceBossRoom()
        {
            var startIdx = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
            var dist = new Dictionary<Vector2Int, int> { [startIdx] = 0 };
            var q = new Queue<Vector2Int>();
            q.Enqueue(startIdx);

            while (q.Count > 0)
            {
                var u = q.Dequeue();
                int d = dist[u];
                foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var v = u + dir;
                    if (v.x >= 0 && v.x < gridSizeX &&
                        v.y >= 0 && v.y < gridSizeY &&
                        roomGrid[v.x, v.y] == 1 &&
                        !dist.ContainsKey(v))
                    {
                        dist[v] = d + 1;
                        q.Enqueue(v);
                    }
                }
            }

            // 2) Filtra candidatos: un �nico vecino y vecino arriba (para boss s�lo puerta abajo)
            var candidates = new List<(Vector2Int idx, int distance)>();
            foreach (var kv in dist)
            {
                var idx = kv.Key;
                bool hasBottom = idx.y > 0 && roomGrid[idx.x, idx.y - 1] == 1;
                int neighs = CountAdjacentRooms(idx);
                if (hasBottom && neighs == 1)
                    candidates.Add((idx, kv.Value));
            }

            // 3) Elige la m�s lejana
            if (candidates.Count > 0)
            {
                var bossIdx = candidates.OrderByDescending(c => c.distance).First().idx;
                bossRoomIndex = bossIdx;
                belowBossIndex = bossIdx + Vector2Int.down;  // el vecino bajo
            }
            else
            {
                Debug.LogWarning("No se encontr� ninguna sala adecuada para colocar al boss.");
            }
        }


        private GameObject GetRoomPrefabFor(Vector2Int roomIndex, bool forceTopDoor = false)
        {
            int x = roomIndex.x;
            int y = roomIndex.y;

            bool hasLeft = x > 0 && roomGrid[x - 1, y] != 0;
            bool hasRight = x < gridSizeX - 1 && roomGrid[x + 1, y] != 0;
            bool hasTop = y < gridSizeY - 1 && roomGrid[x, y + 1] != 0;
            bool hasBottom = y > 0 && roomGrid[x, y - 1] != 0;

            // Aplicar fuerza si se solicita
            if (forceTopDoor)
                hasTop = true;

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

            // 1 conexi�n
            if (hasLeft) return roomPrefabL;
            if (hasRight) return roomPrefabR;
            if (hasTop) return roomPrefabT;
            if (hasBottom) return roomPrefabB;

            // Ninguna conexi�n
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

        private void SpawnAllEnemies()
        {
            foreach (var kv in RoomLookup)
            {
                var room = kv.Value;
                room.SpawnEnemies(_enemyPrefabs, _minEnemiesPerRoom, _maxEnemiesPerRoom);
            }
        }
    }
}