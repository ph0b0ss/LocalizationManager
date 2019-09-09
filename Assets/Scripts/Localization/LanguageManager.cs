using UnityEngine;
#if UNITY_PS4
using UnityEngine.PS4;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Localization
{
    public class LanguageRow
    {
        public string Key;
        public List<string> Values;

        public LanguageRow(string key)
        {
            Key = key;
            Values = new List<string>();
        }

        public string getValue(int id)
        {
            return Values[id];
        }

        public void setValue(int id, string value)
        {
            Values[id] = value;
        }

        public void addLanguage(string defaultValue = "")
        {
            Values.Add(defaultValue);
        }

        public string ToCSV()
        {
            string outValue = Key;
            foreach (string s in Values)
            {
                outValue += "," + s;
            }
            return outValue;
        }
    }

    public class Language
    {
        public string Name;
        public List<string> Values;

        public Language(string name, List<string> values)
        {
            Name = name;
            Values = values;
        }

        public Language(string name, int keySize)
        {
            Name = name;
            Values = new List<string>();
            for (int i = 0; i < keySize; i++)
                Values.Add(string.Empty);
        }

        public string[] ToCSV()
        {
            return Values.ToArray<string>();
        }
    }

    public class LanguageManager
    {
        public static string DEFAULT_LOCALIZATION_PATH = "/Resources/Localization/";
        public static string DEFAULT_LOCALIZATION_PATH_RESOURCES = "Localization/";
        public static string DEFAULT_LANGUAGES_PATH = "Languages/";
        public static string KEYS_FILE = "Keys.csv";
        public static string KEYS_FILE_RESOURCES = "Keys";
        public static string DEFAULT_KEY_HEADER = "Key";
        public static string LANGUAGES_FILE = "Localization.csv";
        public static string HEADER_KEY = "$Key";
        public static int MAX_KEYS = 1000;
        public static string SEARCH_PATTERN = "*.csv";
        public static string DEFAULT_EXTENSION = ".csv";
        public static string[] LINE_SEPARATORS = { "\n", "\r", "\r\n" };


        public List<string> _keys = new List<string>();
        public List<LanguageRow> _data = new List<LanguageRow>();
        public List<List<string>> languages = new List<List<string>>();
        private Dictionary<string, Language> _languagesDict = new Dictionary<string, Language>();
        private List<string> _languageNames = new List<string>();
        private Language _currentLanguage = null;


        public LanguageManager()
        {
            LoadKeys();
        }

        public delegate void OnLanguageChangeHandler();
        public event OnLanguageChangeHandler OnLanguageChange;
        public delegate void OnLanguageInitHandler();
        public event OnLanguageInitHandler OnInitializate;

        static LanguageManager _instance = null;
        public static LanguageManager instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                _instance = new LanguageManager();
                return _instance;
            }
        }

        #region Editor methods
        public void Save()
        {
            WriteKeysToFile();
            WriteAllLanguagesToFile();
        }

        public void Load()
        {
            CleanData();
            ReadKeysFromFile();
            ReadAllLanguagesFromFile();
            CheckLanguageConsistency();
        }

        public void setLanguage(string language)
        {
            ReadLanguageFromFile(language);
            _currentLanguage = _languagesDict[language];
        }

        public List<string> getLanguageNames()
        {
            if(_languageNames.Count == 0)
                Load();
            return _languageNames;
        }


        public List<string> getLanguageValues(string languageName)
        {
            Language value;
            if (_languagesDict.TryGetValue(languageName, out value))
            {
                return value.Values;
            }
            LogManager.Log("No language with name : " + languageName, LogManager.LevelType.Error);
            return null;
        }

        public void setLanguageValues(string languageName, List<string> values)
        {
            if (_languagesDict.ContainsKey(languageName))
            {
                _languagesDict[languageName].Values = values;
            }
        }

        public void MoveKey(int from, int to)
        {
            foreach (KeyValuePair<string, Language> pair in _languagesDict)
            {
                string element = pair.Value.Values[from];
                pair.Value.Values.RemoveAt(from);
                pair.Value.Values.Insert(to, element);
            }
        }


        int getKeyId(string key)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i] == key)
                    return i;
            }
            return -1;
        }

        public void AddKey(string key)
        {
            if (!_keys.Contains(key))
            {
                _keys.Add(key);
                foreach (KeyValuePair<string, Language> pair in _languagesDict)
                {
                    pair.Value.Values.Add(string.Empty);
                }
            }
            else
                LogManager.Log("[Addkey] Key : " + key + " already in file. Aborting", LogManager.LevelType.Error);
        }

        public void RemoveKey(string key)
        {
            if (_keys.Contains(key))
            {
                int removeID = getKeyId(key);
                _keys.RemoveAt(removeID);
                foreach (KeyValuePair<string, Language> pair in _languagesDict)
                {
                    pair.Value.Values.RemoveAt(removeID);
                }
            }
        }

        public void AddLanguage(string language)
        {
            if (!_languagesDict.ContainsKey(language))
            {
                _languagesDict.Add(language, new Language(language, _keys.Count));
            }
            else
            {
                LogManager.Log("[AddLanguage] Language : " + language + " already in file. Aborting", LogManager.LevelType.Error);
            }
            UpdateLanguageLists();
        }

        public void HideLanguage(object obj)
        {
            string language = obj as string;
            _languagesDict.Remove(language);
            UpdateLanguageLists();
        }

        public void DeleteLanguage(object obj)
        {
            string language = obj as string;
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + DEFAULT_LANGUAGES_PATH + language + DEFAULT_EXTENSION;
            _languagesDict.Remove(language);
            UpdateLanguageLists();
            File.Delete(path);
        }

        int getLanguageID(string language)
        {
            return _languageNames.IndexOf(language);
        }

        void UpdateLanguageLists()
        {
            languages.Clear();
            _languageNames.Clear();
            foreach (KeyValuePair<string, Language> kv in _languagesDict)
            {
                languages.Add(kv.Value.Values);
                _languageNames.Add(kv.Key);
            }
        }

        void CheckLanguageConsistency()
        {
            foreach (KeyValuePair<string, Language> kv in _languagesDict)
            {
                int languageSize = kv.Value.Values.Count;
                List<string> correctedList;
                if (languageSize == _keys.Count)
                {
                    continue;
                }
                else if (languageSize > _keys.Count)
                {
                    correctedList = kv.Value.Values.GetRange(0, _keys.Count);
                    LogManager.Log("Language: " + kv.Key + " has more words than keys in our manager, it got trimmed", LogManager.LevelType.Error);
                }
                else
                {
                    correctedList = kv.Value.Values;
                    for (int i = 0; i < _keys.Count - languageSize; i++)
                    {
                        correctedList.Add(string.Empty);
                    }
                    LogManager.Log("Language : " + kv.Key + " has less words than keys in our manager, it got filled with empty values", LogManager.LevelType.Warning);
                }
                kv.Value.Values = correctedList;
            }
            UpdateLanguageLists();
        }
        #endregion

        #region Runtime methods

        public string GetText(string key)
        {
            if (_currentLanguage != null)
            {
                int index = GetKeyIndex(key);
                if (index >= 0)
                {
                    return _currentLanguage.Values[index];
                }
                LogManager.Log("There is a text that isn´t localized with the key: " + key + " in language " + _currentLanguage, LogManager.LevelType.Error);

            }
            LogManager.Log("There is no language selected to translate the key " + key, LogManager.LevelType.Warning);
            return key;
        }

        public string GetText(string key,string language)
        {
            int index = GetKeyIndex(key);
            
            return getLanguageValues(language)[index];
        }

        public int GetKeyIndex(string key)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i] == key)
                    return i;
            }
            return -1;
        }
        #endregion

        #region IO operations
        bool ReadKeysFromFile()
        {

            // Remember to close Visual studio if you get the IO error
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + KEYS_FILE;
            TextAsset keys = Resources.Load<TextAsset>(DEFAULT_LOCALIZATION_PATH_RESOURCES + KEYS_FILE_RESOURCES);

#if UNITY_EDITOR
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Application.dataPath + DEFAULT_LOCALIZATION_PATH);
                FileStream f = File.Create(path);
                f.Close();
                LogManager.Log("[Read]", "Keys file don´t found.Creating a keys file.", LogManager.LevelType.Warning);
                return true;
            }
