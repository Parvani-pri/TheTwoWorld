using System.Collections;
using TwoWorlds.Core;
using UnityEngine;

public class MaskUIController : MonoBehaviour
{
    [SerializeField] RectTransform targetCanvas;
    [SerializeField] RectTransform hiddenPos;
    [SerializeField] RectTransform shownPos;
    [SerializeField] InputReader inputReader;


    private Coroutine activeCoroutine;

    Vector3 startPos;
    Vector3 endPos;

    private void Start()
    {
        startPos = hiddenPos.position;
        endPos = shownPos.position;

        targetCanvas.position = startPos;

        if (inputReader == null)
            inputReader = FindFirstObjectByType<InputReader>();
    }

    private void Update()
    {
        if (inputReader.ToggleMenu.WasPressedThisFrame() && activeCoroutine == null)
        {
           
            activeCoroutine = StartCoroutine(MoveCanvas(0.3f));


        }
    }

    IEnumerator MoveCanvas(float duration)
    {
        float timeLapsed = 0f;
        while (timeLapsed < duration)
        {
            targetCanvas.position = Vector3.Lerp(startPos, endPos, timeLapsed / duration);
            timeLapsed += Time.deltaTime;
            yield return null;
        }
        if (timeLapsed >= duration)
        {
            targetCanvas.position = endPos;
        }
        Vector3 startPos_Temp = startPos;
        startPos = endPos;
        endPos = startPos_Temp;
        activeCoroutine = null;
    }




}
