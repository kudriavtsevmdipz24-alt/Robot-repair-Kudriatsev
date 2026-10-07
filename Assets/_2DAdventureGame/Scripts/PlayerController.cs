using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Налаштування руху
    public InputAction MoveAction;
    public float speed = 3.0f;

    // Компоненти та внутрішні змінні руху
    Rigidbody2D rigidbody2d;
    Vector2 move;

    // Налаштування здоров'я
    public int maxHealth = 5;
    public int health { get { return currentHealth; } }
    int currentHealth;

    // Налаштування невразливості після отримання шкоди
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float damageCooldown;

    void Start()
    {
        // Вмикаємо зчитування вводу
        MoveAction.Enable();

        // Отримуємо посилання на Rigidbody2D
        rigidbody2d = GetComponent<Rigidbody2D>();

        // Встановлюємо початкове здоров'я на максимум
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Читаємо вектор руху від гравця
        move = MoveAction.ReadValue<Vector2>();

        // Відлік таймера невразливості
        if (isInvincible)
        {
            damageCooldown -= Time.deltaTime;
            if (damageCooldown < 0)
            {
                isInvincible = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Фізичне переміщення об'єкта через MovePosition у такт фізики
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    // Публічний метод зміни здоров'я (підходить і для лікування, і для шкоди)
    public void ChangeHealth(int amount)
    {
        // Якщо це шкода (amount від'ємне)
        if (amount < 0)
        {
            if (isInvincible)
            {
                return; // Шкода не проходить, якщо персонаж невразливий
            }
            isInvincible = true;
            damageCooldown = timeInvincible;
        }

        // Обмежуємо значення здоров'я від 0 до maxHealth
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // Виводимо поточний стан у консоль Unity
        Debug.Log(currentHealth + "/" + maxHealth);
    }
}