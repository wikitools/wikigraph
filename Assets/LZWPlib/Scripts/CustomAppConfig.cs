using System;

[Serializable]
public partial class CustomAppConfig { }

public static class LzwpConfigExtension
{
    public static CustomAppConfig GetCustom(this LzwpConfig cfg)
    {
        return cfg.GetCustom<CustomAppConfig>();
    }
}
