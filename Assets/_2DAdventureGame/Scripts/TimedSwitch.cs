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
        door.SetActive(false);
        yield return new WaitForSeconds(seconds);
        if (door != null)
        {
            door.SetActive(true);
        }
    }
}
