using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class TimedSwitch : MonoBehaviour
{
    public GameObject door;
    [Min(0.1f)] public float openTime = 3.5f;
    [SerializeField] Color activeColor = new Color(0.35f, 0.95f, 0.65f);
    [SerializeField] TimedSwitch primarySwitch;

    SpriteRenderer plate;
    Color idleColor;
    float closesAt;
    bool isOpen;

    void Awake()
    {
        plate = GetComponent<SpriteRenderer>();
        idleColor = plate.color;
        if (primarySwitch == null)
        {
            SetDoorOpen(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled || other.isTrigger ||
            !other.TryGetComponent(out PlayerController player) || player.health <= 0)
        {
            return;
        }

        var gateSwitch = primarySwitch != null ? primarySwitch : this;
        gateSwitch.Activate();
    }

    void Activate()
    {
        if (!isActiveAndEnabled || door == null)
        {
            return;
        }

        closesAt = Time.time + openTime;
        SetDoorOpen(true);
    }

    void Update()
    {
        if (primarySwitch != null)
        {
            plate.color = primarySwitch.isOpen ? activeColor : idleColor;
            return;
        }

        if (!isOpen)
        {
            return;
        }

        float remaining = Mathf.Max(0f, closesAt - Time.time);
        UIHandler.instance?.SetDoorTime(remaining, openTime);
        if (remaining == 0f)
        {
            SetDoorOpen(false);
        }
    }

    void OnDisable()
    {
        if (isOpen)
        {
            SetDoorOpen(false);
            UIHandler.instance?.SetDoorTime(0f, openTime);
        }
    }

    void SetDoorOpen(bool open)
    {
        isOpen = open;
        plate.color = open ? activeColor : idleColor;
        if (door != null)
        {
            door.SetActive(!open);
        }
    }
}
