using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public void Awake()
        { 
            Init();
        }

        private void Init()
        {
            _rooms = GameObject.FindObjectsOfType<RoomController>().ToList();
            _hero = GameObject.FindObjectOfType<HeroController>();

            var initialRoom = _rooms.Find(room => room.name.Contains("tutori",
                                                                  StringComparison.InvariantCultureIgnoreCase));
            
            SelectRoom(initialRoom);
            
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

        public void Rewpawn()
        {
            _hero.transform.position = _currentRoom.CheckPoint.position;
        }
    }
}
