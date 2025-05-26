using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System.Collections.Generic;
using Services;

namespace Components.Ui.Pages.Game
{
    public class TutorialPage : BasePage
    {
        [SerializeField] private Button _continue;
        [SerializeField] private TextMeshProUGUI _tutorialTextMesh;
        [SerializeField] private Image _tutorialImage;

        [SerializeField] private List<string> _tutorialTexts;
        [SerializeField] private List<Sprite> _tutorialSprites;

        private int _currentIndex = -1;

        private PauseService _pauseService;

        [Inject]
        private void Construct(PauseService pauseService)
        {
            OnOpen += () =>
            {
                pauseService.Pause();
                ShowNextTutorial();
            };
            _pauseService = pauseService;
        }

        private void Awake()
        {
            _continue.onClick.AddListener(() =>
            {
                PageSwitcher.Open(PageName.Stats).Forget();
                _pauseService.Play();
            });
        }

        private void OnDestroy()
        {
            _continue.onClick.RemoveAllListeners();
        }

        private void ShowNextTutorial()
        {
            if (_tutorialTexts.Count == 0 || _tutorialSprites.Count == 0) return;

            _currentIndex = (_currentIndex + 1) % Mathf.Min(_tutorialTexts.Count, _tutorialSprites.Count);

            _tutorialTextMesh.text = _tutorialTexts[_currentIndex];
            _tutorialImage.sprite = _tutorialSprites[_currentIndex];
        }
    }
}
