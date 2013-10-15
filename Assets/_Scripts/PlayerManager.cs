using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	public static bool mouseControls;

	void Start()
	{
		mouseControls = true;
	}

	void Update()
	{
		if (Input.GetKeyDown("1"))
		{
			mouseControls = true;
		}
		if (Input.GetKeyDown("2"))
		{
			mouseControls = false;
		}
	}

}
