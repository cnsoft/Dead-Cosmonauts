using UnityEngine;
using System.Collections;

public class RotateLight_VLS : MonoBehaviour 
{
    public float speed = 100;

	void FixedUpdate ()
    {
        transform.Rotate(0, 0, Time.deltaTime * speed);
	}
}
