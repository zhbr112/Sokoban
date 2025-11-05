using UnityEngine;
using UnityEngine.InputSystem;

namespace Sokoban
{
    public class PlayerController : MonoBehaviour
    {
        private GameManager gameManager;
        private bool moveCooldown = false;

        void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        public void OnMove(InputValue value)
        {

            if (gameManager.isGameWon || GameManager.isGamePaused || moveCooldown) return;

            Vector2 inputVector = value.Get<Vector2>();

            Vector2 moveDirection = new Vector2(Mathf.Round(inputVector.x), Mathf.Round(inputVector.y));

            if (moveDirection != Vector2.zero)
            {
                gameManager.MovePlayer(moveDirection);

                moveCooldown = true;
                Invoke(nameof(ResetCooldown), 0.1f);
            }
        }

        private void ResetCooldown()
        {
            moveCooldown = false;
        }
    }
}