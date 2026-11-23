using UnityEngine;
using UnityEngine.SceneManagement;

// «Диспетчер» гри: стежить за умовами перемоги й поразки і перезапускає сцену.
public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public UIHandler uiHandler;

    EnemyController[] enemies;

    // ДЗ юніта 5: прапорець, щоб перезапуск не ставався у чергу сотні разів
    bool gameOver;

    void Start()
    {
        // GameManager сам знаходить усіх ворогів на сцені
        enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (gameOver)
        {
            return;
        }

        // Поразка: здоров'я робота впало до нуля
        if (player.health <= 0)
        {
            uiHandler.DisplayLoseScreen();
            gameOver = true;
            Invoke(nameof(ReloadScene), 3f);
        }

        // Перемога: усі вороги полагоджені
        if (AllEnemiesFixed())
        {
            uiHandler.DisplayWinScreen();
            gameOver = true;
            Invoke(nameof(ReloadScene), 3f);
        }
    }

    bool AllEnemiesFixed()
    {
        foreach (EnemyController enemy in enemies)
        {
            if (enemy.isBroken)
            {
                return false;
            }
        }
        return true;
    }
}
