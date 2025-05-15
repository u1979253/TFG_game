using UnityEngine;
using UnityEngine.SceneManagement;

namespace ggj25
{
    public class EnterRoomDetector : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        public RoomController _roomController;

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
            print("colisio");
            if (1 << other.gameObject.layer == _layerMask.value)
            {
                print("condicio passada");
                if (_changeScene && !_hasChanged && !string.IsNullOrEmpty(_sceneToLoad))
                {
                    // Guarda info del spawn para usarla después
                    // SceneTransferData.SpawnPosition = _spawnPositionInNewScene;
                    print("entro canvi escena");
                    // Cambia de escena
                    _hasChanged = true;
                    if (GameManager.Instance.LevelManager == null)
                        GameManager.Instance.LevelManager =
                            new GameObject(nameof(LevelManager)).AddComponent<LevelManager>();

                    // 2) Ahora suscribe a sceneLoaded _antes_ de LoadScene:
                    SceneManager.sceneLoaded += GameManager.Instance.LevelManager.OnSceneLoaded;
                    //SceneManager.LoadScene(_sceneToLoad);
                    GameManager.Instance.ToProcedural();
                }
                else
                {
                    print("level manager nuull?" + GameManager.Instance?.LevelManager);
                    print("room controller nuull?" + _roomController);
                    if (GameManager.Instance?.LevelManager != null && _roomController != null)
                    {
                        print("agafo hab" + _roomController.name + " en " + _roomController.transform.position);
                        GameManager.Instance.LevelManager.SelectRoom(_roomController);

                        _changeScene = true;
                    }
                }
            }
        }
    }
}
