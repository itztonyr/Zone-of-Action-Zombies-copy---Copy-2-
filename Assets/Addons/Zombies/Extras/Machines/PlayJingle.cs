using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayJingle : MonoBehaviour
{
    public AudioSource Jingle;

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player")
        {
            Jingle.Play();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Jingle.Stop();
        }

    }

}
