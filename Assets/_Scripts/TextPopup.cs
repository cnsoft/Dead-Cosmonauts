using UnityEngine;
using System.Collections;

public class TextPopup : MonoBehaviour {

	private tk2dTextMesh tk2dText;
	private iTweenEvent tweenEvent;



	void OnSpawned()
	{
		if (tk2dText == null)
			tk2dText = GetComponent<tk2dTextMesh>();
		if (tweenEvent == null)
			tweenEvent = GetComponent<iTweenEvent>();

		tweenEvent.Play();

		Invoke("Despawn", 0.5f);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			GetComponent<iTweenEvent>().Play();
		}
	}

	void ChangeText(int dmg)
	{
		tk2dText.text = dmg.ToString("f0");
		tk2dText.Commit();
	}

	void Despawn()
	{
		PREFAB.DespawnPrefab(this.transform, "1");
	}
}
