using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] string key;

    TMP_Text _text;
    LocalizationManager _manager;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        _manager = LocalizationManager.Instance;
        _manager.onLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (_manager != null)
            _manager.onLanguageChanged -= Refresh;
    }

    public void SetKey(string newKey)
    {
        key = newKey;
        Refresh();
    }

    void Refresh()
    {
        if (_text == null)
            _text = GetComponent<TMP_Text>();
        if (_text != null && !string.IsNullOrEmpty(key))
            _text.text = LocalizationManager.Get(key);
    }
}
