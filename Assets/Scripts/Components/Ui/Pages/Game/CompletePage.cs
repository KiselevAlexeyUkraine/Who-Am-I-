using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;

namespace Components.Ui.Pages.Game
{
    public class CompletePage : BasePage
    {
        [SerializeField] private TMP_Text _tile;
        //[SerializeField] private TMP_Text _time;
        [SerializeField] private Button _nextLevel;
        [SerializeField] private Button _restart;
        [SerializeField] private Button _menu;
        [SerializeField] private Button _diaryButton;

        [Inject]
        private void Construct(PauseService pauseService, PauseSwitcher pauseSwitcher)
        {
            OnOpen += () =>
            {
                pauseService.Pause();
                pauseSwitcher.enabled = false;
            };
        }

        private void Awake()
        {
            _nextLevel.onClick.AddListener(() => { PageSwitcher.Open(PageName.NextLevel).Forget(); });
            _restart.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameRestart).Forget(); });
            _menu.onClick.AddListener(() => { PageSwitcher.Open(PageName.GameExit).Forget(); });
            _diaryButton.onClick.AddListener(() => { PageSwitcher.Open(PageName.Diary).Forget(); });

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int totalScenes = SceneManager.sceneCountInBuildSettings;

            bool isLastLevel = currentSceneIndex >= totalScenes - 1;

            if (isLastLevel)
            {
                _tile.text = "Молодец, ты прошёл игру!";
                _nextLevel.interactable = false;
                _nextLevel.gameObject.SetActive(false);
            }
            else
            {
                _tile.text = "Уровень пройден!";
                _nextLevel.interactable = true;
                _nextLevel.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _nextLevel.onClick.RemoveAllListeners();
            _restart.onClick.RemoveAllListeners();
            _menu.onClick.RemoveAllListeners();
            _diaryButton.onClick.RemoveAllListeners();
        }
    }
}
