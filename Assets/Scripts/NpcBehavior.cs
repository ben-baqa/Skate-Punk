using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    public Sprite image, knockedImage;

    private Transform cam;
    private SpriteRenderer rend;
    private Rigidbody rb;

    private float rot = 0, lerp = 0.2f;

    private bool bonked = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = image;
        cam = Camera.main.transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!bonked && collision.gameObject.CompareTag("Player")){
            bonked = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.velocity *= 2;
            rend.sprite = knockedImage;
        }
    }
    private void Update()
    {
        if (bonked)
        {
            rend.flipX = transform.forward.y > 0;
        }
        else
        {
            Vector3 dir = cam.position - transform.position;
            float r = Mathf.Atan2(dir.x, dir.z);
            r = Mathf.Rad2Deg * r;

            rot = Mathf.Lerp(rot, r, lerp);

            rb.MoveRotation(Quaternion.Euler(0, rot, 0));
        }
    }
}
