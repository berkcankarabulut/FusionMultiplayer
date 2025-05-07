using UnityEngine;

namespace FPSGame.Weapons
{
    

    public class Sway : MonoBehaviour
    {
        [Header("Settings")] public float swayClamp = .09f;
        public float swaySmooth = .05f;

        private Vector3 origin;

        private void Start()
        {
            origin = transform.localPosition;
        }

        void Update()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            input.x = Mathf.Clamp(input.x, -swayClamp, swayClamp);
            input.y = Mathf.Clamp(input.y, -swayClamp, swayClamp);
            Vector3 target = new Vector3(-input.x, -input.y, 0);
            transform.localPosition =
                Vector3.Lerp(transform.localPosition, target + origin, swaySmooth * Time.deltaTime);
        }
    }

}