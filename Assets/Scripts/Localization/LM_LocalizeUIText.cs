using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Localization;

[RequireComponent(typeof(Text))]
public class LM_LocalizeUIText : MonoBehaviour
{
    [SerializeField]
    Text _text;

    [Language]
    public string language;

    public string _selectedkey = string.Empty;

    public Text text
    {
        get
        {
            if (_text != null)
                return _text;
            _text = GetComponent<Text>();
            return _text;
        }
    }
    void Start()
    {
        LanguageManager.instance.OnLanguageChange += LanguageManager_OnLanguageChange;
        LanguageManager.instance.OnInitializate += LanguageManager_OnInitializate;
        translate();
    }

    void OnDestroy()
    {
        LanguageManager.instance.OnLanguageChange -= LanguageManager_OnLanguageChange;
        LanguageManager.instance.OnInitializate -= LanguageManager_OnInitializate;
    }

    private void LanguageManager_OnInitializate()
    {
        translate();
    }

    private void LanguageManager_OnLanguageChange()
    {
        translate();
    }

    void translate()
    {
        text.text = LanguageManager.instance.GetText(_selectedkey);
    }

    public void setKeyText(string t)
    {
        _selectedkey = t;
        translate();
    }
	
    public void setKeyInLanguage(string language)
    {
        text.text = LanguageManager.instance.GetText(_selectedkey, language);
    }
}
