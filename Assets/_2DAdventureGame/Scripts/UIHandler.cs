using UnityEngine;
using UnityEngine.UIElements;

// HUD гри: смуга здоров'я, репліка NPC, екрани перемоги й поразки.
// Синглтон: HUD один на гру, тому доступний звідусіль через UIHandler.instance.
public class UIHandler : MonoBehaviour
{
    public static UIHandler instance { get; private set; }

    public float displayTime = 4.0f;

    private VisualElement m_NonPlayerDialogue;
    private float m_TimerDisplay;
    private VisualElement m_Healthbar;
    private VisualElement m_WinScreen;
    private VisualElement m_LoseScreen;

    // ДЗ юніта 5: лічильник «Полагоджено N / M» у HUD
    private Label m_FixedCounter;
    private int m_TotalEnemies;
    private int m_FixedEnemies;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        m_Healthbar = uiDocument.rootVisualElement.Q<VisualElement>("HealthBar");
        SetHealthValue(1.0f);

        m_NonPlayerDialogue = uiDocument.rootVisualElement.Q<VisualElement>("NPCDialogueContainer");
        if (m_NonPlayerDialogue != null)
        {
            m_NonPlayerDialogue.style.display = DisplayStyle.None;
        }
        m_TimerDisplay = -1.0f;

        m_LoseScreen = uiDocument.rootVisualElement.Q<VisualElement>("LoseScreenContainer");
        m_WinScreen = uiDocument.rootVisualElement.Q<VisualElement>("WinScreenContainer");

        // Лічильник полагоджених: загальна кількість ворогів відома на старті сцени
        m_FixedCounter = uiDocument.rootVisualElement.Q<Label>("FixedCounter");
        m_TotalEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        m_FixedEnemies = 0;
        UpdateFixedCounter();
    }

    public void SetHealthValue(float percentage)
    {
        m_Healthbar.style.width = Length.Percent(100 * percentage);
    }

    private void Update()
    {
        // Таймер показу репліки: коли скінчився — ховаємо вікно
        if (m_TimerDisplay > 0)
        {
            m_TimerDisplay -= Time.deltaTime;
            if (m_TimerDisplay < 0)
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
        }
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
