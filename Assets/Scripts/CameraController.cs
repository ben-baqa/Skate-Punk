using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public void Follow(Vector3 pos, Vector3 lookAt)
    {
        transform.position = pos;
        transform.LookAt(lookAt);
    }
}
