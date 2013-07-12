using UnityEngine;[RequireComponent(typeof(CharacterController))]public class Player : MonoBehaviour{    private const string AxisNameHorizontal = "Horizontal";    private const string AxisNameVertical = "Vertical";    /// <summary>    /// Your player's current movement.    /// </summary>    public Vector3 currentMovement;    /// <summary>    /// Your character's move speed.    /// </summary>    public float moveSpeed = 5;    /// <summary>    /// A pointer to the character controller, initialized by default.    /// </summary>    public CharacterController controller;

    private Vector3 _maxMagnitude = new Vector3(1,1,1);

    /// <summary>
    /// Constrain the magnitude of the axes.
    /// </summary>
    public float maxMagnitude
    {
        get { return _maxMagnitude[0]; }
        set
        {
            _maxMagnitude = new Vector3(value,value,value);
        }
    }	// Use this for initialization	void Awake ()	{        if (controller == null)        {            controller = GetComponent<CharacterController>();        }	}		// Update is called once per frame	void Update ()
	{
	    currentMovement[0] = Input.GetAxisRaw(AxisNameHorizontal);
        currentMovement[1] = Input.GetAxisRaw(AxisNameVertical);

	    if (currentMovement.magnitude > maxMagnitude)
	    {
	        currentMovement.Normalize();
            currentMovement.Scale(_maxMagnitude);
	    }	    controller.Move(currentMovement*moveSpeed*Time.deltaTime);	}}