using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityToolbarExtender;
#endif

[InitializeOnLoad]
public class GameViewCaptureButton
{
    private static readonly GUIContent SceneViewContent;
    private static readonly GUIContent GameViewContent;

    static GameViewCaptureButton()
    {
        SceneViewContent = new GUIContent("Ⓢ", "Capture Scene View");
        GameViewContent  = new GUIContent("Ⓖ", "Capture Game View");
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace(); // To align right.

        if (GUILayout.Button(SceneViewContent, EditorStyles.toolbarButton, GUILayout.Width(33)))
        {
            CaptureView(false);
        }

        if (GUILayout.Button(GameViewContent, EditorStyles.toolbarButton, GUILayout.Width(33)))
        {
            CaptureView();
        }
    }

    private static void CaptureView(bool gameView = true)
    {
        var timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var filePath  = EditorUtility.SaveFilePanel("Screenshot", "", timestamp, "png");

        if (!string.IsNullOrEmpty(filePath))
        {
            if (gameView)
            {
                ScreenCapture.CaptureScreenshot(filePath);
            }
            else
            {
                CaptureLastActiveSceneView(filePath);
            }
        }

        // CAUTION:
        // To avoid "EndLayoutGroup: BeginLayoutGroup must be called first" error.
        // https://forum.unity.com/threads/endlayoutgroup-beginlayoutgroup-must-be-called-first.523209/
        // This maybe caused by Unity's bug.
        GUIUtility.ExitGUI();
    }

    private static void CaptureLastActiveSceneView(string filePath)
    {
        SceneView.lastActiveSceneView.camera.Render(); // Force render is important X(

        var renderTexture = SceneView.lastActiveSceneView.camera.activeTexture;

        RenderTexture.active = renderTexture;

        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

        File.WriteAllBytes(filePath, texture.EncodeToPNG());

        Object.DestroyImmediate(texture);
    }
}