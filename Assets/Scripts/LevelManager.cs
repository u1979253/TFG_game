using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ggj25
{
    using Direction = DoorController.Direction;
    public class LevelManager : MonoBehaviour
    {
        private float PLAYER_ROOM_CHECK_TIME = 1;

        private float _timestamp = 0;
        
        private List<RoomController> _rooms;
        private RoomController _currentRoom;
        private HeroController _hero;
        
        private int _completedRooms = 0;
        private bool _gameWon = false;

        public DustCleaner DustCleaner => _currentRoom?.DustCleaner;


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Init();  // vuelve a recabar RoomControllers y selecciona la sala inicial
        }

        public void Awake()
        { 
            Init();
        }

        public void Init()
        {
            print("entrooo");
            // 1. Recopila todas las salas y al héroe
            _rooms = GameObject.FindObjectsOfType<RoomController>().ToList();
            _hero = GameObject.FindObjectOfType<HeroController>();
            print("numero habitacions:" + _rooms.Count);
            // 2. Detecta en qué sala está el héroe al comenzar, usando OverlapPoint
            Vector2 heroPos = _hero.transform.position;
            int layer = LayerMask.NameToLayer("Room");    // tu layer de detectores
            int mask = 1 << layer;

            Collider2D hit = Physics2D.OverlapPoint(heroPos, mask);
            print("colider" + hit);
            if (hit != null)
            {
                var detector = hit.GetComponent<EnterRoomDetector>();
                if (detector != null)
                {
                    print("room name" + detector._roomController.name + " en " + detector._roomController);
                    SelectRoom(detector._roomController);
                }
            }
            else
            {
                // 3. Fallback: si no encuentra detector, elige la sala "tutorial"
                var tutorial = _rooms
                    .FirstOrDefault(r => r.name
                        .IndexOf("tutori", StringComparison.InvariantCultureIgnoreCase) >= 0);

                if (tutorial != null)
                    SelectRoom(tutorial);
            }

            // 4. Inicializaciones restantes
            _timestamp = PLAYER_ROOM_CHECK_TIME;
            _completedRooms = 0;
            _gameWon = false;
        }

        public void SelectRoom(RoomController roomController)
        {
            if (roomController == _currentRoom)
            {
                return;
            }
            _currentRoom?.SetActive(false);
            _currentRoom = roomController;
            _currentRoom.SetActive(true);
            
        }

        void Update()
        {
            if (_gameWon) return; // Skip updates if game is already won
            
            if (IsCurrentRoomFinished())
            {
                CleanCurrentRoom();
            }
        }

        private void CheckWinCondition()
        {
            if (_currentRoom.name.Equals("BossRoom", StringComparison.OrdinalIgnoreCase)
        && _currentRoom.IsCompleted)
            {
                _gameWon = true;
                GameManager.Instance.GameOver(true);
            }
        }

        private bool IsCurrentRoomFinished()
        {
            return _currentRoom.CleanFactor >= GameManager.Instance.GameConfig.CleanSuccessRate 
                && !_currentRoom.IsCompleted;
        }

        private void UnlockAdjacentDoors(RoomController room)
        {
            var idx = room.RoomIndex;
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                var offset = dir switch
                {
                    Direction.Left => Vector2Int.left,
                    Direction.Right => Vector2Int.right,
                    Direction.Up => Vector2Int.up,
                    Direction.Down => Vector2Int.down,
                    _ => Vector2Int.zero
                };

                var neighborIdx = idx + offset;
                if (RoomManager.RoomLookup.TryGetValue(neighborIdx, out var neighborRoom))
                {
                    // abre la puerta de esta sala
                    room.OpenDoor(dir);
                    // abre la puerta contraria en la vecina
                    Direction opp = dir switch
                    {
                        Direction.Left => Direction.Right,
                        Direction.Right => Direction.Left,
                        Direction.Up => Direction.Down,
                        Direction.Down => Direction.Up,
                        _ => dir
                    };
                    neighborRoom.OpenDoor(opp);
                }
            }
        }

        public void CleanCurrentRoom()
        {
            _currentRoom.Complete();
            UnlockAdjacentDoors(_currentRoom);
            _completedRooms++;
            CheckWinCondition();
        }
    }
}
