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

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;

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
				var newLevel = Instantiate(_levelButtonPrefab, _levelsContainer);
                newLevel.GetComponentInChildren<TMP_Text>().text = $"{i - 1}";
				newLevel.onClick.AddListener(() =>
				{
					_sceneService.SceneToLoad = levelIndex;
                    PageSwitcher.Open(PageName.Start).Forget();
				});
			}
		}
	}
}