using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    [SerializeField] float speed;
    private void Update()
    {
        if (Input.GetKey(KeyCode.W)) transform.Translate(speed * Time.deltaTime * Vector3.up);
        if (Input.GetKey(KeyCode.A)) transform.Translate(speed * Time.deltaTime * Vector3.left);
        if (Input.GetKey(KeyCode.S)) transform.Translate(speed * Time.deltaTime * Vector3.down);
        if (Input.GetKey(KeyCode.D)) transform.Translate(speed * Time.deltaTime * Vector3.right);
    }
}
