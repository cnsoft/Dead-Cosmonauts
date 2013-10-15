using UnityEngine;
using System.Collections;

public class StartGame : MonoBehaviour {

	private tk2dSprite sprite;
	private AudioSource audioS;
	public GameObject clientP;

	// Use this for initialization
	void Start () {
		sprite = GetComponent<tk2dSprite>();
		audioS = GetComponent<AudioSource>();
		clientP.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.anyKeyDown)
		{
			sprite.color = Color.clear;
			clientP.SetActive(true);
		}


	}
}
