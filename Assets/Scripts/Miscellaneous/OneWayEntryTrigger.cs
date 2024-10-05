// Description: General use case one-way entry trigger
// Can be used with barriers and deadzones (note that the older version of this script has exit and entry backwards, hence they have been deprecated; try not to get confused)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayEntryTrigger : MonoBehaviour
{
    [SerializeField] private Barrier barrier;
    // [SerializeField] private DeadzoneCollider deadzoneCollider;
    // [SerializeField] private DeadzoneBarrier deadzoneBarrier;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is entering from the entry side, deactivate the collider");
            if (barrier != null)
            {
                barrier.Deactivate();
            }
            /*else if (deadzoneCollider != null)
            {
                deadzoneCollider.Deactivate();
            }
            else
            {
                deadzoneBarrier.Deactivate();
            }*/
        }
    }
}