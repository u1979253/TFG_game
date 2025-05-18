using UnityEngine;
using UnityEngine.SceneManagement;

namespace ggj25
{
    public class EnterRoomDetector : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        public RoomController _roomController;

        [SerializeField] private bool isProceduralPortal = false;
        private static bool proceduralStarted = false;

        private void Awake()
        {
            _roomController = GetComponentInParent<RoomController>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            print("colisio");

            if ((_layerMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            if(isProceduralPortal && !proceduralStarted)
            {
                proceduralStarted = true;
                var world = GameObject.Find("World");
                if (world)
                {
                    world.SetActive(false);
                }
                var rm = GameObject.FindObjectOfType<RoomManager>();
                rm.gameObject.SetActive(true);
                rm.StartGeneration();


            }

            if (GameManager.Instance?.LevelManager != null && _roomController != null)
            {
                GameManager.Instance.LevelManager.SelectRoom(_roomController);
                Debug.Log($"Sala seleccionada: {_roomController.name}");
            }
        
        }
    }
}
