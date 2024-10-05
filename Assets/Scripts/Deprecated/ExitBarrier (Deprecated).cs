// Description: Used in the implementation of a one-way barrier in the learning stage
// It disables the collider when the player enters from the exit side
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitBarrier : MonoBehaviour
{
    [SerializeField] private Collider barrierCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is entering from the exit side, make the wall passable");
            barrierCollider.isTrigger = true;
        }
    }
}