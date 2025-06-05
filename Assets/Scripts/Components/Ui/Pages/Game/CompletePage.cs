using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;
using Components.Player;

namespace Components.Ui.Pages.Game
{
    public class CompletePage : BasePage
    {
        [SerializeField] private TMP_Text _tile;
        [SerializeField] private TMP_Text _deathCounter;
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
            _nextLevel.onClick.AddListener(() =>
            {
                PageSwitcher.Open(PageName.NextLevel).Forget();
                PlayerHealth.DeathCount = 0;
            });

            _restart.onClick.AddListener(() =>
            {
                PageSwitcher.Open(PageName.GameRestart).Forget();
                PlayerHealth.DeathCount = 0;
            });

            _menu.onClick.AddListener(() =>
            {
                PageSwitcher.Open(PageName.GameExit).Forget();
                PlayerHealth.DeathCount = 0;
            });

            _diaryButton.onClick.AddListener(() => { PageSwitcher.Open(PageName.DiaryGame).Forget(); });

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int totalScenes = SceneManager.sceneCountInBuildSettings;

            bool isLastLevel = currentSceneIndex >= totalScenes - 1;

            if (isLastLevel)
            {
                _tile.text = "Молодец, ты прошёл игру!";
                _nextLevel.interactable = false;
                _nextLevel.gameObject.SetActive(false);
                PlayerHealth.DeathCount = 0;
            }
            else
            {
                _tile.text = "Уровень пройден!";
                _nextLevel.interactable = true;
                _nextLevel.gameObject.SetActive(true);
            }

            _deathCounter.text = $"{PlayerHealth.DeathCount}";
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
