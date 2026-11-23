using UnityEngine;

// Ворог, що патрулює за таймером і шкодить на дотик.
// Снаряд його не знищує, а лагодить (Fix) — зламана машина стає справною.
public class EnemyController : MonoBehaviour
{
    public float speed;
    public bool vertical;
    public float changeTime = 3.0f;

    // ДЗ юніта 4: якщо ввімкнено — ворог випадково міняє вісь руху на кожному розвороті
    public bool randomAxis;

    Rigidbody2D rigidbody2d;
    Animator animator;

    float timer;
    int direction = 1;
    bool broken = true;
    Vector2 patrolOrigin;
    float patrolRange;

    // Жорсткі межі рівня — ворог не виходить за ігрову зону
    const float WorldMinX = -12.5f;
    const float WorldMaxX = 11.5f;
    const float WorldMinY = -4.8f;
    const float WorldMaxY = 2.8f;

    // Публічна властивість для GameManager: чи ворог ще зламаний
    public bool isBroken { get { return broken; } }

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        timer = changeTime;
        patrolOrigin = rigidbody2d.position;
        patrolRange = speed * changeTime;
    }

    void Update()
    {
        // Таймер патруля: коли скінчився — змінюємо напрям і починаємо знову
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            direction = -direction;
            timer = changeTime;
            if (randomAxis)
            {
                vertical = !vertical; // випадкова зміна осі руху
            }
        }
    }

    void FixedUpdate()
    {
        // Полагоджений ворог не рухається і не шкодить
        if (!broken)
        {
            return;
        }

        Vector2 position = rigidbody2d.position;

        if (vertical)
        {
            position.y = position.y + speed * direction * Time.deltaTime;
            position.y = Mathf.Clamp(position.y, patrolOrigin.y - patrolRange, patrolOrigin.y + patrolRange);
            animator.SetFloat("Move X", 0);
            animator.SetFloat("Move Y", direction);
        }
        else
        {
            position.x = position.x + speed * direction * Time.deltaTime;
            position.x = Mathf.Clamp(position.x, patrolOrigin.x - patrolRange, patrolOrigin.x + patrolRange);
            animator.SetFloat("Move X", direction);
            animator.SetFloat("Move Y", 0);
        }

        position.x = Mathf.Clamp(position.x, WorldMinX, WorldMaxX);
        position.y = Mathf.Clamp(position.y, WorldMinY, WorldMaxY);

        rigidbody2d.MovePosition(position);
    }

    // Ворог б'є раз, коли торкнувся (OnTriggerEnter2D, а не Stay)
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ChangeHealth(-1);
        }
    }

    // Полагодити ворога: викликає снаряд
    public void Fix()
    {
        broken = false;
        rigidbody2d.simulated = false;
        animator.SetTrigger("Fixed");

        // ДЗ юніта 5: оновлюємо лічильник полагоджених у HUD
        if (UIHandler.instance != null)
        {
            UIHandler.instance.OnEnemyFixed();
        }
    }
}
