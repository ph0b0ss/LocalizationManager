using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System;
using System.IO;
using Localization;

public class LanguageManagerWindow : EditorWindow
{
    

    GUISkin _defaultSkin;
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

    

    private ReorderableList rowList;
    private string _newKey = string.Empty;
    private string _newLanguage = string.Empty;
    private Vector2 _scrollPosition = Vector2.zero;
    private LanguageManager _manager;
    private int _selectedIndex = -1;

    List<ReorderableList> LanguageRL;


    [MenuItem("ph0b0ss/LanguageManager")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        LanguageManagerWindow window = (LanguageManagerWindow)EditorWindow.GetWindow(typeof(LanguageManagerWindow));
        window.initialize();
        window.Show();
        

    }

    void OnEnable()
    {
        initialize();
    }

    void initialize()
    {
        LanguageManager.instance.Load();
        
        AssetDatabase.Refresh();
        //List<string> languageNames = LanguageManager.instance.getLanguageNames();
        rowList = new ReorderableList(LanguageManager.instance._keys, typeof(List<string>), true, false, false, true);
        
        rowList.onReorderCallback = (ReorderableList list) =>
        {
            LanguageManager.instance.MoveKey(_selectedIndex, list.index);
        };

        rowList.onSelectCallback = (ReorderableList list) =>
        {
            _selectedIndex = list.index;
            foreach (ReorderableList rl in LanguageRL)
                rl.index = list.index;
        };

        rowList.onRemoveCallback = (ReorderableList list) =>
        {
            LanguageManager.instance.RemoveKey(list.list[list.index] as string);
        };
        

        rowList.headerHeight = 2;
        rowList.elementHeight = 30;
        //rowList.footerHeight = 0;

        LanguageRL = new List<ReorderableList>();
        syncLanguageRL();

    }

    void syncLanguageRL()
    {
        LanguageRL.Clear();
        for (int i = 0; i < LanguageManager.instance.languages.Count; i++)
        {

            ReorderableList currentRL = new ReorderableList(LanguageManager.instance.languages[i], typeof(List<string>), true, false, false, false);
            LanguageRL.Add(currentRL);
            currentRL.headerHeight = 2;
            currentRL.footerHeight = 0;
            currentRL.elementHeight = 30;
            currentRL.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                currentRL.list[index] = EditorGUI.TextField(rect, currentRL.list[index] as string, Skin.FindStyle("LanguageTextArea"));
            };
        }
    }

    void LanguageContexMenu(Rect elementRect,string language)
    {
        Event e = Event.current;
        if(e.type == EventType.ContextClick)
        {
            if(elementRect.Contains(e.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Hide Language"), false,LanguageManager.instance.HideLanguage, language);
                menu.ShowAsContext();
            }
        }
    }


    

    void OnGUI()
    {

        if (EditorApplication.isCompiling)
        {
            GUILayout.Label("COMPILING");
            return;
        }


        //_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUILayout.Height(300));

        GUILayout.BeginVertical(Skin.box);
            GUILayout.BeginHorizontal();
                //Titles
                GUILayout.BeginHorizontal(Skin.FindStyle("HeaderBackground"));
                    GUILayout.Label("Keys", Skin.FindStyle("Header"));
                    if (GUILayout.Button(" ", Skin.FindStyle("HideLanguage")))
                    {

                    }
                    if (GUILayout.Button(" ", Skin.FindStyle("HideLanguage")))
                    {

                    }
                GUILayout.EndHorizontal();
                for (int i = 0; i < LanguageRL.Count; i++)
                {
                    GUILayout.BeginHorizontal(Skin.FindStyle("HeaderBackground"));
                    GUILayout.Label(LanguageManager.instance.getLanguageNames()[i], Skin.FindStyle("Header"));
                    LanguageContexMenu(GUILayoutUtility.GetLastRect(), LanguageManager.instance.getLanguageNames()[i]);
                    if (GUILayout.Button("_", Skin.FindStyle("HideLanguage")))
                    {
                        LanguageManager.instance.HideLanguage(LanguageManager.instance.getLanguageNames()[i]);
                        LanguageRL.RemoveAt(i);
                        return;
                    }
                    if (GUILayout.Button("x", Skin.FindStyle("HideLanguage")))
                    {
                        string languageName = LanguageManager.instance.getLanguageNames()[i];
                        if(EditorUtility.DisplayDialog("Delete language", "Are you sure? That will delete the " + languageName + " file forever.","Delete"))
                        {
                            LanguageRL.RemoveAt(i);
                            LanguageManager.instance.DeleteLanguage(languageName);
                            return;
                        }
                        
                    }
                    GUILayout.EndHorizontal();
                }
            GUILayout.Space(14);
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                                rowList.DoLayoutList();
                    GUILayout.EndVertical();
                    for (int i = 0; i < LanguageRL.Count; i++)
                    {
                        GUILayout.BeginVertical();
                        LanguageRL[i].DoLayoutList();
                        GUILayout.EndVertical();
                    }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();

        GUILayout.EndVertical();
        
        

        // Add Key
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(Skin.box);
        _newKey = EditorGUILayout.TextField(_newKey);
        if (GUILayout.Button("Add Key"))
        {
            LanguageManager.instance.AddKey(_newKey);
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical(Skin.box);

        _newLanguage = EditorGUILayout.TextField(_newLanguage);

        if (GUILayout.Button("Add Language"))
        {
            LanguageManager.instance.AddLanguage(_newLanguage);
            syncLanguageRL();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        /*
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            LanguageManager.instance.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load"))
        {
            LanguageManager.instance.Load();
        }
        GUILayout.EndHorizontal();
        */
        /*
        // List
        if (rowList != null)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,false,true);
                rowList.DoLayoutList();
            GUILayout.EndScrollView();
        }

        
        */
    }
    /*
    void OnInspectorUpdate()
    {
        if (EditorWindow.focusedWindow == this &&
            EditorWindow.mouseOverWindow == this)
        {
            this.Repaint();
        }
    }*/

    void OnDestroy()
    {
        //EditorUtility.SetDirty(al);
        LanguageManager.instance.Save();
        AssetDatabase.Refresh();
        //AssetDatabase.SaveAssets();
    }

    


    

}
