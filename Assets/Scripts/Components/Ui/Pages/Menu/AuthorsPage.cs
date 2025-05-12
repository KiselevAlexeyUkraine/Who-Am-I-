using UnityEngine;
using UnityEngine.UI;

namespace Components.Ui.Pages.Menu
{
    public class AuthorsPage : BasePage
    {
        [SerializeField] private Button _back;

        private void Awake()
        {
            _back.onClick.AddListener(() => { PageSwitcher.Open(PageName.Menu).Forget(); });
        }

        private void OnDestroy()
        {
            _back.onClick.RemoveAllListeners();
        }
    }
}