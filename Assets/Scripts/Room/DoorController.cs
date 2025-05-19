using UnityEngine;

namespace ggj25
{
    public class DoorController : MonoBehaviour
    {

        public enum Direction { Left, Right, Up, Down }
        [SerializeField] private GameObject _colliders;
        [SerializeField] private SpriteRenderer _blockImage;

        [SerializeField] private Direction _direction;
        

        public Direction Dir => _direction;

        public void OpenDoor()
        {
            if (_colliders != null)
            {
                _colliders.gameObject.SetActive(false);
                _blockImage.gameObject.SetActive(false);
            }
        }

        public void Open()
        {
            if (_colliders != null) _colliders.SetActive(false);
            if (_blockImage != null) _blockImage.enabled = false;
        }
    }
}
