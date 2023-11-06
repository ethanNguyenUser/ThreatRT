using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorer : MonoBehaviour
{
    int hitCounter = 0;
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag != "Hit")
        {
            hitCounter++;
            Debug.Log("You have hit a wall " + hitCounter + " times");
        }
        
    }
}
