using UnityEngine;
using UnityEngine.UI;

[HelpURL(Lzwp.LzwpLibManualUrl + "#input-example")]
public class MovingSphere_InputExample : MonoBehaviour
{
    public int flystickIdx = 0;
    public LzwpInput.Flystick.ButtonID buttonID = LzwpInput.Flystick.ButtonID.Unknown;

    public Text label;
    public string btnName = "";
    int stateChangeCounter = 0;

    Material mat;
    Vector3 startingPos;
    float sinX = Mathf.Deg2Rad * 270f;
    
    void Awake()
    {
        UpdateLabel();
    }

    void Start()
    {
        startingPos = transform.position;
        mat = GetComponent<Renderer>().material;

        Lzwp.input.flysticks[flystickIdx].GetButton(buttonID).OnPress += OnPress;
        Lzwp.input.flysticks[flystickIdx].GetButton(buttonID).OnRelease += OnRelease;
    }

    void Update()
    {
        bool move = Lzwp.input.flysticks[flystickIdx].GetButton(buttonID).isActive;

        if (move)
        {
            transform.position = startingPos + Vector3.up * (Mathf.Sin(sinX) + 1f) * 0.5f;
            sinX += Time.deltaTime * 5f;
        }
    }

    void OnPress()
    {
        SetColor(Color.green);
        stateChangeCounter++;
        UpdateLabel();
    }

    void OnRelease()
    {
        SetColor(Color.red);
        stateChangeCounter++;
        UpdateLabel();
    }

    void SetColor(Color c)
    {
        mat.color = c;
    }

    void UpdateLabel()
    {
        label.text = string.Format("{0}\n{1}", btnName, stateChangeCounter);
    }
}
