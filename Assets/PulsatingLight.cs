using UnityEngine;
using System.Collections;

public class PulsatingLight : MonoBehaviour {

	private Light2D light;

	public float minRadius = 5;
	public float maxRadius = 10;
	public float time = 0.5f;
	public float delay = 0.5f;

	public string easetype = "EaseOutBack";

	void Start()
	{
		light = GetComponent<Light2D>();
		iTween.ValueTo(this.gameObject, iTween.Hash("delay", delay, "from", minRadius, "to", maxRadius, "time", time, "easetype", easetype, "looptype", "pingpong", "onupdate", "ChangeRadius"));
	}

	void ChangeRadius (float val)
	{
		light.LightRadius = val;
	}
}
