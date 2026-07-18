using System.Collections;
using UnityEngine;

public class CoinFireworkEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem coinFirework;
    [SerializeField] private Transform originalParent;
    [SerializeField] private GameObject originalPivot;

    public void PlayCoinFirework()
    {
        coinFirework.transform.parent = null;
        coinFirework.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        coinFirework.Play();
        StartCoroutine(WaitPlayFinished());
    }

    IEnumerator WaitPlayFinished()
    {
        yield return new WaitForSeconds(4f);
        coinFirework.transform.parent = originalParent;
        coinFirework.transform.position = originalPivot.transform.position;
    }
}