// Description: This script scales the position of objects in the scene based on the inputted scale
// A seperate script is necessary for size scaling since the scale of the objects should not change
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
        heightScale = PersistentDataManager.Instance.HeightScale;

        // y transform prevents floating objects on the default map
        transform.position = new Vector3(
            transform.position.x * scale,
            transform.position.y * ((PersistentDataManager.Instance.Map == "Default Map") ? 1f : heightScale),
            transform.position.z * scale
        );
    }
}
