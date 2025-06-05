using Components.Player;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Components.Ui.Pages.Menu
{
    public class LevelsPage : BasePage
    {
        [SerializeField] private Button _levelButtonPrefab;
        [SerializeField] private RectTransform _levelsContainer;
        [SerializeField] private Button _back;

        private SceneService _sceneService;
        private DiContainer _container;

        [Inject]
        private void Construct(SceneService sceneService, DiContainer container)
        {
            _sceneService = sceneService;
            _container = container;

            CreateLevelButtons();
        }

        private void Awake()
        {
            _back.onClick.AddListener(() => { PageSwitcher.Open(PageName.Menu).Forget(); });
        }

        private void OnDestroy()
        {
            _back.onClick.RemoveAllListeners();
        }

        private void CreateLevelButtons()
        {
            for (var i = 2; i < _sceneService.GetScenesCount(); i++)
            {
                var levelIndex = i;
                var newLevelGO = _container.InstantiatePrefab(_levelButtonPrefab, _levelsContainer);
                var newLevel = newLevelGO.GetComponent<Button>();
                newLevel.GetComponentInChildren<TMP_Text>().text = $"{i - 1}";
                newLevel.onClick.AddListener(() =>
                {
                    _sceneService.SceneToLoad = levelIndex;
                    PageSwitcher.Open(PageName.Start).Forget();
                    PlayerHealth.DeathCount = 0;
                });
            }
        }
    }
}
