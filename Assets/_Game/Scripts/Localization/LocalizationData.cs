using System;

[Serializable]
public class MenuStrings
{
    public string play;
    public string settings;
    public string quit;
}

[Serializable]
public class SettingsStrings
{
    public string title;
    public string master;
    public string bgm;
    public string sfx;
    public string voice;
    public string fullscreen;
    public string language;
    public string close;
}

[Serializable]
public class DialogStrings
{
    public string skip;
    public string correct;
    public string wrong;
}

[Serializable]
public class CommonStrings
{
    public string yes;
    public string no;
    public string confirm;
    public string save;
    public string cancel;
    public string loading;
}

[Serializable]
public class LocalizationData
{
    public MenuStrings menu = new MenuStrings();
    public SettingsStrings settings = new SettingsStrings();
    public DialogStrings dialog = new DialogStrings();
    public CommonStrings common = new CommonStrings();
}
