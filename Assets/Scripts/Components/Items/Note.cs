using UnityEngine;
using System.Collections.Generic;

namespace Components.Items
{
    public class Note : MonoBehaviour
    {
        [Header("ID записки (уникальный индекс)")]
        [SerializeField] private int _noteId;
        public int NoteId => _noteId;

        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private bool _isCollected;
        public bool IsCollected => _isCollected;

        public static List<Note> AllNotes { get; private set; } = new List<Note>();

        private void Awake()
        {
            if (!AllNotes.Contains(this))
                AllNotes.Add(this);
        }

        private void OnDestroy()
        {
            AllNotes.Remove(this);
        }

        private void Start()
        {
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
            _boxCollider.enabled = false;
            _spriteRenderer.enabled = false;
        }
    }
}
