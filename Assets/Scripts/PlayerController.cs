using UnityEngine;
using UnityEngine.InputSystem; // Важно подключить новую систему

public class PlayerController : MonoBehaviour
{
    private GameManager gameManager;
    private bool moveCooldown = false; // Чтобы не двигаться каждый кадр при зажатой клавише

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Этот метод будет вызван автоматически компонентом Player Input
    // когда вы нажимаете клавиши, привязанные к действию "Move"
    public void OnMove(InputValue value)
    {
        // Если игра выиграна или мы уже в процессе хода, ничего не делаем
        if (gameManager.isGameWon || GameManager.isGamePaused || moveCooldown) return;

        // Получаем вектор направления из ввода
        Vector2 inputVector = value.Get<Vector2>();

        // Округляем значения, чтобы получить чистое направление (1,0), (-1,0), (0,1) и т.д.
        // Это защитит от диагонального движения
        Vector2 moveDirection = new Vector2(Mathf.Round(inputVector.x), Mathf.Round(inputVector.y));

        if (moveDirection != Vector2.zero)
        {
            gameManager.MovePlayer(moveDirection);

            // Включаем небольшую задержку, чтобы один клик = один ход
            moveCooldown = true;
            Invoke(nameof(ResetCooldown), 0.1f); // Сбрасываем через 0.1 секунды
        }
    }

    private void ResetCooldown()
    {
        moveCooldown = false;
    }
}