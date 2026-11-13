using UnityEngine;

// Аптечка: лікує гравця. Число здоров'я тепер у полі префаба (Amount),
// тож можна зробити велику аптечку варіантом префаба, а не другим скриптом.
public class HealthCollectible : MonoBehaviour
{
    public int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Шукаємо скрипт PlayerController на об'єкті, який зайшов у тригер
        PlayerController controller = other.GetComponent<PlayerController>();

        // Перевіряємо: це гравець І його здоров'я менше за максимальне
        if (controller != null && controller.health < controller.maxHealth)
        {
            controller.ChangeHealth(amount); // додаємо стільки, скільки задано в префабі
            Destroy(gameObject);             // знищуємо аптечку
        }
    }
}
