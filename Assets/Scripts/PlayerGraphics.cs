using UnityEngine;

public class PlayerGraphics : MonoBehaviour
{
    // Сюда мы перетащим 4 спрайта для каждого направления
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Получаем компонент для смены спрайта
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Устанавливаем спрайт по умолчанию при запуске игры
        spriteRenderer.sprite = spriteDown; 
    }

    public void SetDirectionalSprite(Vector2 direction)
    {
        if (direction == Vector2.up)
        {
            spriteRenderer.sprite = spriteUp;
        }
        else if (direction == Vector2.down)
        {
            spriteRenderer.sprite = spriteDown;
        }
        else if (direction == Vector2.left)
        {
            spriteRenderer.sprite = spriteLeft;
        }
        else if (direction == Vector2.right)
        {
            spriteRenderer.sprite = spriteRight;
        }
        // Если direction равен (0,0), спрайт не меняется,
        // игрок продолжает смотреть в последнем направлении.
    }
}