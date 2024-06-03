using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    Image image;
    Coroutine fadeRoutine;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.enabled = false;
    }

    public void FadeOut(float length)
    {
        image.enabled = true;
        if (fadeRoutine != null )
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine("FadeOutHelper", length);
    }

    public void FadeIn(float length)
    {
        image.enabled = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine("FadeInHelper", length);
    }

    IEnumerator FadeOutHelper(float length)
    {
        float time = 0;
        while (time < length)
        {
            var tmp = image.color;
            tmp.a = Mathf.Lerp(0f, 1f, time / length);
            image.color = tmp;
            time = Mathf.Clamp(time + Time.deltaTime, 0, length);
            yield return new WaitForEndOfFrame();
        }
        // let image stay enabled
    }

    IEnumerator FadeInHelper(float length)
    {
        float time = 0;
        while (time < length)
        {
            var tmp = image.color;
            tmp.a = Mathf.Lerp(1f, 0f, time / length);
            image.color = tmp;
            time = Mathf.Clamp(time + Time.deltaTime, 0, length);
            yield return new WaitForEndOfFrame();
        }
        image.enabled = false;
    }
}
