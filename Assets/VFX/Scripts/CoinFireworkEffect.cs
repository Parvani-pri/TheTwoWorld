using UnityEngine;

public class CoinFireworkEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem coinFirework;

    public void PlayCoinFirework()
    {
        coinFirework.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        coinFirework.Play();
    }
}