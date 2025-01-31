// Description: This script scales the visual size AND hitbox size of objects in the scene based on the inputted scale
// A seperate script is necessary for position scaling since this script does not modify position
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SizeAndHitboxScaler : MonoBehaviour
{
    private float scale;
    private float heightScale;

    private void Start()
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = PersistentDataManager.Instance.HeightScale;

        // Scale objects as well
        transform.localScale = new Vector3(
            transform.localScale.x * scale,
            transform.localScale.y * heightScale,
            transform.localScale.z * scale
        );
    }
}
