using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	
	public Vector2		offset;
	public float		smoothing = 5.0f;
	
	public float		minX = -5;
	public float		maxX = 10;
	public float		minY = 0;
	public float		maxY = 10;
	
	public Transform	playerTransform;
	private float		_curSmoothX;
	private float		_curSmoothY;
	private Camera		_cam;
	
	private float		_distanceX;
	private float		_distanceY;
	
	
	void Start () {
		_cam = Camera.mainCamera;
	}
	
	void Update () {
        if (playerTransform == null) {
            return;
        }
        _distanceX  = Mathf.Abs (Vector3.Distance(new Vector3(transform.position.x, 0,0), new Vector3(playerTransform.position.x, 0,0)));
        _distanceY  = Mathf.Abs (Vector3.Distance(new Vector3(0, transform.position.y,0), new Vector3(0, playerTransform.position.y, 0)));
        
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
        if (playerTransform == null) {
            return;
        }
		transform.position = new Vector3(Mathf.Lerp (transform.position.x, playerTransform.position.x + offset.x, _curSmoothX),
						 				 Mathf.Lerp (transform.position.y, playerTransform.position.y + offset.y, _curSmoothY),
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
