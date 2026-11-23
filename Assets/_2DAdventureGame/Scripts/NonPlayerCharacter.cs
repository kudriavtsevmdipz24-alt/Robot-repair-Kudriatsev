using UnityEngine;

// NPC: тримає бульбашку з клавішею X, яку показує PlayerController через промінь
public class NonPlayerCharacter : MonoBehaviour
{
    public GameObject dialogueBubble;

    void Start()
    {
        dialogueBubble.SetActive(false);
    }
}
