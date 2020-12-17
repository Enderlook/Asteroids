using AvalonStudios.Additions.Attributes.StylizedGUIs;

using System.Linq;
using UnityEngine;
using UnityEditor;

namespace AvalonStudios.Additions.Tools.ReplacerTool
{
    public class QuickTool : EditorWindow
    {
        private QuickToolData data;
        private SerializedObject serializedData;
        private SerializedProperty replace;
        private SerializedProperty replaceName;
        private SerializedProperty nameObject;
        private SerializedProperty amountOfObjectsToCreate;

        private Vector2 selectedObjectScrollPosition;
        private bool showError = false;
        private bool replaceInProgress = false;

        [MenuItem("Avalon Studios/Tools/Quick Tool")]
        public static void ShowWindow() => GetWindow<QuickTool>("Quick Tool");

        private void Initialize()
        {
            if (data == null)
            {
                data = CreateInstance<QuickToolData>();
                serializedData = null;
            }

            if (serializedData == null)
            {
                serializedData = new SerializedObject(data);
                replace = null;
                replaceName = null;
                nameObject = null;
                amountOfObjectsToCreate = null;
            }

            if (replace == null && replaceName == null)
            {
                replace = serializedData.FindProperty("replace");
                replaceName = serializedData.FindProperty(nameof(replaceName));
                nameObject = serializedData.FindProperty(nameof(nameObject));
                amountOfObjectsToCreate = serializedData.FindProperty(nameof(amountOfObjectsToCreate));
            }
        }

        private void OnGUI()
        {
            Initialize();
            serializedData.Update();
            GUILayout.Space(15);
            GUIStyle styles = GUIStylesConstants.TitleStyle(16);
            EditorGUILayout.LabelField("Quicks Tool", styles);
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(nameObject, new GUIContent("Name Object", "Object name to create."));
            EditorGUILayout.PropertyField(replace, new GUIContent("Replace Object For"));
            EditorGUILayout.PropertyField(replaceName, new GUIContent("Replace Object Name For", "Replace the name of the object."));

            EditorGUILayout.Separator();

            int objectToReplaceCount = data.ObjectsToReplace != null ? data.ObjectsToReplace.Length : 0;

            EditorGUI.indentLevel++;
            if (objectToReplaceCount == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox("Select a object or objects in hierarchy to replace them.", MessageType.Warning);
                EditorGUILayout.HelpBox("Select a object or objects in hierarchy to replace his names.", MessageType.Warning);
            }

            selectedObjectScrollPosition = EditorGUILayout.BeginScrollView(selectedObjectScrollPosition);
            GUI.enabled = false;
            if (data != null && data.ObjectsToReplace != null)
            {
                foreach (GameObject obj in data.ObjectsToReplace)
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();

            if (showError)
                EditorGUILayout.HelpBox("Missing prefab to replace with!", MessageType.Error);

            if (replaceInProgress)
                EditorGUILayout.HelpBox("Finish task!", MessageType.Info);

            if (GUILayout.Button("Replace Objects"))
            {
                if (!replace.objectReferenceValue)
                {
                    showError = true;
                    Debug.LogErrorFormat("Missing prefab to replace with!");
                    return;
                }
                showError = false;
                ReplaceSelectedObjects(data.ObjectsToReplace, data.Object);
            }

            if (GUILayout.Button("Replace Object Names"))
            {
                ReplaceSelectedObjectNames(data.ObjectsToReplace, replaceName.stringValue);
            }

            if (GUILayout.Button("Create Object"))
            {
                CreateObjects(nameObject.stringValue, amountOfObjectsToCreate.intValue);
            }

            if (GUILayout.Button("Delete Objects"))
            {
                DeleteObjects(data.ObjectsToReplace);
            }
            EditorGUILayout.Separator();
            serializedData.ApplyModifiedProperties();
        }

        private void OnInspectorUpdate()
        {
            if (serializedData != null && serializedData.UpdateIfRequiredOrScript())
                Repaint();
        }

        private void OnSelectionChange()
        {
            Initialize();
            SelectionMode objectFilter = SelectionMode.Unfiltered ^ ~(SelectionMode.Assets | SelectionMode.DeepAssets | SelectionMode.Deep);
            Transform[] selections = Selection.GetTransforms(objectFilter);

            data.ObjectsToReplace = selections.Select(s => s.gameObject).ToArray();

            if (serializedData.UpdateIfRequiredOrScript())
                Repaint();
        }

        private void ReplaceSelectedObjects(GameObject[] objectsToReplace, GameObject replaceObjectFor)
        {
            foreach (GameObject obj in objectsToReplace)
            {
                GameObject go = obj;
                Undo.RegisterCompleteObjectUndo(go, "Saving game object state");

                GameObject newObj = Instantiate(replaceObjectFor);
                newObj.transform.position = go.transform.position;
                newObj.transform.rotation = go.transform.rotation;
                newObj.transform.localScale = go.transform.localScale;
                Undo.RegisterCreatedObjectUndo(newObj, "Replacement creation.");

                foreach (Transform child in go.transform)
                    Undo.SetTransformParent(child, newObj.transform, "Parent Change");
                Undo.DestroyObjectImmediate(go);
            }
            replaceInProgress = true;
        }

        private void ReplaceSelectedObjectNames(GameObject[] objectsToReplace, string name)
        {
            foreach (GameObject obj in objectsToReplace)
                obj.name = name;
            replaceInProgress = true;
        }

        private void CreateObjects(string name, int amount)
        {
            for (int x = 0; x < amount; x++)
            {
                GameObject newObj = new GameObject();
                newObj.name = name;
            }
        }

        private void DeleteObjects(GameObject[] objectsToDelete)
        {
            foreach (GameObject obj in objectsToDelete)
                DestroyImmediate(obj);
        }
    }
}
