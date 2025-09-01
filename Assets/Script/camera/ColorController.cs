using UnityEngine;

public class ColorController : MonoBehaviour
{
    public Color color = Color.black; // choose your single color
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = color; // set once
    }
}
