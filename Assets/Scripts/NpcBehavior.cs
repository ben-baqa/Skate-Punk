using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    public Sprite image, knockedImage;

    private SpriteRenderer rend;
    private Rigidbody rb;

    private bool bonked = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = image;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!bonked && collision.gameObject.CompareTag("Player")){
            bonked = true;
            rb.velocity *= 2;
            rend.sprite = knockedImage;
        }
    }
    private void Update()
    {
        rend.flipX = transform.forward.y > 0;
    }
}
