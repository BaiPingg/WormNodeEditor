using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class WormNodeGlobalSettings : ScriptableObject
{
    public const string k_MyCustomSettingsPath = "Assets/wormNode/Editor/WormNodeGlobalSettings.asset";

    [SerializeField]
    public Color colorbg;
    [SerializeField]
    public Color colorInPort;
    [SerializeField]
    public Color colorOutPort;
    [SerializeField]
    public Color colorPortCircle;
    [SerializeField]
    public float nodeBaseWidth ;
    [SerializeField]
    public float columnHeight;
    [SerializeField]
    public int  contetntSize;
    [SerializeField]
    public Color contentColor;
    [SerializeField]
    public Color linetColor;

    [SerializeField]
    public int titleSize;

    internal static WormNodeGlobalSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<WormNodeGlobalSettings>(k_MyCustomSettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<WormNodeGlobalSettings>();
            settings.colorbg = Color.gray;
            settings.colorInPort = Color.green;
            settings.colorOutPort = Color.blue;
            settings.colorPortCircle = Color.grey;
            settings.nodeBaseWidth = 250f;          
            settings.contetntSize = 20;
            settings.titleSize = 25;
            settings.contentColor = Color.white;
            settings.linetColor = Color.red;
            AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}

/// <summary>
/// 创建Preferences菜单
/// </summary>
static class MyCustomSettingsIMGUIRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
       
        var provider = new SettingsProvider("Preferences/wormSetting", SettingsScope.User)
        {
          label = "worm Setting",
            guiHandler = (searchContext) =>
            {
                var settings = WormNodeGlobalSettings.GetSerializedSettings();

                
                EditorGUILayout.PropertyField(settings.FindProperty("colorbg"), new GUIContent("colorbg"));
                EditorGUILayout.PropertyField(settings.FindProperty("colorInPort"), new GUIContent("colorInPort"));
                EditorGUILayout.PropertyField(settings.FindProperty("colorOutPort"), new GUIContent("colorOutPort"));
                EditorGUILayout.PropertyField(settings.FindProperty("colorPortCircle"), new GUIContent("colorPortCircle"));
                EditorGUILayout.PropertyField(settings.FindProperty("nodeBaseWidth"), new GUIContent("nodeBaseWidth（基本宽度）"));
                EditorGUILayout.PropertyField(settings.FindProperty("columnHeight"), new GUIContent("columnHeight（列高）"));
                EditorGUILayout.PropertyField(settings.FindProperty("contetntSize"), new GUIContent("contetntSize"));
                EditorGUILayout.PropertyField(settings.FindProperty("titleSize"), new GUIContent("titleSize"));
                EditorGUILayout.PropertyField(settings.FindProperty("contentColor"), new GUIContent("contentColor"));
                 EditorGUILayout.PropertyField(settings.FindProperty("linetColor"), new GUIContent("linetColor"));
                settings.ApplyModifiedProperties();
            },

            keywords = new HashSet<string>(new[] { "colorbg", })
        };

        return provider;
    }
}

