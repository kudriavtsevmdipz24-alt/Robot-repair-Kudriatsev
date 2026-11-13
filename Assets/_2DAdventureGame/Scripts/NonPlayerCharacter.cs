using UnityEngine;

// NPC: тримає бульбашку з клавішею X, яку показує PlayerController через промінь
public class NonPlayerCharacter : MonoBehaviour
{
    public GameObject dialogueBubble;
    public Vector3 bubbleLocalPosition = new Vector3(0f, 2.2f, 0f);
    public Vector3 bubbleLocalScale = new Vector3(0.03f, 0.03f, 1f);

    void Awake()
    {
        if (dialogueBubble == null)
        {
            return;
        }

        // Захист від scene/prefab override: у грі завжди ставимо читабельний розмір і позицію.
        dialogueBubble.transform.localPosition = bubbleLocalPosition;
        dialogueBubble.transform.localScale = bubbleLocalScale;
        dialogueBubble.SetActive(false);
    }
}
