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
        public bool Action => Input.GetKeyDown(KeyCode.E);
        public bool Light => Input.GetKeyDown(KeyCode.F);
        public bool Escape => Input.GetKeyDown(KeyCode.Escape);
        public bool Crouch => Input.GetKey(KeyCode.LeftControl);
        public bool Sprint => Input.GetKey(KeyCode.LeftShift);
        public bool MedicineChest => Input.GetKeyDown(KeyCode.Alpha1);
        public bool ReloadBatarey => Input.GetKeyDown(KeyCode.R);
        public bool Rifle => Input.GetKeyDown(KeyCode.Alpha3);
        public bool Shotgun => Input.GetKeyDown(KeyCode.Alpha4);
    }
}
