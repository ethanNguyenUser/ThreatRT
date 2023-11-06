using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    Rigidbody rigidbody;
    MeshRenderer renderer;
    [SerializeField] float timeToWait = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponent<MeshRenderer>();

        renderer.enabled = false;
        rigidbody.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timeToWait)
        {
            renderer.enabled = true;
            rigidbody.useGravity = true;
        }

    }
}
