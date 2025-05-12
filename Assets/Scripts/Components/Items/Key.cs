using UnityEngine;

namespace Components.Items
{
    public class Key : MonoBehaviour
    {
        [SerializeField] private KeyType _type;
        [SerializeField] private Transform _model;
		[SerializeField] private float _positionAmplitude;

        public KeyType Type => _type;

		private void Awake()
		{
			//Sequence.Create(-1, CycleMode.Yoyo)
			//	.Chain(Tween.LocalPositionY(_model, -_positionAmplitude, _positionAmplitude, 5f, Ease.InOutSine, startDelay: Mathf.Sin(_model.position.x)));
		}
	}
}