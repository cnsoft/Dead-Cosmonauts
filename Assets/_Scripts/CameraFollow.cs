using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	
	public Vector2		offset;
	public float		smoothing = 5.0f;
	
	public float		minX = -5;
	public float		maxX = 10;
	public float		minY = 0;
	public float		maxY = 10;
	
	private Transform	_player;
	private float		_curSmoothX;
	private float		_curSmoothY;
	private Camera		_cam;
	
	private float		_distanceX;
	private float		_distanceY;
	
	
	void Start () {
		_player = GameObject.FindGameObjectWithTag("Player").transform;
		if(_player == null)
			Debug.LogError("CameraFollow: Start: Couldn't find Player");
		
		_cam = Camera.mainCamera;
	}
	
	void Update () {
		
		_distanceX  = Mathf.Abs (Vector3.Distance(new Vector3(transform.position.x, 0,0), new Vector3(_player.position.x, 0,0)));
		_distanceY  = Mathf.Abs (Vector3.Distance(new Vector3(0, transform.position.y,0), new Vector3(0, _player.position.y, 0)));
		
		if (_distanceX > 0.1f)
			_curSmoothX = smoothing * Time.deltaTime;
		else
			_curSmoothX = smoothing * Time.deltaTime * 4;
		
		if (_distanceY > 0.025f)
			_curSmoothY = smoothing * Time.deltaTime * 0.85f;
		else
			_curSmoothY = smoothing * Time.deltaTime * 2;
		
		//print (_distanceX.ToString("f3") +" " + _distanceY.ToString("f3"));
	}
	
	void LateUpdate()
	{
		transform.position = new Vector3(Mathf.Lerp (transform.position.x, _player.position.x + offset.x, _curSmoothX),
						 				 Mathf.Lerp (transform.position.y, _player.position.y + offset.y, _curSmoothY),
										 -10);
		transform.position = new Vector3(Mathf.Clamp (transform.position.x, minX+_cam.orthographicSize*_cam.aspect, maxX-_cam.orthographicSize*_cam.aspect), Mathf.Clamp (transform.position.y, minY+_cam.orthographicSize, maxY-_cam.orthographicSize), -10);
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(new Vector3(minX,minY,0), new Vector3(minX,maxY,0)); 
		Gizmos.DrawLine(new Vector3(minX,minY,0), new Vector3(maxX,minY,0));
		Gizmos.DrawLine(new Vector3(minX,maxY,0), new Vector3(maxX,maxY,0));
		Gizmos.DrawLine(new Vector3(maxX,minY,0), new Vector3(maxX,maxY,0));
	}
}
