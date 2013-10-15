using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {

	private tk2dTextMesh timerText;

	private float overAllTime;
	private float minuteTime;
	private float secondTime;
	private float miliSecondTime;

	public float startTime = 181;

	void Start()
	{
		timerText = GetComponent<tk2dTextMesh>();
		overAllTime = startTime;
	}

	void Update()
	{

		overAllTime -= Time.deltaTime;

		minuteTime = Mathf.FloorToInt(overAllTime / 60f);
		secondTime = overAllTime-(minuteTime*60f);

		timerText.text = string.Format("{0:00}:{1:00}", minuteTime, secondTime);
		//timerText.text
		//timerText.Commit();

		timerText.Commit();
	}

	public void ResetClock()
	{
		overAllTime = 0;
	}
}
