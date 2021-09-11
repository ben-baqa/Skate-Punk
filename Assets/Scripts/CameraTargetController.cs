using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{
    public float followLerp, centreLerp, offset, offsetAngle,
        standoff, standoffLerp, standoffThreshold, standoffAngle, standoffFollowLerp;
    public Vector2 mouseSensitivity, gamePadSensitivity;

    private Rigidbody rb;
    private Transform targetTransform, body;
    private CameraController cam;
    private Vector3 target, baseTargetOffset;

    private float rotX, rotY, sAngle = 0;
    private bool flip, stopped;

    // Start is called before the first frame update
    void Start()
    {
        targetTransform = transform.GetChild(0);
        baseTargetOffset = targetTransform.localPosition;
        body = transform.parent;
        cam = FindObjectOfType<CameraController>();
        target = targetTransform.position;

        rb = transform.parent.parent.GetComponentInChildren<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        MoveAndRotate();
        cam.Follow(target, transform.position);
    }

    public void ResetRotation(float f)
    {
        rotX = f;
        flip = true;
        stopped = false;
        MoveAndRotate();
        //cam.Follow(target, transform.position);
    }

    public void OnStop()
    {
        stopped = true;
        flip = true;
        transform.localPosition = new Vector3(0, .25f, 0);
        MoveAndRotate();
    }

    private void MoveAndRotate()
    {
        bool notUpright = (body.up - Vector3.up).magnitude > standoffThreshold;
        Vector2 look = Vector2.zero;
        if (stopped)
        {
            Gamepad g = Gamepad.current;
            if (g != null)
            {
                Vector2 gD = g.rightStick.ReadValue();
                look.x = gD.x * gamePadSensitivity.x;
                look.y = gD.y * gamePadSensitivity.y;
            }
            Vector2 mouseD = Mouse.current.delta.ReadValue();
            look.x += mouseD.x * mouseSensitivity.x;
            look.y -= mouseD.y * mouseSensitivity.y;
        }else if(notUpright)
        {
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition,
                baseTargetOffset + Vector3.back * standoff, standoffLerp);
            sAngle = Mathf.Lerp(sAngle, standoffAngle, standoffLerp);
        }
        else
        {
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition,
                baseTargetOffset, standoffLerp);
            sAngle = Mathf.Lerp(sAngle, 0, standoffLerp);
        }

        rotX = Mathf.Lerp(rotX + look.x, flip? offsetAngle + sAngle:-offsetAngle -sAngle,
            stopped?0:centreLerp);
        rotY = Mathf.Lerp(rotY + look.y, 0, stopped?0:centreLerp);
        rotY = Mathf.Clamp(rotY, -20, 50);
        rotX = LoopAngle(rotX);
        if (!stopped) {
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                flip = !flip;

            transform.localPosition = new Vector3(flip ? -offset : offset, .25f, 0);
            transform.up = Vector3.up;
            transform.eulerAngles = Vector3.up * (rotX + body.eulerAngles.y);
        }else
            transform.localEulerAngles = Vector3.up * rotX + Vector3.right * rotY;


        target = Vector3.Lerp(target, targetTransform.position, notUpright?
            standoffFollowLerp: followLerp);
    }

    private float LoopAngle(float f)
    {
        if (f > 180)
            return LoopAngle(f - 360);
        if (f < -180)
            return LoopAngle(f + 360);
        return f;
    }
}
