using System.Collections;
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

    // Зворотний зв'язок (лаба 9): окремі звуки та короткий спалах на подію
    public AudioClip playerHitSound;
    public AudioClip collectibleSound;
    public AudioClip projectileThrowSound;
    public Color damageFlashColor = new Color(1f, 0.55f, 0.55f, 1f);
    public Color healFlashColor = new Color(0.65f, 1f, 0.65f, 1f);
    public float feedbackFlashTime = 0.15f;

    // ДЗ юніта 4: темп пострілів — після вистрілу C не діє, поки не мине кулдаун
    public float shootCooldown = 1.0f;
    float shootTimer;

    // Діалог: кнопка X і промінь, який шукає NPC
    public InputAction TalkAction;
    private NonPlayerCharacter lastNonPlayerCharacter;
    private const float talkDistance = 2.0f;

    AudioSource audioSource;
    SpriteRenderer spriteRenderer;
    Color baseColor;
    Coroutine feedbackRoutine;

    void OnEnable()
    {
        if (MoveAction != null) MoveAction.Enable();
        if (LaunchAction != null) LaunchAction.Enable();
        if (TalkAction != null) TalkAction.Enable();
    }

    void OnDisable()
    {
        if (MoveAction != null) MoveAction.Disable();
        if (LaunchAction != null) LaunchAction.Disable();
        if (TalkAction != null) TalkAction.Disable();
    }

    void Start()
    {

        // Отримуємо посилання на компоненти
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }

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
        if (animator != null)
        {
            animator.SetFloat("Look X", moveDirection.x);
            animator.SetFloat("Look Y", moveDirection.y);
            animator.SetFloat("Speed", move.magnitude);
        }

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
            PlaySound(playerHitSound);
            FlashSprite(damageFlashColor);
            isInvincible = true;
            damageCooldown = timeInvincible;
            animator.SetTrigger("Hit"); // анімація отриманого удару
        }

        int previousHealth = currentHealth;

        // Обмежуємо значення здоров'я від 0 до maxHealth
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (amount > 0 && currentHealth > previousHealth)
        {
            PlaySound(collectibleSound);
            FlashSprite(healFlashColor);
        }

        // Оновлюємо смугу здоров'я в HUD; (float) робить ділення дробовим: 2/5 = 0.4, а не 0
        if (UIHandler.instance != null)
        {
            UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
        }
    }

    // Постріл: створюємо снаряд перед роботом і штовхаємо його в напрямку погляду
    void Launch()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector2 aim = moveDirection.sqrMagnitude > 0.01f ? moveDirection.normalized : Vector2.right;
        PlaySound(projectileThrowSound);
        GameObject projectileObject = Instantiate(projectilePrefab,
            rigidbody2d.position + aim * 0.55f + Vector2.up * 0.2f,
            Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        if (projectile == null)
        {
            Destroy(projectileObject);
            return;
        }

        projectile.Launch(aim, 300);

        if (animator != null)
        {
            animator.SetTrigger("Launch");
        }
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
        bool actionPressed = LaunchAction != null && LaunchAction.WasPressedThisFrame();
        bool fallbackPressed = Keyboard.current != null &&
            (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
        return actionPressed || fallbackPressed;
    }

    bool WasTalkPressedThisFrame()
    {
        bool actionPressed = TalkAction.WasPressedThisFrame();
        bool fallbackPressed = Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
        return actionPressed || fallbackPressed;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void FlashSprite(Color flashColor)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }
        feedbackRoutine = StartCoroutine(FlashRoutine(flashColor));
    }

    IEnumerator FlashRoutine(Color flashColor)
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(feedbackFlashTime);
        spriteRenderer.color = baseColor;
        feedbackRoutine = null;
    }
}
