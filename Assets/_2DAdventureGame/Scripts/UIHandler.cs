using UnityEngine;
using UnityEngine.UIElements;

// HUD гри: смуга здоров'я, репліка NPC, екрани перемоги й поразки.
// Синглтон: HUD один на гру, тому доступний звідусіль через UIHandler.instance.
public class UIHandler : MonoBehaviour
{
    public static UIHandler instance { get; private set; }

    public float displayTime = 4.0f;

    private VisualElement m_NonPlayerDialogue;
    private Label m_DialogueLabel;
    private float m_TimerDisplay;
    private VisualElement m_Healthbar;
    private VisualElement m_WinScreen;
    private VisualElement m_LoseScreen;
    private VisualElement m_DoorTimer;
    private Label m_DoorTimerText;
    private VisualElement m_DoorTimerFill;

    // ДЗ юніта 5: лічильник «Полагоджено N / M» у HUD
    private Label m_FixedCounter;
    private int m_TotalEnemies;
    private int m_FixedEnemies;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        root.style.width = Length.Percent(100);
        root.style.height = Length.Percent(100);

        m_Healthbar = root.Q<VisualElement>("HealthBar");
        SetHealthValue(1.0f);

        m_NonPlayerDialogue = root.Q<VisualElement>("NPCDialogueContainer");
        if (m_NonPlayerDialogue != null)
        {
            m_NonPlayerDialogue.style.display = DisplayStyle.None;
            m_NonPlayerDialogue.style.overflow = Overflow.Hidden;
            m_DialogueLabel = m_NonPlayerDialogue.Q<Label>("NPCDialogueText");
            ConfigureDialogueLabel();
        }
        m_TimerDisplay = -1.0f;

        m_LoseScreen = root.Q<VisualElement>("LoseScreenContainer");
        m_WinScreen = root.Q<VisualElement>("WinScreenContainer");
        m_DoorTimer = root.Q("DoorTimer");
        m_DoorTimerText = root.Q<Label>("DoorTimerText");
        m_DoorTimerFill = root.Q("DoorTimerFill");

        // Лічильник полагоджених: загальна кількість ворогів відома на старті сцени
        m_FixedCounter = root.Q("FixedCounter")?.Q<Label>("CounterLabel");
        m_TotalEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        m_FixedEnemies = 0;
        UpdateFixedCounter();
    }

    public void SetHealthValue(float percentage)
    {
        m_Healthbar.style.width = Length.Percent(100 * percentage);
    }

    public void SetDoorTime(float remaining, float duration)
    {
        if (m_DoorTimer == null)
        {
            return;
        }

        m_DoorTimer.style.display = remaining > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        m_DoorTimerText.text = $"Двері відчинені · {Mathf.CeilToInt(remaining)} с";
        m_DoorTimerFill.style.width = Length.Percent(duration > 0f ? 100f * Mathf.Clamp01(remaining / duration) : 0f);
    }

    private void Update()
    {
        // Таймер показу репліки: коли скінчився — ховаємо вікно
        if (m_TimerDisplay > 0)
        {
            m_TimerDisplay -= Time.deltaTime;
            if (m_TimerDisplay <= 0)
            {
                m_NonPlayerDialogue.style.display = DisplayStyle.None;
            }
        }
    }

    public void DisplayDialogue()
    {
        if (m_NonPlayerDialogue != null)
        {
            m_NonPlayerDialogue.style.display = DisplayStyle.Flex;
            m_TimerDisplay = displayTime;
            m_NonPlayerDialogue.schedule.Execute(ConfigureDialogueLabel);
        }
    }

    void ConfigureDialogueLabel()
    {
        if (m_DialogueLabel == null)
        {
            return;
        }

        m_DialogueLabel.style.position = Position.Absolute;
        m_DialogueLabel.style.left = Length.Percent(12);
        m_DialogueLabel.style.right = Length.Percent(12);
        m_DialogueLabel.style.top = Length.Percent(20);
        m_DialogueLabel.style.bottom = Length.Percent(16);
        m_DialogueLabel.style.whiteSpace = WhiteSpace.Normal;
        m_DialogueLabel.style.overflow = Overflow.Hidden;
        m_DialogueLabel.style.color = Color.black;
    }

    public void DisplayWinScreen()
    {
        m_WinScreen.style.opacity = 1.0f;
    }

    public void DisplayLoseScreen()
    {
        m_LoseScreen.style.opacity = 1.0f;
    }

    // Викликається ворогом у Fix(): оновлює лічильник у HUD
    public void OnEnemyFixed()
    {
        m_FixedEnemies++;
        UpdateFixedCounter();
    }

    private void UpdateFixedCounter()
    {
        if (m_FixedCounter != null)
        {
            m_FixedCounter.text = "Полагоджено " + m_FixedEnemies + " / " + m_TotalEnemies;
        }
    }
}
