using UnityEngine;

// Снаряд робота: летить в напрямку погляду і лагодить зламаних ворогів.
public class Projectile : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    bool spent;

    // Awake, а не Start: PlayerController викликає Launch одразу після Instantiate,
    // тож посилання на Rigidbody2D має бути готове вже тут.
    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Снаряд, що полетів у нікуди, прибираємо за 100 одиниць від центру світу
        if (transform.position.magnitude > 100.0f)
        {
            Destroy(gameObject);
        }
    }

    // Запуск: напрямок задає PlayerController (погляд робота), сила 300
    public void Launch(Vector2 direction, float force)
    {
        rigidbody2d.AddForce(direction * force);
    }

    // Влучання у тригер (ворог — тригер; стіни теж повідомляються, бо колайдер снаряда — тригер)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || other.GetComponentInParent<PlayerController>() != null)
        {
            return;
        }

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy == null && other.isTrigger)
        {
            return;
        }

        spent = true;
        enemy?.Fix();
        Destroy(gameObject);
    }

    // Страховка: спрацює, лише якщо в снаряда вимкнено Is Trigger
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            return;
        }
        Destroy(gameObject);
    }
}
