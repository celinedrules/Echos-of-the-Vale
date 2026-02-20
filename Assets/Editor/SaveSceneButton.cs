#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEditor.SceneManagement;

[Overlay(typeof(SceneView), "Save All")]
public class SaveAllOverlay : ToolbarOverlay
{
    public SaveAllOverlay() : base(SaveAllButton.ID) { }
}

[EditorToolbarElement(ID, typeof(SceneView))]
public class SaveAllButton : EditorToolbarButton
{
    public const string ID = "BitWaveLabs/SaveAllButton";

    private static SaveAllButton _instance;

    public SaveAllButton()
    {
        text = "💾 Save All";
        tooltip = "Save all open scenes and assets (Ctrl+Shift+S)";
        clicked += SaveAll;

        // Keep reference so we can toggle interactability
        _instance = this;
        EditorApplication.update += UpdateState;
    }

    private static void SaveAll()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("✅ All scenes and assets saved.");
        _instance.SetEnabled(false);
    }

    private static void UpdateState()
    {
        if (_instance == null)
            return;

        bool hasUnsavedScene = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            if (EditorSceneManager.GetSceneAt(i).isDirty)
            {
                hasUnsavedScene = true;
                break;
            }
        }

        _instance.SetEnabled(hasUnsavedScene);
    }
}
#endif