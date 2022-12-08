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
                // CaptureLastActiveSceneView(filePath);
                CaptureSceneViewWithComponents(filePath);
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

    private static void CaptureSceneViewWithComponents(string filePath)
    {
        // CAUTION:
        // UnityEditorInternal.InternalEditorUtility.ReadScreenPixel not consider the scaling by Windows-OS.

        var sceneView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.SceneView"));
            sceneView.Focus();

        var sceneViewPosition = sceneView.position.position;
        var sceneViewWidth    = (int)sceneView.position.width;
        var sceneViewHeight   = (int)sceneView.position.height;

        // NOTE:
        // There are some gap in captured image.

        const int frameHeight    = 45;
        const int noisyLeftFrame = 1;
        const int aLittleHeight  = 18;
        const int aLittleWidth   = 1;

        sceneViewPosition.x += noisyLeftFrame;
        sceneViewWidth      -= noisyLeftFrame;
        sceneViewWidth      += aLittleWidth;

        sceneViewPosition.y += frameHeight;
        sceneViewHeight     -= frameHeight;
        sceneViewHeight     += aLittleHeight;

        var pixels = UnityEditorInternal.InternalEditorUtility
            .ReadScreenPixel(sceneViewPosition, sceneViewWidth, sceneViewHeight);

        var texture = new Texture2D(sceneViewWidth, sceneViewHeight, TextureFormat.RGB24, false);
            texture.SetPixels(pixels);
            texture.Apply();

        File.WriteAllBytes(filePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }
}