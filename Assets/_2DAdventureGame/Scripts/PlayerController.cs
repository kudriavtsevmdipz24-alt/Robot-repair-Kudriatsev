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

    // ДЗ юніта 4: темп пострілів — після вистрілу C не діє, поки не мине кулдаун
    public float shootCooldown = 1.0f;
    float shootTimer;

    // Діалог: кнопка X і промінь, який шукає NPC
    public InputAction TalkAction;
    private NonPlayerCharacter lastNonPlayerCharacter;
    private const float talkDistance = 2.0f;

    void Start()
    {
        // Вмикаємо зчитування вводу
        MoveAction.Enable();
        LaunchAction.Enable();
        TalkAction.Enable();

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

        // Таймер темпу пострілів
        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        // Постріл за клавішею C — але не частіше, ніж дозволяє shootCooldown
        if (WasLaunchPressedThisFrame() && shootTimer <= 0)
        {
            Launch();
            shootTimer = shootCooldown;
        }

        // Промінь: чи дивиться робот на NPC (лише шар NPC, 1.5 одиниці вперед)
        RaycastHit2D hit = Physics2D.Raycast(
            rigidbody2d.position + Vector2.up * 0.2f,
            moveDirection, 1.5f, LayerMask.GetMask("NPC"));

        if (hit.collider != null)
        {
            // Промінь влучив — показуємо бульбашку з X цього NPC і запам'ятовуємо його
            NonPlayerCharacter npc = hit.collider.GetComponent<NonPlayerCharacter>();
            if (npc != null)
            {
                npc.dialogueBubble.SetActive(true);
                lastNonPlayerCharacter = npc;
            }
        }
        else
        {
            // Промінь не влучив, а хтось запам'ятований є — ховаємо його бульбашку
            if (lastNonPlayerCharacter != null)
            {
                lastNonPlayerCharacter.dialogueBubble.SetActive(false);
                lastNonPlayerCharacter = null;
            }
        }

        FindFriend();
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

        // Оновлюємо смугу здоров'я в HUD; (float) робить ділення дробовим: 2/5 = 0.4, а не 0
        UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
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

    // X: показати репліку NPC, якщо робот дивиться на нього
    void FindFriend()
    {
        if (!WasTalkPressedThisFrame())
        {
            return;
        }

        if (lastNonPlayerCharacter == null)
        {
            return;
        }

        // Підстраховка: діалог працює лише коли NPC реально поруч
        float distance = Vector2.Distance(rigidbody2d.position, lastNonPlayerCharacter.transform.position);
        if (distance <= talkDistance && UIHandler.instance != null)
        {
            UIHandler.instance.DisplayDialogue();
        }
    }

    bool WasLaunchPressedThisFrame()
    {
        bool actionPressed = LaunchAction.WasPressedThisFrame();
        bool fallbackPressed = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
        return actionPressed || fallbackPressed;
    }

    bool WasTalkPressedThisFrame()
    {
        bool actionPressed = TalkAction.WasPressedThisFrame();
        bool fallbackPressed = Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
        return actionPressed || fallbackPressed;
    }
}
