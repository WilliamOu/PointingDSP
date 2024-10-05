// Description: This script scales the position of objects in the scene based on the inputted scale
// A seperate script is necessary since the size of the objects should not change
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionScaler : MonoBehaviour
{
    private float scale;
    private float heightScale;

    private void Start()
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = (PersistentDataManager.Instance.Map == "Default Map") ? 1f : PersistentDataManager.Instance.HeightScale;

        transform.position = new Vector3(
            transform.position.x * scale,
            transform.position.y * heightScale,
            transform.position.z * scale
        );

        // Prevents clipping
        // Hardcoded for a specific roomscale study, which uses a scale of 0.33
        /*if (scale < 1)
        {
            transform.localScale = new Vector3(
                transform.localScale.x * 0.6f,
                transform.localScale.y * 0.6f,
                transform.localScale.z * 0.6f
            );
        }*/
    }
}
