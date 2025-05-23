using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CybSDK
{
    public class Bullet : MonoBehaviour
    {
        void OnTriggerEnter(Collider collider)
        {
            // Search for a Virtualizer player that is supposedly located on either the collider's gameObject itself or its parents
            CVirtPlayerController cVirtPlayerController = collider.GetComponentInParent<CVirtPlayerController>();
            if (cVirtPlayerController != null)
            {
                // Search for a PlayerHitHapticEmitter that will simulate a hit
                PlayerHitHapticEmitter hapticPlayerHitScript = collider.GetComponentInChildren<PlayerHitHapticEmitter>();
                if (hapticPlayerHitScript != null)
                    hapticPlayerHitScript.HitImpact();
            }

            // Destroy the buller
            Destroy(gameObject);
        }
    }
}

