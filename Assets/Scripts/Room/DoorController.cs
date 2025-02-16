using UnityEngine;

namespace ggj25
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private GameObject _colliders;
        [SerializeField] private SpriteRenderer _blockImage;

        public void OpenDoor()
        {
            if (_colliders != null)
            {
                _colliders.gameObject.SetActive(false);
                _blockImage.gameObject.SetActive(false);
            }
        }
    }
}
