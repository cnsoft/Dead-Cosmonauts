using UnityEngine;

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
    }
	{
	    currentMovement[0] = Input.GetAxisRaw(AxisNameHorizontal);
        currentMovement[1] = Input.GetAxisRaw(AxisNameVertical);

	    if (currentMovement.magnitude > maxMagnitude)
	    {
	        currentMovement.Normalize();
            currentMovement.Scale(_maxMagnitude);
	    }