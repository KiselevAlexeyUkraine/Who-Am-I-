using UnityEngine;

namespace Components.Helpers
{
    public class SkyboxRotation : MonoBehaviour
    {
        [SerializeField] private Material _skybox;
        [SerializeField] private Light _sun;
        [SerializeField] private float _angleSpeed = 1f;
        [SerializeField] private Gradient _gradient;

        private float _angle;
        
        private static readonly int Tint = Shader.PropertyToID("_Tint");
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");

        private void Update()
        {
            _angle += _angleSpeed * Time.deltaTime;

            if (_angle > 360f)
            {
                _angle = 0f;
            }

            var color = _gradient.Evaluate(_angle / 360f);
            _skybox.SetColor(Tint, color);
            _skybox.SetFloat(Rotation, _angle);
            _sun.color = color;
            
            DynamicGI.UpdateEnvironment();
        }
    }
}
