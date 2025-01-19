using UnityEngine;
using UnityEditor;

public class TimeScaleSliderWindow : EditorWindow
{
    // Cached timescale value.
    private float timeScale = 1f;

    // Add a menu item under "Window" to open the tool.
    [MenuItem("Window/TimeScale Slider")]
    public static void ShowWindow()
    {
        // Create the window or focus it if it already exists.
        TimeScaleSliderWindow window = GetWindow<TimeScaleSliderWindow>("TimeScale");
        window.minSize = new Vector2(200, 50);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Time Scale", EditorStyles.boldLabel);

        // Use a slider to adjust the timescale value in the range 0 to 2.
        EditorGUI.BeginChangeCheck();
        timeScale = EditorGUILayout.Slider(timeScale, 0f, 2f);
        if (EditorGUI.EndChangeCheck())
        {
            // Immediately update Unity's Time.timeScale.
            Time.timeScale = timeScale;
            // Optionally, request an update/redraw (especially helpful while in play mode).
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}