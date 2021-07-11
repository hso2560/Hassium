using System;

[Serializable]
public class SaveData
{
    public Option option = new Option();  
}

[Serializable]
public class Option
{
    public float masterSoundSize=0.7f;
    public float soundEffectSize=0.7f;
    public float bgmSize=0.7f;
}