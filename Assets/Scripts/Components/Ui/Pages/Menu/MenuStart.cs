using UnityEngine;

namespace Components.Ui.Pages.Menu
{
    public class MenuStart : BasePage
    {
        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
			Time.timeScale = 1f;
			Opened += () => { PageSwitcher.Open(PageName.Menu).Forget(); };
        }
    }
}