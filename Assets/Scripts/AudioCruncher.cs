using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCruncher : MonoBehaviour
{
    public float hiFreq, loFreq, airLerp, groundLerp;

    private float hiBase, loBase, hiV, loV;

    public static bool air = false;

    private AudioConfiguration config;
    private AudioHighPassFilter hi;
    private AudioLowPassFilter lo;

    // Start is called before the first frame update
    void Start()
    {
        config = AudioSettings.GetConfiguration();
        hi = GetComponent<AudioHighPassFilter>();
        hiBase = hi.cutoffFrequency;
        lo = GetComponent<AudioLowPassFilter>();
        loBase = lo.cutoffFrequency;
    }

    // Update is called once per frame
    void Update()
    {
        if (air)
        {
            hiV = Mathf.Lerp(hiV, hiFreq, airLerp);
            loV = Mathf.Lerp(loV, loFreq, airLerp);
        }
        else
        {
            hiV = Mathf.Lerp(hiV, hiBase, groundLerp);
            loV = Mathf.Lerp(loV, loBase, groundLerp);
        }
        hi.cutoffFrequency = hiV;
        lo.cutoffFrequency = loV;
    }
}
