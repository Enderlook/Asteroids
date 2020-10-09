using UnityEngine;

namespace AvalonStudios.Additions.Utils.InputsManager
{
    [System.Serializable]
    public class KeyInputManager
    {
        [SerializeField, Tooltip("Amount of inputs that can be activated.")]
        private KeyCode[] keys = null;

        [SerializeField, 
        Tooltip("How the input behaves.\n" +
            "\n" +
            "On: The inputs can be kept activated if the key/s are kept pressed.\n" +
            "\n" +
            "Off: The input will be activated each time the key is pressed.")]
        private bool canBeHoldDown = false;

        public bool Execute()
        {
            bool isTrigger = false;
            foreach(KeyCode key in keys)
                isTrigger = canBeHoldDown ? Input.GetKey(key) : Input.GetKeyDown(key);

            return isTrigger;
        }
    }
}
