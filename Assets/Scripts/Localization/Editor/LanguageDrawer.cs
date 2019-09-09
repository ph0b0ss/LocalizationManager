using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using Localization;

[CustomPropertyDrawer(typeof(LanguageAttribute))]
public class LanguageDrawer : PropertyDrawer {

    int _selectedLanguage = 0;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        string[] _languages = LanguageManager.instance.getLanguageNames().ToArray();
        int[] _languagesID = new int[_languages.Length];
        for(int i = 0; i < _languages.Length; i++)
        {
            if(_languages[i] == property.stringValue)
            {
                _selectedLanguage = i;
            }
            _languagesID[i] = i;
        }
        _selectedLanguage = EditorGUI.IntPopup(position, _selectedLanguage, _languages, _languagesID);
        property.stringValue = _languages[_selectedLanguage];
    }
}
