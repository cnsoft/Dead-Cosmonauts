using UnityEngine;
using System.Collections;

public class ToggleLight_VLS : MonoBehaviour 
{
    Light2D l2D;

    void Start()
    {
        Random.seed = gameObject.GetInstanceID();
        InvokeRepeating("ToggleLight", Random.Range(0.2f, 2f), Random.Range(0.2f, 2f));
        l2D = gameObject.GetComponent<Light2D>();
    }

    void ToggleLight()
    {
        if (!l2D.LightEnabled)
            l2D.LightColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.2f);

        l2D.ToggleLight(true);
    }
}
