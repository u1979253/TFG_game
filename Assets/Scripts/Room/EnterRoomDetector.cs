using UnityEngine;
using UnityEngine.SceneManagement;

namespace ggj25
{
    public class EnterRoomDetector : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        private RoomController _roomController;

        [SerializeField] private bool _changeScene = false;
        [SerializeField] private bool _hasChanged = false;
        [SerializeField] private string _sceneToLoad;

        private void Awake()
        {
            _roomController = GetComponentInParent<RoomController>();
            print("RoomController" + _roomController.name + " found in " + _roomController);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (1 << other.gameObject.layer == _layerMask.value)
            {
                if (_changeScene && !_hasChanged && !string.IsNullOrEmpty(_sceneToLoad))
                {
                    // Guarda info del spawn para usarla después
                    // SceneTransferData.SpawnPosition = _spawnPositionInNewScene;

                    // Cambia de escena
                    _hasChanged = true;
                    SceneManager.LoadScene(_sceneToLoad);
                    
                }
                else
                {
                    if (GameManager.Instance?.LevelManager != null && _roomController != null)
                    {
                        GameManager.Instance.LevelManager.SelectRoom(_roomController);

                        _changeScene = true;
                    }
                }
            }
        }
    }
}
