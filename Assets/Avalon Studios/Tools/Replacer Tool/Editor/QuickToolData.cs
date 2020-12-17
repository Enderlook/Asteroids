using UnityEngine;

namespace AvalonStudios.Additions.Tools.ReplacerTool
{
    public class QuickToolData : ScriptableObject
    {
        public GameObject Object => replace;

        public string ReplaceName => replaceName;

        public string NameObject => nameObject;

        public GameObject[] ObjectsToReplace { get => objectsToReplace; set => objectsToReplace = value; }

        [SerializeField]
        private string nameObject = "";

        [SerializeField]
        private int amountOfObjectsToCreate = 1;

        [SerializeField]
        private GameObject replace = null;

        [SerializeField]
        private string replaceName = "";

        [SerializeField]
        private GameObject[] objectsToReplace = null;
    }
}
