using UnityEngine;
using UnityEngine.UI;

[HelpURL(Lzwp.LzwpLibManualUrl + "#input-example")]
public class FlystickJoystick_InputExample : MonoBehaviour
{
    public int flystickIdx = 0;
    public Transform pointerTransform;
    public Text label;

    Texture2D tex;
    const int texRes = 512;
    const float halfTexRes = (float)texRes / 2f;

    int prev_tx = (int)halfTexRes;
    int prev_ty = (int)halfTexRes;

    Color drawColor = new Color(0, 0, 0, 0.1f);
   
    void Awake()
    {
        tex = new Texture2D(texRes, texRes);
        transform.GetComponent<Renderer>().material.mainTexture = tex;
    }

    void FixedUpdate()
    {
        float x = Lzwp.input.flysticks[flystickIdx].joysticks[0];
        float y = Lzwp.input.flysticks[flystickIdx].joysticks[1];

        label.text = string.Format("{0}, {1}", x.ToString("F3"), y.ToString("F3"));

        pointerTransform.localPosition = new Vector3(
            x * 0.5f,
            y * 0.5f,
            -0.005f
        );

        int tx = JoystickValToTexCoord(x);
        int ty = JoystickValToTexCoord(y);
        DrawLine(tex, prev_tx, prev_ty, tx, ty, drawColor);
        prev_tx = tx;
        prev_ty = ty;

        tex.SetPixel(tx, ty, Color.red);
        tex.Apply();
    }



    int JoystickValToTexCoord(float x)
    {
        // x in range <-1; 1>
        return (int)((x + 1f) * halfTexRes);
    }

    // http://wiki.unity3d.com/index.php/TextureDrawLine#TextureDraw.cs
    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        tex.SetPixel(x0, y0, col);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                tex.SetPixel(x0, y0, col);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                tex.SetPixel(x0, y0, col);
            }
        }
    }
}
