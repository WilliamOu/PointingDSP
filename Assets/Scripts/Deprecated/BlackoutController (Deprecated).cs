using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackoutControllerDeprecated : MonoBehaviour
{
    private void Start()
    {
        // Enable the blackout veil on game start
        gameObject.SetActive(true);
    }

    public void DisableBlackout()
    {
        // Disable the blackout veil
        gameObject.SetActive(false);
    }
}
