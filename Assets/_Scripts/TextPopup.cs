using UnityEngine;
using System.Collections;

public class TextPopup : MonoBehaviour {

	private tk2dTextMesh tk2dText;
	private iTweenEvent tweenEvent;
	public float lifetime = 0.7f;
	
	void OnSpawned()
	{
		if (tk2dText == null)
			tk2dText = GetComponent<tk2dTextMesh>();
		if (tweenEvent == null)
			tweenEvent = GetComponent<iTweenEvent>();

		tweenEvent.Play();

		Invoke("Despawn", lifetime);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			GetComponent<iTweenEvent>().Play();
		}
	}

	public void ChangeText(string txt)
	{
		tk2dText.text = txt;
		tk2dText.color = Color.white;
		tk2dText.Commit();
	}

	public void ChangeText(string txt, Color clr)
	{
		tk2dText.text = txt;
		tk2dText.color = clr;
		tk2dText.Commit();
	}

	void Despawn()
	{
		PREFAB.DespawnPrefab(this.transform, "1");
	}
}
