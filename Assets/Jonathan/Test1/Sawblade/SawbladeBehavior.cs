using UnityEngine;
using UnityEngine.Assertions;
using System;

public class SawbladeBehavior : MonoBehaviour
{
    public float speed = 1.0f;
    Vector3 startPosition;
    Vector3 endPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        endPosition = transform.Find("End").position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPosition + (1.0f - Math.Abs((speed * Time.time % 2.0f) - 1.0f)) * (endPosition - startPosition);
    }
}
