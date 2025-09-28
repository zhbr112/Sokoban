using UnityEngine;

namespace Sokoban
{
    public class PlayerGraphics : MonoBehaviour
    {
        // Сюда мы перетащим 4 спрайта для каждого направления
        [SerializeField] private Sprite spriteUp;
        [SerializeField] private Sprite spriteDown;
        [SerializeField] private Sprite spriteLeft;
        [SerializeField] private Sprite spriteRight;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            // Получаем компонент для смены спрайта
            spriteRenderer = GetComponent<SpriteRenderer>();
            // Устанавливаем спрайт по умолчанию при запуске игры
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
                _ => spriteRenderer.sprite // Если direction равен (0,0), спрайт не меняется
            };
        }
    }
}
