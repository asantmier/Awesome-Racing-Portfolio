using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderOnce : MonoBehaviour
{
    public Camera cam;
    public RenderTexture tex;

    private void LateUpdate()
    {
        if (cam.enabled)
        {
            cam.targetTexture = tex;

            cam.Render();

            // Could probably even disable this script as well to save time
            cam.enabled = false;
        }
    }
}
