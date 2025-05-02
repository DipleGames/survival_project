using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTileAlpha : MonoBehaviour
{
    [SerializeField] SpriteRenderer image;
    public GameObject player;

    private void Update()
    {
        if ((image.gameObject.transform.position.z + 0.7f < player.transform.position.z))
        {
            Color color = Color.white;
            color.a = 0.2f;
            image.color = color;
        }

        else
        {
            image.color = Color.white;
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Character"))
        {
            Debug.Log("!");

            Color color = Color.white;
            color.a = 0.3f;
            image.color = color;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        *//*if (other.CompareTag("Character"))
        {
            image.color = Color.white;
        }*//*
    }*/
}
