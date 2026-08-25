using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    static LocalizationManager _instance;

    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("LocalizationManager");
                _instance = go.AddComponent<LocalizationManager>();
            }
            return _instance;
        }
    }

    public event System.Action onLanguageChanged;

    LocalizationData _current;
    int _languageIndex = -1;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        int saved = PlayerPrefs.GetInt("Language", 0);
        LoadLocale(saved);
        _languageIndex = saved;
    }

    public void SetLanguage(int index)
    {
        if (index == _languageIndex && _current != null)
            return;

        PlayerPrefs.SetInt("Language", index);
        PlayerPrefs.Save();
        _languageIndex = index;
        LoadLocale(index);

        onLanguageChanged?.Invoke();
    }

    void LoadLocale(int index)
    {
        string locale = index == 1 ? "en" : "id";
        var asset = Resources.Load<TextAsset>("Locales/" + locale);
        if (asset == null)
        {
            Debug.LogWarning($"[LocalizationManager] File 'Locales/{locale}' tidak ditemukan di folder Resources.");
            return;
        }

        var data = JsonUtility.FromJson<LocalizationData>(asset.text);
        if (data == null)
        {
            Debug.LogWarning($"[LocalizationManager] Gagal parse 'Locales/{locale}'.");
            return;
        }

        _current = data;
    }

    public static string Get(string key)
    {
        var data = Instance._current;
        if (data == null || string.IsNullOrEmpty(key))
            return key ?? "";

        object node = data;
        var parts = key.Split('.');
        for (int i = 0; i < parts.Length; i++)
        {
            var field = node.GetType().GetField(parts[i]);
            if (field == null)
                return key;
            node = field.GetValue(node);
            if (node == null)
                return key;
        }

        return node as string ?? key;
    }
}
