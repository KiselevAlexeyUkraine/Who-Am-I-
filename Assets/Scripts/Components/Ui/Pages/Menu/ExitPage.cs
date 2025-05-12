using UnityEngine;
using UnityEngine.UI;

namespace Components.Ui.Pages.Menu
{
    public class ExitPage : BasePage
    {
        [SerializeField] private Button _accept;
        [SerializeField] private Button _cancel;

        private void Awake()
        {
            _accept.onClick.AddListener(Application.Quit);
            _cancel.onClick.AddListener(() =>
            {
                PageSwitcher.Open(PageName.Menu).Forget();
            });
        }

        private void OnDestroy()
        {
            _accept.onClick.RemoveAllListeners();
            _cancel.onClick.RemoveAllListeners();
        }
    }
}