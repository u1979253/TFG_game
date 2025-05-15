using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ggj25
{
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

        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

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

        public void CleanCurrentRoom()
        {
            _currentRoom.Complete();
            _completedRooms++;
            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            if (_completedRooms >= _rooms.Count)
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

        /*
        private void UnlockAdjacentDoors(RoomController room)
        {
            var idx = room.RoomIndex;

            // chequea cada vecino y abre ambas puertas
            TryUnlock(idx + Vector2Int.left, Direction.Left);
            TryUnlock(idx + Vector2Int.right, Direction.Right);
            TryUnlock(idx + Vector2Int.up, Direction.Up);
            TryUnlock(idx + Vector2Int.down, Direction.Down);
        }
        
        private void TryUnlock(Vector2Int neighborIdx, Direction dirFromCurrent)
        {
            if (RoomManager.RoomLookup.TryGetValue(neighborIdx, out var neighbor))
            {
                // abre puerta en la habitación actual:
                _currentRoom.OpenDoor(dirFromCurrent);

                // abre la puerta contraria en el vecino:
                Direction opposite = Opposite(dirFromCurrent);
                neighbor.OpenDoor(opposite);
            }
        }

        private Direction Opposite(Direction d)
        {
            return d switch
            {
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        */
        /*public void Rewpawn()
        {
            _hero.transform.position = _currentRoom.CheckPoint.position;
        }
        */
    }
}
