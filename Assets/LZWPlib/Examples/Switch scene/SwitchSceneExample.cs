using System;
using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#switch-scene-example")]
public class SwitchSceneExample : MonoBehaviour
{
    public const string SCENE_1_NAME = "LzwpExampleScene1";
    public const string SCENE_2_NAME = "LzwpExampleScene2";


    Action<LzwpInput.Flystick.ButtonID>[] flystickBtnPressAction = { };

    private void OnEnable()
    {
        Lzwp.AddAfterInitializedAction(Init);
    }

    private void OnDisable()
    {
        //if (Lzwp.Instance != null)
        //{
            Lzwp.RemoveAfterInitializedAction(Init);

            if (Lzwp.initialized)
            {
                for (int i = 0; i < Lzwp.input.flysticks.Count && i < flystickBtnPressAction.Length; i++)
                {
                    Lzwp.input.flysticks[i].OnButtonPress -= flystickBtnPressAction[i];
                }
            }
        //}
    }

    void Init()
    {
        if (!Lzwp.sync.isMaster)  // only server can read flystick's data
            return;

        flystickBtnPressAction = new Action<LzwpInput.Flystick.ButtonID>[Lzwp.input.flysticks.Count];

        for (int i = 0; i < Lzwp.input.flysticks.Count; i++)
        {
            flystickBtnPressAction[i] = (btn) =>
            {
                FlystickButtonPress(btn);
            };

            Lzwp.input.flysticks[i].OnButtonPress += flystickBtnPressAction[i];
        }
    }

    void FlystickButtonPress(LzwpInput.Flystick.ButtonID btn)
    {
        if (btn == LzwpInput.Flystick.ButtonID.Button1)
            Lzwp.sync.LoadScene(SCENE_1_NAME);
        else if (btn == LzwpInput.Flystick.ButtonID.Button2)
            Lzwp.sync.LoadScene(SCENE_2_NAME);
        else if (btn == LzwpInput.Flystick.ButtonID.Fire)
            Lzwp.sync.ReloadScene();
    }

    void Update()
    {
        if (!Lzwp.sync.isMaster)  // only server can init scene load
            return;

        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // or: activeScene = Lzwp.sync.GetActiveScene().name;

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))  // toggle between the two
        {
            if (activeScene == SCENE_1_NAME)
                Lzwp.sync.LoadScene(SCENE_2_NAME);
            else
                Lzwp.sync.LoadScene(SCENE_1_NAME);
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.R))  // reload current scene
            Lzwp.sync.ReloadScene();  // same as: Lzwp.sync.LoadScene(activeScene);


        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            Lzwp.sync.LoadScene(SCENE_1_NAME);

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            Lzwp.sync.LoadScene(SCENE_2_NAME);
    }
}
