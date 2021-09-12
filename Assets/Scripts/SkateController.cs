using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkateController : MonoBehaviour
{
    [Header("Basic Attributes")]
    public float pushForce;
    public float turnForce, backupTurnForce, airTurnForce, brakeForce,
        jumpForce, pushDelay, maxSpeed = 10;
    [Range(0, 1f)]
    public float wheelFriction, driftFriction, maxBrakeFriction;

    [Header("Fine Tuning")]
    [Range(0, 1)]
    public float brakeLerp, turnLerp, turnDecayLerp;
    public AnimationCurve pushForceCurve;

    [Header("Noises")]
    public AudioSource scootSfx;
    public AudioSource jumpSfx;

    [Header("Misc")]
    public float standingAngle;


    private Transform body, sprite, camTarget;
    private CameraTargetController camCon;
    private Animator anim;

    private float pushTime;

    private Rigidbody rb;
    private Vector3 surfaceNormal;

    //private TrickHandler trickHandler;

    // controls
    private float turn, brake, rotation, spriteRotation;
    private bool go, jump, pushing = false;

    private float groundThreshold;
    private bool tricks = false, stopped = true, shouldGo=true;
    public static bool ground;

    private float groundTime;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundThreshold = rb.GetComponent<SphereCollider>().radius * 1.25f;

        body = transform.GetChild(0);
        sprite = body.GetChild(0);
        camCon = GetComponentInChildren<CameraTargetController>();
        camCon.OnStop();
        camTarget = camCon.transform;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateControls();
        anim.SetBool("stopped", stopped);
    }

    private void FixedUpdate()
    {
        UpdatePositions();
        anim.SetBool("ground", ground);
        Turn();
        if (ground)
        {
            if (stopped)
            {
                rb.velocity = Vector3.zero;
                if (go && shouldGo)
                {
                    stopped = false;
                    go = false;
                    shouldGo = false;
                    //OnStart();
                }
                else
                {
                    sprite.eulerAngles = Vector3.up * (camTarget.eulerAngles.y + standingAngle);
                }
            }
            else
            {
                ApplyFriction();

                if(brake > 0 &&
                    Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z) < 0.1f)
                {
                    stopped = true;
                    shouldGo = true;
                    go = false;
                }

                if (go && !pushing)
                {
                    if (pushTime + pushDelay < Time.time)
                    {
                        anim.SetTrigger("push");
                        pushTime = Time.time;
                        pushing = true;
                    }
                }

                if (pushing && Vector3.ProjectOnPlane(rb.velocity, Vector3.up)
                    .magnitude < maxSpeed)
                {
                    float v = pushForceCurve.Evaluate(Time.time - pushTime);
                    rb.AddForce(body.forward * v * pushForce);
                    if (pushTime + pushForceCurve[pushForceCurve.length - 1].time < Time.time)
                        pushing = false;
                }
                if (jump)
                {
                    anim.SetTrigger("jump");
                    jump = false;
                }
                else if (body.forward.y >= -.15f)
                    rb.AddForce(-Physics.gravity * 0.95f);
            }
        }
        AudioCruncher.air = !ground;
        //else go = false;
    }

    private void ApplyFriction()
    {
        Vector3 normVel = Vector3.Project(rb.velocity, surfaceNormal);
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, surfaceNormal);


        Vector3 forward = Vector3.ProjectOnPlane(body.forward, surfaceNormal);
        Vector3 right = Vector3.ProjectOnPlane(body.right, surfaceNormal);

        float paraFric = wheelFriction + brake;
        paraFric = Mathf.Clamp(paraFric, 0, maxBrakeFriction);

        Vector3 paraV = Vector3.Project(flatVel, forward);
        Vector3 perpV = Vector3.Project(flatVel, right);

        //if (Vector3.Dot(paraV, forward) <= reverseThreshhold)
        //{
        //    paraFric = wheelFriction;
        //    rb.AddForce(-body.forward * brake * brakeForce);
        //}

        paraV = paraV * (1 - paraFric);
        perpV = perpV * (1 - driftFriction);

        rb.velocity = paraV + perpV + normVel;
    }

    private void UpdateControls()
    {
        bool brakeDecay = true, turnDecay = true;
        jump = go = false;

        Gamepad g = Gamepad.current;
        if (g != null)
        {
            turn = g.leftStick.x.ReadValue();
            turnDecay = turn == 0;
            go = g.rightTrigger.IsActuated();
            brake = g.leftTrigger.ReadValue();
            brakeDecay = brake == 0;

            jump |= g.aButton.isPressed;
        }
        var k = Keyboard.current;
        if (k.wKey.isPressed || k.upArrowKey.isPressed)
            go = true;

        if (k.sKey.isPressed || k.downArrowKey.isPressed)
            brake = Mathf.Lerp(brake, 1, brakeLerp);
        else if (brakeDecay)
            brake = 0;

        if (turnDecay)
            turn = Mathf.Lerp(turn, 0, turnDecayLerp);
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)
            turn = Mathf.Lerp(turn, -1, turnLerp);
        if (k.dKey.isPressed || k.rightArrowKey.isPressed)
            turn = Mathf.Lerp(turn, 1, turnLerp);

        turn = CleanInput(turn);
        brake = CleanInput(brake);

        jump |= k.spaceKey.isPressed;
    }

    private float CleanInput(float f)
    {
        return Mathf.Clamp(f, -1, 1);
    }

    private void Turn()
    {
        if (ground)
        {
            spriteRotation = Mathf.Lerp(spriteRotation, 90, 0.1f);
            float v = Vector3.Dot(rb.velocity, body.forward);
            float diff = turn * v * Time.fixedDeltaTime;
            diff *= turnForce;
            rotation = LoopAngle(rotation + diff);
        }
        else
        {
            spriteRotation = LoopAngle(spriteRotation + turn * airTurnForce, 90);
        }
        sprite.localEulerAngles = Vector3.up * spriteRotation;
    }

    private void UpdatePositions()
    {
        if (rb.position.y < -30)
        {
            rb.position = new Vector3(0, .5f, 0);
            rb.velocity = Vector3.zero;
            stopped = true;
            shouldGo = true;
            go = false;
            OnStop();
        }

        body.position = rb.position;
        ground = false;

        RaycastHit rayHit;
        bool hit = Physics.Raycast(rb.position, -body.up, out rayHit);
        ground = hit && rayHit.distance < groundThreshold;
        if (ground) surfaceNormal = rayHit.normal;
        else
        {
            hit = Physics.Raycast(rb.position, Vector3.down, out rayHit);
            ground = hit && rayHit.distance < groundThreshold;
            if (ground) surfaceNormal = rayHit.normal;
        }
        if (ground)
            if (rayHit.collider)
                camCon.shouldStandoff = !rayHit.collider.CompareTag("ground");
            else
                camCon.shouldStandoff = false;


        if (ground)
            groundTime = Time.time;

        if (!ground && !tricks && Time.time > groundTime + .25f)
        {
            tricks = true;
            //trickHandler.StartTricks();
        }
        else if (ground && tricks)
        {
            tricks = false;
            Vector3 dir = Vector3.ProjectOnPlane(rb.velocity, surfaceNormal).normalized;
            //float r = Mathf.Atan2(dir.z, Mathf.Sqrt(dir.y * dir.y + dir.x * dir.x));
            //r *= Mathf.Rad2Deg;
            //if (dir.x > 0)
            //    r = 180 - r;
            //rotation = r + 90;

            body.forward = dir;
            rotation = body.localEulerAngles.y;

            anim.SetTrigger("land");
            //trickHandler.EndTricks();
        }

        body.up = Vector3.Lerp(body.up, surfaceNormal, .2f);
        body.RotateAround(body.position, body.up, rotation);
    }

    private float LoopAngle(float angle, float centre = 0)
    {
        if (angle > 180 + centre)
            return LoopAngle(angle - 360, centre);
        else if (angle < -180 + centre)
            return LoopAngle(angle + 360, centre);
        return angle;
    }

    public void OnStart()
    {
        float camRot = camTarget.eulerAngles.y;
        float diff = camRot - rotation;
        rotation = LoopAngle(camTarget.eulerAngles.y);
        body.RotateAround(body.position, body.up, rotation);
        camCon.ResetRotation(0);
        spriteRotation = LoopAngle(90 - standingAngle, 90);
        sprite.localEulerAngles = Vector3.up * spriteRotation;
        go = false;
    }

    public void OnStop()
    {
        camCon.OnStop();
    }

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Push(float f)
    {
        rb.AddForce(body.forward * f, ForceMode.Impulse);
        anim.ResetTrigger("jump");
    }
}
