using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

[CustomEditor(typeof(MainMenu))]
public class MainMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MainMenu menu = (MainMenu)target;

        // Get all scenes from Build Settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] sceneNames = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = Path.GetFileNameWithoutExtension(path);
        }

        // Dropdown
        if (sceneCount > 0)
        {
            menu.startSceneIndex = EditorGUILayout.Popup("Start Scene", menu.startSceneIndex, sceneNames);
        }
        else
        {
            EditorGUILayout.HelpBox("No scenes in Build Settings! Add some in File → Build Settings.", MessageType.Warning);
        }

        DrawDefaultInspectorExcept("startSceneIndex");
    }

    private void DrawDefaultInspectorExcept(string exclude)
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (iterator.name == exclude) continue;
            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
