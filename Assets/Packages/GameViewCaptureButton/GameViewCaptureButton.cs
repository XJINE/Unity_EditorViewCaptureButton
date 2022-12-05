using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityToolbarExtender;
#endif

[InitializeOnLoad]
public class GameViewCaptureButton
{
    private static readonly GUIContent Content;

    static GameViewCaptureButton()
    {
        Content = new GUIContent("❖", "Capture Game View"); // ex:icons ❖▩★
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace(); // To align right.

        if (GUILayout.Button(Content, EditorStyles.toolbarButton, GUILayout.Width(33)))
        {
            CaptureScreen(1);
        }
    }

    public static void CaptureScreen(int superSize)
    {
        var timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var filePath  = EditorUtility.SaveFilePanel("Screenshot", "", timestamp, "png");

        if (!string.IsNullOrEmpty(filePath))
        {
            ScreenCapture.CaptureScreenshot(filePath, superSize);
        }

        // CAUTION:
        // To avoid "EndLayoutGroup: BeginLayoutGroup must be called first" error.
        // https://forum.unity.com/threads/endlayoutgroup-beginlayoutgroup-must-be-called-first.523209/
        // This maybe caused by Unity's bug.
        GUIUtility.ExitGUI();
    }
}