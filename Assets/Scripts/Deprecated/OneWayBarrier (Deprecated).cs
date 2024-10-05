using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayBarrierController : MonoBehaviour
{
    // The actual collider of the wall
    [SerializeField] private Collider barrierCollider;
    // Trigger zone to enable the collider
    [SerializeField] private Collider entryTrigger;
    // Trigger zone to disable the collider
    [SerializeField] private Collider exitTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other == entryTrigger)
            {
                Debug.Log("Player is entering from the entry side, make the wall solid");
                // Player is entering from the entry side, make the wall solid
                barrierCollider.enabled = true;
            }
            else if (other == exitTrigger)
            {
                // Print to debug log
                Debug.Log("Player is entering from the exit side, make the wall passable");
                // Player is entering from the exit side, make the wall passable
                barrierCollider.enabled = false;
            }
        }
    }
}
