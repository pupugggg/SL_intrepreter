using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide : MonoBehaviour {

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Pair")
        {
            GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            other.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        other.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
    }
}
