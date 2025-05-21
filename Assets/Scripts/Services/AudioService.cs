using UnityEngine;

namespace Services
{
	public class AudioService : MonoBehaviour
	{
		[SerializeField] private AudioSource _audioSource;

		public void PlayOneShot(AudioClip clip)
		{
			_audioSource.PlayOneShot(clip);
		}
	}
}