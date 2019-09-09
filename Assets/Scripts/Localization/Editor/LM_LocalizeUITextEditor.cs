using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Localization;

[CustomEditor(typeof(LM_LocalizeUIText))]
public class LM_LocalizeTextEditor : Editor
{

    string[] _keys;
    SerializedProperty languageSP;
    
    LM_LocalizeUIText _this
    {
        get { return target as LM_LocalizeUIText; }
    }

    GUISkin _skin;
    public GUISkin Skin
    {
        get
        {
            if (_skin == null)
            {
                //_skin = Resources.Load<GUISkin>("GUISkins/LanguageManagerSkin");
                _skin = (GUISkin)EditorGUIUtility.Load("GUISKins/LanguageManagerSkin.guiskin");
                if (_skin == null)
                    return GUI.skin;
            }

            return _skin;
        }
    }

    void OnEnable()
    {
        LanguageManager.instance.Load();
        _keys = LanguageManager.instance._keys.ToArray<string>();
        languageSP = serializedObject.FindProperty("language");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        int _selectedID = LanguageManager.instance.GetKeyIndex(_this._selectedkey);
        EditorGUILayout.BeginHorizontal();
            if (_selectedID != -1)
                GUILayout.Label(_keys[_selectedID], Skin.FindStyle("KeyTitle"));
            else
                GUILayout.Label(_this._selectedkey + " don´t exist", Skin.FindStyle("KeyTitle"));

        EditorGUILayout.EndHorizontal();

        if (_selectedID == -1)
            _selectedID = 0;
        EditorGUILayout.BeginHorizontal();
        
            _selectedID = EditorGUILayout.Popup(_selectedID, _keys);
            _this._selectedkey = _keys[_selectedID];
            //_newKey = EditorGUILayout.TextField(_newKey);
            if(GUILayout.Button("Open Language Manager"))
            {
                LanguageManagerWindow window = (LanguageManagerWindow)EditorWindow.GetWindow(typeof(LanguageManagerWindow));

                window.Show();
                /*
                LanguageManager.instance.AddKey(_newKey);
                _newKey = "";
                LanguageManager.instance.Save();
                _keys = LanguageManager.instance._keys.ToArray<string>();*/
            }


        /*
        _selectedLanguage = EditorGUILayout.IntPopup(_selectedLanguage, _languages, _languagesID);
        */

        EditorGUILayout.PropertyField(languageSP);
        _this.setKeyInLanguage(languageSP.stringValue);
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }

}
