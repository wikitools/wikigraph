using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#edit-config-in-play-mode")]
public class EditorOnly_EditConfigInPlayMode : MonoBehaviour
{
    public LzwpConfig config;

    [Space]
    public bool applyCustomConfigChanges = true;
    public CustomAppConfig customConfig;

    void Start()
    {
        Lzwp.AddAfterInitializedAction(() => {
            config = Lzwp.config;
            customConfig = Lzwp.config.GetCustom();
        });
    }
}
