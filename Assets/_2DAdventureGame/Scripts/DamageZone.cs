using UnityEngine;

// Зона шкоди: шкодить гравцю, поки той всередині. Сила шкоди — у полі префаба (Damage).
public class DamageZone : MonoBehaviour
{
    public int damage = 1;

    // Викликається постійно (кожен такт фізики), поки об'єкт перебуває всередині тригера
    void OnTriggerStay2D(Collider2D other)
    {
        // Шукаємо скрипт PlayerController на об'єкті, що завітав у зону
        PlayerController controller = other.GetComponent<PlayerController>();

        // Якщо це гравець — завдаємо шкоди
        if (controller != null)
        {
            controller.ChangeHealth(-damage);
        }
    }
}