#endif
            
            _keys = keys.text.Split(LINE_SEPARATORS,System.StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            //_keys = File.ReadAllLines(path).ToList<string>();
            return false;
        }


        void ReadAllLanguagesFromFile()
        {

            
            TextAsset[] languages = Resources.LoadAll<TextAsset>(DEFAULT_LOCALIZATION_PATH_RESOURCES + DEFAULT_LANGUAGES_PATH);
            foreach(TextAsset language in languages)
            {
                ReadLanguageFromFile(language.name);
            }
#if UNITY_EDITOR
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + DEFAULT_LANGUAGES_PATH;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            LogManager.Log("Reading all language files from " + Path.GetFullPath(path), LogManager.LevelType.Info);
#endif

        }

        void ReadLanguageFromFile(string languageName)
        {
            
            TextAsset language = Resources.Load<TextAsset>(DEFAULT_LOCALIZATION_PATH_RESOURCES + DEFAULT_LANGUAGES_PATH+languageName);
#if UNITY_EDITOR
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + DEFAULT_LANGUAGES_PATH + languageName + DEFAULT_EXTENSION;
            
            if (!File.Exists(path))
            {
                LogManager.Log("[Read]", "Language file: " + languageName + " don´t found.Skipping", LogManager.LevelType.Error);
                return;
            }
#endif
            List<string> lines =language.text.Split(LINE_SEPARATORS, System.StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (_languagesDict.ContainsKey(languageName))
            {
                _languagesDict[languageName].Values = lines;
            }
            else
            {
                _languagesDict.Add(languageName, new Language(languageName, lines));
            }
        }

        void WriteKeysToFile()
        {
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + KEYS_FILE;
            File.WriteAllLines(path, _keys.ToArray<string>());
        }

        void WriteAllLanguagesToFile()
        {
            foreach (KeyValuePair<string, Language> pair in _languagesDict)
            {
                WriteLanguageToFile(pair.Key);
            }
        }

        void WriteLanguageToFile(string languageName)
        {
            string path = Application.dataPath + DEFAULT_LOCALIZATION_PATH + DEFAULT_LANGUAGES_PATH + languageName + DEFAULT_EXTENSION;
            Language currentLanguage;
            if (_languagesDict.TryGetValue(languageName, out currentLanguage))
            {
                File.WriteAllLines(path, currentLanguage.ToCSV());
            }
            else
            {
                LogManager.Log("[Write]", "Language file :" + languageName + " don´t found. Skipping", LogManager.LevelType.Error);
                return;
            }
        }
        #endregion

        #region Resources Load
        bool LoadLenguage(string language)
        {
            TextAsset file = Resources.Load(DEFAULT_LOCALIZATION_PATH_RESOURCES + DEFAULT_LANGUAGES_PATH + language) as TextAsset;
            if (file != null)
            {
                List<string> lines = file.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None).ToList<string>();
                if (_languagesDict.ContainsKey(language))
                {
                    _languagesDict[language].Values = lines;
                }
                else
                {
                    _languagesDict.Add(language, new Language(language, lines));
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool LoadKeys()
        {
            return ReadKeysFromFile();
        }
        #endregion

        void CleanData()
        {
            _keys.Clear();
            _languagesDict.Clear();
        }

    }
}
