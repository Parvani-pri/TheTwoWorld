using UnityEngine;

public class MaskUIButtons : MonoBehaviour
{
    public void ScaleUIElementsOnHover(bool shouldScaleUp)
    {
        transform.localScale = shouldScaleUp ? new Vector3(1.2f, 1.2f, 1.2f) : Vector3.one;
    }
}
