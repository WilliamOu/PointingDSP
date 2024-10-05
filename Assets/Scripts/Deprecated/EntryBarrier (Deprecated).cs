// Description: Used in the implementation of a one-way barrier in the learning stage
// It enables the collider when the player enters from the entry side
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryBarrier : MonoBehaviour
{
    [SerializeField] private Collider barrierCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is entering from the entry side, make the wall solid");
            barrierCollider.isTrigger = false;
        }
    }
}