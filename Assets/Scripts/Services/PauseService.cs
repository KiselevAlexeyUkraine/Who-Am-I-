using System;
using UnityEngine;

namespace Services
{
    public class PauseService : MonoBehaviour
    {
        public event Action OnPlay; 
        public event Action OnPause; 
        
        public bool IsPaused { get; private set; }

        private void OnDestroy()
        {
            OnPlay = null;
            OnPause = null;
            
            IsPaused = false;
            Time.timeScale = 1f;
        }

        public void Toggle()
        {
            if (IsPaused)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }
        
        public void Play()
        {
            OnPlay?.Invoke();
            IsPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }

        public void Pause()
        {
            OnPause?.Invoke();
            IsPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }
}
