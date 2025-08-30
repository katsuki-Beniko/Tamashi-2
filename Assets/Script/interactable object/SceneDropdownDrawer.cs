#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(SceneDropdownAttribute))]
public class SceneDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [SceneDropdown] with strings only.");
            return;
        }

        // Get all scenes in build settings
        var sceneNames = GetScenesInBuildSettings();
        
        if (sceneNames.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "No scenes in Build Settings");
            return;
        }

        // Find current selection index
        string currentValue = property.stringValue;
        int selectedIndex = System.Array.IndexOf(sceneNames, currentValue);
        
        // If current value isn't in the list, add it as "Missing Scene"
        if (selectedIndex == -1 && !string.IsNullOrEmpty(currentValue))
        {
            var tempList = sceneNames.ToList();
            tempList.Insert(0, $"{currentValue} (Missing)");
            sceneNames = tempList.ToArray();
            selectedIndex = 0;
        }
        else if (selectedIndex == -1)
        {
            selectedIndex = 0;
        }

        // Draw the dropdown
        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, sceneNames);
        
        if (EditorGUI.EndChangeCheck())
        {
            // Update the property value
            string selectedScene = sceneNames[selectedIndex];
            
            // Remove " (Missing)" suffix if present
            if (selectedScene.EndsWith(" (Missing)"))
            {
                selectedScene = selectedScene.Replace(" (Missing)", "");
            }
            
            property.stringValue = selectedScene;
        }
    }
    
    private string[] GetScenesInBuildSettings()
    {
        List<string> sceneNames = new List<string>();
        
        // Add empty option
        sceneNames.Add("None");
        
        // Get all scenes from build settings
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                sceneNames.Add(sceneName);
            }
        }
        
        return sceneNames.ToArray();
    }
}
#endif
