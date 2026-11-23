using UnityEngine;

// Механіка-твіст: вода повільно піднімається і завдає шкоди при контакті.
// Усі числа керуються з Inspector (швидкість, межа висоти, шкода).
public class RisingWater : MonoBehaviour
{
    public float riseSpeed = 0.22f;
    public float maxHeight = -0.6f;
    public int damage = 1;

    void Update()
    {
        if (transform.position.y >= maxHeight)
        {
            return;
        }

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ChangeHealth(-damage);
        }
    }
}
