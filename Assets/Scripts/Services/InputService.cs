using UnityEngine;

namespace Services
{
    public class InputService
    {
        public float Horizontal => Input.GetAxis("Horizontal");
        public float Vertical => Input.GetAxis("Vertical");
        public float MouseX => Input.GetAxis("Mouse X");
        public float MouseY => Input.GetAxis("Mouse Y");
        public bool Jump => Input.GetKeyDown(KeyCode.Space);
        public bool Fire => Input.GetKey(KeyCode.Mouse0);
        public bool FirePressed => Input.GetKeyDown(KeyCode.Mouse0);
        public bool Action => Input.GetKeyDown(KeyCode.E);
        public bool Escape => Input.GetKeyDown(KeyCode.Escape);
        public bool Crouch => Input.GetKey(KeyCode.LeftControl);
        public bool Sprint => Input.GetKey(KeyCode.LeftShift);
        public float ScrollWheel => Input.GetAxis("Mouse ScrollWheel");
        public bool Knife => Input.GetKeyDown(KeyCode.Alpha1);
        public bool Pistol => Input.GetKeyDown(KeyCode.Alpha2);
        public bool Rifle => Input.GetKeyDown(KeyCode.Alpha3);
        public bool Shotgun => Input.GetKeyDown(KeyCode.Alpha4);
        public bool GrenadeLauncher => Input.GetKeyDown(KeyCode.Alpha5);
    }
}
