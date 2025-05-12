using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    public class MusicService : MonoBehaviour
    {
        [SerializeField]
        public AudioSource _audioSource;
        [SerializeField]
        public List<AudioClip> _musicList;

        private int _index = -1;

        public bool IsPlaying => _audioSource.isPlaying;

        public void PlayNext()
        {
            _index = (_index + 1) % _musicList.Count;
            _audioSource.clip = _musicList[_index];
            _audioSource.Play();
        }

        private void Update()
        {
            if (_musicList.Count == 0)
            {
                return;
            }
            
            if (!IsPlaying)
            {
                PlayNext();
            }
        }
    }
}