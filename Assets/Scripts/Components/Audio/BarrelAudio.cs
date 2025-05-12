using Components.Environment;
using UnityEngine;

[RequireComponent(typeof(ExplosiveBarrel))]
public class BarrelAudio : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _explodeClip;

	private ExplosiveBarrel _barrel;

	private void Awake()
	{
		_barrel = GetComponent<ExplosiveBarrel>();

		_barrel.OnExplode += PlayExplode;
	}

	private void OnDestroy()
	{
		_barrel.OnExplode -= PlayExplode;
	}

	private void PlayExplode()
	{
		_audioSource.PlayOneShot(_explodeClip);
	}
}
