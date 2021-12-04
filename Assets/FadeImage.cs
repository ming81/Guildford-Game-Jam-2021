using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeImage : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(FadeImageToZeroAlpha(5f, GetComponent<RawImage>()));
    }

    public IEnumerator FadeImageToZeroAlpha(float t, RawImage i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }
}