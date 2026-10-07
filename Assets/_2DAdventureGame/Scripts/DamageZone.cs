using UnityEngine;

public class DamageZone : MonoBehaviour
{
    // Викликається постійно (кожен такт фізики), поки об'єкт перебуває всередині тригера
    void OnTriggerStay2D(Collider2D other)
    {
        // Шукаємо скрипт PlayerController на об'єкті, що завітав у зону
        PlayerController controller = other.GetComponent<PlayerController>();

        // Якщо це гравець — завдаємо шкоди (-1)
        if (controller != null)
        {
            controller.ChangeHealth(-1);
        }
    }
}