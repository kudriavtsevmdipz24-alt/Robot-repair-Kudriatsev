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

    // Анімація: напрям погляду пам'ятається, щоб стоячи не обертатися «до глядача»
    Animator animator;
    Vector2 moveDirection = new Vector2(1, 0);

    // Снаряд і кнопка пострілу (C)
    public GameObject projectilePrefab;
    public InputAction LaunchAction;

    void Start()
    {
        // Вмикаємо зчитування вводу
        MoveAction.Enable();
        LaunchAction.Enable();

        // Отримуємо посилання на компоненти
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Встановлюємо початкове здоров'я на максимум
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Читаємо вектор руху від гравця
        move = MoveAction.ReadValue<Vector2>();

        // Якщо робот рухається — запам'ятовуємо напрям погляду
        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }

        // Передаємо напрям і швидкість в аніматор (Blend Tree глядає за поглядом)
        animator.SetFloat("Look X", moveDirection.x);
        animator.SetFloat("Look Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        // Відлік таймера невразливості
        if (isInvincible)
        {
            damageCooldown -= Time.deltaTime;
            if (damageCooldown < 0)
            {
                isInvincible = false;
            }
        }

        // Постріл за клавішею C
        if (LaunchAction.WasPressedThisFrame())
        {
            Launch();
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
            animator.SetTrigger("Hit"); // анімація отриманого удару
        }

        // Обмежуємо значення здоров'я від 0 до maxHealth
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // Виводимо поточний стан у консоль Unity
        Debug.Log(currentHealth + "/" + maxHealth);
    }

    // Постріл: створюємо снаряд перед роботом і штовхаємо його в напрямку погляду
    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab,
            rigidbody2d.position + Vector2.up * 0.5f,
            Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(moveDirection, 300);

        animator.SetTrigger("Launch");
    }
}
