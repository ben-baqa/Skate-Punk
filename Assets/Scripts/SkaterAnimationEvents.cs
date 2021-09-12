using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkaterAnimationEvents : MonoBehaviour
{
    public AudioSource jump, scoot, land;

    private SkateController pCon;

    // Start is called before the first frame update
    void Start()
    {
        pCon = GetComponentInParent<SkateController>();
    }

    private void OnStop()
    {
        pCon.OnStop();
    }

    private void OnStart()
    {
        pCon.OnStart();
    }

    private void Jump()
    {
        pCon.Jump();
        jump.Play();
    }

    private void Push(float f)
    {
        pCon.Push(f);
    }

    private void OnLand()
    {
        land.Play();
    }

    private void OnScoot()
    {
        scoot.Play();
    }
}
