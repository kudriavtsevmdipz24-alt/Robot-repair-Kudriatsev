using System.Collections;
using UnityEngine;

// Твіст: натискаємо плиту, двері відкриваються на кілька секунд.
public class TimedSwitch : MonoBehaviour
{
    public GameObject door;
    public float openTime = 4.0f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        if (door == null)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(OpenDoorFor(openTime));
    }

    IEnumerator OpenDoorFor(float seconds)
    {
        SetDoorOpen(true);
        yield return new WaitForSeconds(seconds);
        if (door != null)
        {
            SetDoorOpen(false);
        }
    }

    // Відкриваємо лише «середній сегмент» стіни — бічні блоки лишаються.
    void SetDoorOpen(bool open)
    {
        if (door == null)
        {
            return;
        }

        door.SetActive(true);
        foreach (var col in door.GetComponentsInChildren<Collider2D>(true))
        {
            col.enabled = !open;
        }

        var renderer = door.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = !open;
        }
    }
}
