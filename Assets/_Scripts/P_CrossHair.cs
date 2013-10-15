using UnityEngine;
using System.Collections;

public class P_CrossHair : MonoBehaviour {

	[HideInInspector]
	public 	Transform					crossHair;
	private Camera						cam;

	[HideInInspector]
	public	float						currentAngle;

	void Awake () {
		cam =							Camera.mainCamera;
		crossHair = GameObject.Find("CrossHair").transform;
		
		Screen.showCursor = 			false;
	}
	
	void Update()
	{
		if (PlayerManager.mouseControls){

			CrossHairPositioning();
		
			SetCurrentAngle();

			transform.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(transform.localEulerAngles.z, currentAngle+90, Time.deltaTime * 14));
		}else
		{
			crossHair.position = new Vector3(-10000,0,0);
		}
	}

	void CrossHairPositioning()
	{
		crossHair.position = cam.camera.ScreenToWorldPoint(Input.mousePosition);
		crossHair.position = new Vector3(crossHair.position.x, crossHair.position.y, -5);
	}
	
	void SetCurrentAngle()
	{
		currentAngle = CalculateAngleToCrossHair(transform.position);
	}
	
	public float CalculateAngleToCrossHair (Vector3 pos)
	{
		Vector3 calculatedDirection = new Vector3 (crossHair.position.x, crossHair.position.y, 0) - new Vector3(pos.x, pos.y, 0);
		float 	calculatedAngle = Vector3.Angle(Vector3.up, calculatedDirection);
		
		if (crossHair.position.x < pos.x)
			return calculatedAngle;
		else
			return -calculatedAngle;
	}
	
	public float CalculateAngle (Vector3 pos1, Vector3 pos2)
	{
		Vector3 calculatedDirection = new Vector3 (pos2.x, pos2.y, 0) - new Vector3(pos1.x, pos1.y, 0);
		float 	calculatedAngle = Vector3.Angle(transform.up, calculatedDirection);
		
		if (pos2.x < pos1.x)
			return calculatedAngle;
		else
			return -calculatedAngle;
	}
}
