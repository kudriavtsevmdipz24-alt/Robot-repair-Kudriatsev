using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Шукаємо скрипт PlayerController на об'єкті, який зайшов у тригер
        PlayerController controller = other.GetComponent<PlayerController>();

        // Перевіряємо: це гравець І його здоров'я менше за максимальне
        if (controller != null && controller.health < controller.maxHealth)
        {
            controller.ChangeHealth(1); // Додаємо 1 одиницю здоров'я
            Destroy(gameObject);        // Знищуємо аптечку
        }
    }
}