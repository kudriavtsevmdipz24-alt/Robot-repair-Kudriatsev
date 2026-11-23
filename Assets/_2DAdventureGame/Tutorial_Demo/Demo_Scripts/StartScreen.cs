using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Beginner2D
{
    // Стартовий екран для фінальної версії: одна кнопка і перехід у MyLevel.
    public class StartScreen : MonoBehaviour
    {
        [SerializeField] string levelSceneName = "MyLevel";
        [SerializeField] string startButtonName = "StartButton";

        UIDocument m_UIDocument;
        Button m_StartButton;

        private void OnEnable()
        {
            m_UIDocument = GetComponent<UIDocument>();
            if (m_UIDocument == null)
            {
                return;
            }

            m_StartButton = m_UIDocument.rootVisualElement.Q<Button>(startButtonName);
            if (m_StartButton != null)
            {
                m_StartButton.clicked += StartGame;
            }
        }

        private void OnDisable()
        {
            if (m_StartButton != null)
            {
                m_StartButton.clicked -= StartGame;
            }
        }

        void StartGame()
        {
            SceneManager.LoadScene(levelSceneName);
        }
    }
}