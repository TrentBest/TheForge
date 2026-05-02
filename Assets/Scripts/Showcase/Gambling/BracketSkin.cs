using System;

[Serializable]
public class BracketSkin
{
    // The "simple integer" backing
    public int SkinId;
    public string DisplayName;

    // Path to the UXML/USS or Material that defines the visual look
    public string ThemeResourcePath;

    public BracketSkin() { }

    public BracketSkin(int id, string name, string path)
    {
        SkinId = id;
        DisplayName = name;
        ThemeResourcePath = path;
    }

    // Default "Static" references to maintain the ease-of-use of an enum
    public static BracketSkin Digital => new BracketSkin(0, "Digital Neon", "Themes/Gambling/Digital");
    public static BracketSkin Roman => new BracketSkin(1, "Roman Parchment", "Themes/Gambling/Colosseum");
    public static BracketSkin Retro => new BracketSkin(2, "8-Bit Arcade", "Themes/Gambling/Retro");
}