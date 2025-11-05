using UnityEngine;

namespace Sokoban
{
    public class PlayerGraphics : MonoBehaviour
    {

        [SerializeField] private Sprite spriteUp;
        [SerializeField] private Sprite spriteDown;
        [SerializeField] private Sprite spriteLeft;
        [SerializeField] private Sprite spriteRight;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {

            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteDown != null)
            {
                spriteRenderer.sprite = spriteDown;
            }
        }

        public void SetDirectionalSprite(Vector2 direction)
        {
            spriteRenderer.sprite = direction switch
            {
                _ when direction == Vector2.up => spriteUp,
                _ when direction == Vector2.down => spriteDown,
                _ when direction == Vector2.left => spriteLeft,
                _ when direction == Vector2.right => spriteRight,
                _ => spriteRenderer.sprite
            };
        }
    }
}
