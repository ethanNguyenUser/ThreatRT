using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    int hitCounter = 0;
    void OnCollisionEnter()
    {
        hitCounter++;
        Debug.Log("You have hit a wall " + hitCounter + " times");
    }
}
