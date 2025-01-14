using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Text;

public class HierarchyExporter : EditorWindow
{
    [MenuItem("Tools/Hierarchy Exporter")]
    private static void ShowWindow()
    {
        GetWindow<HierarchyExporter>("Hierarchy Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export the current scene's hierarchy to a text file.", EditorStyles.boldLabel);

        if (GUILayout.Button("Export Hierarchy"))
        {
            ExportHierarchy();
        }
    }

    private void ExportHierarchy()
    {
        // Gets the currently active scene
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("No valid scene is open.");
            return;
        }

        // Build a string of the hierarchy
        StringBuilder sb = new StringBuilder();
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            AppendGameObjectHierarchy(sb, rootObj, 0);
        }

        // Choose where to save the file
        string path = EditorUtility.SaveFilePanel("Save Hierarchy", "", scene.name + "_Hierarchy.txt", "txt");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, sb.ToString());
            Debug.Log("Hierarchy exported to: " + path);
        }
    }

    // Recursively append children
    private void AppendGameObjectHierarchy(StringBuilder sb, GameObject obj, int indentLevel)
    {
        sb.AppendLine(new string('-', indentLevel * 2) + " " + obj.name);

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            AppendGameObjectHierarchy(sb, obj.transform.GetChild(i).gameObject, indentLevel + 1);
        }
    }
}