using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputData : ScriptableObject
{
    public KeyCode key;
    public PlayerController.PlayerAction actionType;
}

#if UNITY_EDITOR
public class CreateMenuElementData
{
    [MenuItem("Assets/Create/InputData")]
    public static void createInputData()
    {
        InputData asset = ScriptableObject.CreateInstance<InputData>();

        AssetDatabase.CreateAsset(asset, "Assets/MenuElementData.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}
#endif
