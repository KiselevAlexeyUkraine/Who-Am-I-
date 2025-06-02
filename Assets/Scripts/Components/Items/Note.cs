using UnityEngine;

namespace Components.Items
{
    public class Note : MonoBehaviour
    {
        [Header("ID записки (уникальный индекс)")]
        [SerializeField] private int _noteId;
        public int NoteId => _noteId;

        private bool _isCollected;
        public bool IsCollected => _isCollected;

        private void Start()
        {
            // Если записка уже была сохранена как собранная — отключаем
            if (PlayerPrefs.GetInt($"Note_{_noteId}", 0) == 1)
            {
                _isCollected = true;
                gameObject.SetActive(false);
            }
        }

        public void Collect()
        {
            if (_isCollected)
                return;

            _isCollected = true;
            gameObject.SetActive(false);
        }
    }
}
