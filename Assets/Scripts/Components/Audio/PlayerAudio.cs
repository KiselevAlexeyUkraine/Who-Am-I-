using Components.Player;
using UnityEngine;

[RequireComponent(typeof(PlayerControls))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerAudio : MonoBehaviour
{
	[SerializeField]
	private AudioSource _audioSource;
	[SerializeField]
	private AudioSource _stepsSource;
	[SerializeField]
	private AudioClip _stepsClip;
	[SerializeField]
	private AudioClip _jumpClip;
	[SerializeField]
	private AudioClip _hitClip;
	[SerializeField]
	private AudioClip _healClip;
	[SerializeField]
	private AudioClip _deathClip;
	[SerializeField]
	private float _stepsInterval = 0.5f;
	[SerializeField]
	private float _minPitch = 0.9f;
	[SerializeField]
	private float _maxPitch = 1.1f;

	private PlayerControls _controls;
	private PlayerHealth _health;
	private float _timer;

	private void Awake()
	{
		_controls = GetComponent<PlayerControls>();
		_health = GetComponent<PlayerHealth>();

		_controls.OnMove += PlayMove;
		_controls.OnJump += PlayJump;
		_health.OnIncrease += PlayHeal;
		_health.OnDecrease += PlayHit;
		_health.OnDied += PlayDeath;
	}

	private void OnDestroy()
	{
		_controls.OnMove -= PlayMove;
		_controls.OnJump -= PlayJump;
		_health.OnIncrease -= PlayHeal;
		_health.OnDecrease -= PlayHit;
		_health.OnDied -= PlayDeath;
	}

	private void PlayMove()
	{
		if (_timer >= _stepsInterval)
		{
			_stepsSource.pitch = Random.Range(_minPitch, _maxPitch);
			_stepsSource.PlayOneShot(_stepsClip);
			_timer = 0f;
		}

		_timer += Time.deltaTime;
	}

	private void PlayDeath()
	{
		_audioSource.pitch = Random.Range(_minPitch, _maxPitch);
		_audioSource.PlayOneShot(_deathClip);
	}

	private void PlayJump()
	{
		_audioSource.pitch = Random.Range(_minPitch, _maxPitch);
		_audioSource.PlayOneShot(_jumpClip);
	}

	private void PlayHit()
	{
		_audioSource.pitch = Random.Range(_minPitch, _maxPitch);
		_audioSource.PlayOneShot(_hitClip);
	}

	private void PlayHeal()
	{
		_audioSource.pitch = Random.Range(_minPitch, _maxPitch);
		_audioSource.PlayOneShot(_healClip);
	}
}
