using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSCamera : MonoBehaviour
{

    #region Public Variables

    // Controls
    public string verticalAxis = "Vertical";
    public string horizontalAxis = "Horizontal";
    public string rotateAxis = "";
    public string tiltAxis = "";
    public string directionAxis = "Mouse ScrollWheel";
    public string mouseXAxis = "Mouse X";
    public string mouseYAxis = "Mouse Y";
    public string orbitAxis = "";
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode orbitLeftKey = KeyCode.None;
    public KeyCode orbitRightKey = KeyCode.None;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode tiltIncKey = KeyCode.KeypadPlus;
    public KeyCode tiltDecKey = KeyCode.KeypadMinus;
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;
    public KeyCode directionPositiveKey = KeyCode.None;
    public KeyCode directionNegativeKey = KeyCode.None;
    public KeyCode resetKey = KeyCode.Backspace;
    public Vector3 directionToMove = new Vector3(0, -1, 0);
    public Vector3 directionMultiplier = new Vector3(500, 500, 500);
    public bool middleMouseLocksMouse = true;
    public bool invertMouseX = false;
    public bool invertMouseY = true;
    public bool clampMiddleMouseInput;
    public float middleMouseInputMultiplier = 1f;

    // Control Setup
    public ControlSetup verticalSetup = ControlSetup.Axis;
    public ControlSetup horizontalSetup = ControlSetup.Axis;
    public ControlSetup rotateSetup = ControlSetup.KeyCode;
    public ControlSetup tiltSetup = ControlSetup.KeyCode;
    public ControlSetup directionSetup = ControlSetup.Axis;
    public ControlSetup orbitSetup = ControlSetup.Disabled;
    public MiddleMouseSetup mouseXSetup = MiddleMouseSetup.Rotate;
    public MiddleMouseSetup mouseYSetup = MiddleMouseSetup.Tilt;


    // Camera Variables
    public float cloudStartAtHeightPercent = 90f;
    public float cloudFinishAtHeightPercent = 100f;
    public float cloudStartAtHeight = 200f;
    public float cloudFinishAtHeight = 250f;
    public float cloudMaxAlpha = 0.15f;
    public float movementSpeed = 90f;
    public float rotateSpeed = 180.0f;
    public float minHeightDistance = 5f;
    public float maxHeightDistance = 250f;
    public float tiltMaxHeight = 10f;
    public float lowTilt = 15f;
    public float highTilt = 60f;
    public float tiltSpeed = 50f;
    public bool useDeltaTimeToOne = true;
    public LayerMask groundMask = 0;

    // Behaviour Variables
    public AutoAdjustState autoAdjustState = AutoAdjustState.DistanceToGround;
    public bool adjustTiltWhenHit = true;
    public float distanceFromGroundFinishAdjust = 20f;
    public float distanceFromGroundStartAdjust = 50f;
    public AnimationCurve groundHeightTiltAdjustCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float adjustHeightMin = 0.0f;
    public float adjustHeightMax = 100.0f;
    public AnimationCurve heightTiltAdjustCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public bool smoothPosition = true;
    public bool smoothTilt = true;
    public bool smoothRotate = true;
    public float movementAdjustSpeed = 0.25f;
    public float tiltAdjustSpeed = 0.25f;
    public float rotateAdjustSpeed = 0.25f;
    public HeightAdjustState heightAdjustState = HeightAdjustState.ChangeHeight;
    public Transform followTarget;
    public Vector3 followOffset = new Vector3(0, 15, 0);
    public bool shouldFollow = false;
    public bool shouldLookAt = false;
    public bool movementAdjustsOffset = true;
    public float xEdgeSpeed = 0f;
    public float yEdgeSpeed = 0f;
    public CloudDrawState cloudDrawState = CloudDrawState.PercentOfMaxHeight;
    public float terrainAdjustTilt = 0.0f;

    private float currentRotationDelay = 0;
    private float rotationDelayOnChange = 1.5f;

    // Vel
    private Vector3 moveVel;
    private float tiltVel;
    private float rotateVel;

    // Reset Variables
    private float _resetTilt;
    private Quaternion _resetRotation;
    private float _resetHeight;

    #endregion

    #region Private Variables

    private Vector3 _newPosition;
    private Quaternion _newRotation;
    /// <summary>
    /// The current value used as high tilt. This is lerped to user input to provide a smooth transcation between 
    /// user view changes.
    /// </summary>
    private float _currentTilt;
    private float _targetTilt;
    private Texture2D cloudTexture;
    #endregion

    #region Properties

    /// <summary>
    /// The delta time to use. If useDeltaTimeToOne is set to true this will return a delta time as if
    /// the time scale were set to one. Otherwise it will return the usual delta time.
    /// </summary>
    float CameraDeltaTime
    {
        get
        {
            if (useDeltaTimeToOne)
                return Time.deltaTime / Mathf.Max(Time.timeScale, 1);
            else
                return Time.deltaTime;
        }
    }

    /// <summary>
    /// Use this to set what postion the Camera should move to
    /// </summary>
    public Vector3 CameraTargetPosition
    {
        get
        {
            if (!shouldFollow)
                return _newPosition;
            else
                return followOffset;
        }
        set
        {
            if (!shouldFollow)
                _newPosition = value;
            else
                followOffset = value;
        }
    }



    #endregion

    #region States

    public enum CloudDrawState
    {
        Disabled,
        PercentOfMaxHeight,
        AtHeight
    }

    public enum AutoAdjustState
    {
        Disabled,
        DistanceToGround,
        Height
    }

    public enum HeightAdjustState
    {
        PreserveHeight,
        ChangeHeight,
    }

    public enum ControlSetup
    {
        Disabled,
        Axis,
        KeyCode
    }

    public enum MiddleMouseSetup
    {
        Disabled,
        Vertical,
        Horizontal,
        Directional,
        Rotate,
        Tilt,
        Orbit
    }
    #endregion

    #region Initialization

    void Start()
    {
        //followTarget = OverseerTarget.startTarget.gameObject.transform;
        _currentTilt = Mathf.Clamp(transform.localEulerAngles.x, lowTilt, highTilt);
        _targetTilt = Mathf.Clamp(transform.localEulerAngles.x, lowTilt, highTilt);
        _newRotation = transform.rotation;
        _newPosition = transform.position;

        _resetTilt = _currentTilt;
        _resetHeight = _newPosition.y;
        _resetRotation = _newRotation;

        cloudTexture = new Texture2D(1, 1);
        cloudTexture.SetPixel(0, 0, Color.white);
        cloudTexture.Apply();
    }

    #endregion

    #region Logic

    public void changeTarget(Transform newTarget)
    {
        currentRotationDelay = rotationDelayOnChange;
        followTarget = newTarget;
    }

    void LateUpdate()
    {
        PerformRTSCamera();
    }

    void PerformRTSCamera()
    {
        if (Input.GetKeyDown(resetKey))
        {
            _currentTilt = _resetTilt;
            _newPosition = new Vector3(_newPosition.x, _resetHeight, _newPosition.z);
            _newRotation = _resetRotation;
        }

        if (Input.GetMouseButtonDown(2) && middleMouseLocksMouse)
            Screen.lockCursor = true;
        if (Input.GetMouseButtonUp(2) && middleMouseLocksMouse)
            Screen.lockCursor = false;


        RaycastHit hit;
        bool rayHit = Physics.Raycast(new Ray(new Vector3(transform.position.x, maxHeightDistance, transform.position.z), -Vector3.up), out hit, Mathf.Infinity, 1 << groundMask.value);

        MouseHeldInput(hit);

        if (!shouldFollow || (shouldFollow && movementAdjustsOffset))
        {
            // Apply Mouse Movement
            DirectionInput(hit);

            // Move the Camera  target position left or right depending on the horizontal input
            HorizontalInput();
            // Move the Camera target position forward or backward depending on the vertical input
            VerticalInput();

            OrbitInput();
        }

        if (shouldFollow)
            FollowCameraTarget();

        EdgeScrolling();

        AdjustPosition(hit);
        if (!shouldLookAt)
            AdjustTilt(hit);

        LookAtCamTarget();

        AdjustRotation();
    }

    #region Controls and Adjustments

    private void EdgeScrolling()
    {

        if (Input.mousePosition.x <= 0 || Input.mousePosition.x >= Screen.width - 1)
        {
            float direction = Input.mousePosition.x <= 0 ? -1 : 1;
            direction *= xEdgeSpeed;
            MoveHorizontal(direction);
        }
        if (Input.mousePosition.y <= 0 || Input.mousePosition.y >= Screen.height - 1)
        {
            float direction = Input.mousePosition.y <= 0 ? -1 : 1;
            direction *= yEdgeSpeed;
            MoveVertical(direction);
        }
    }

    /// <summary>
    /// Adjust the Camera position in the world
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="hit"></param>
    private void AdjustPosition(RaycastHit hit)
    {
        // For use with Preserve height, this is the offset from the ground position to adjust the camera
        float groundHeightOffset = 0.0f;
        float distanceFromGround = _newPosition.y - hit.point.y;

        if (distanceFromGround < minHeightDistance)
        {
            switch (heightAdjustState)
            {
                case HeightAdjustState.ChangeHeight:
                    _newPosition = new Vector3(_newPosition.x, Mathf.Clamp(_newPosition.y, hit.point.y + minHeightDistance, maxHeightDistance), _newPosition.z);
                    break;
                case HeightAdjustState.PreserveHeight:
                    groundHeightOffset = minHeightDistance - distanceFromGround;
                    break;
            }
        }

        // Adjust the transform position taking into account whether it should be smoothed or instantly adjusted
        if (smoothPosition)
        {
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_newPosition.x, _newPosition.y + groundHeightOffset, _newPosition.z), ref moveVel, movementAdjustSpeed, Mathf.Infinity, CameraDeltaTime);
        }
        else
            // Don't smooth and adjust it instantly
            transform.position = new Vector3(_newPosition.x, _newPosition.y + groundHeightOffset, _newPosition.z);
    }
    private void FollowCameraTarget()
    {
        if (followTarget != null)
            _newPosition = followTarget.position + followOffset;
    }

    private void LookAtCamTarget()
    {
        if (followTarget != null)
        {
            if ((currentRotationDelay -= Time.deltaTime) > 0) //while is changing targets
            {
                _newRotation =
                Quaternion.Slerp(_newRotation, Quaternion.LookRotation(followTarget.position - transform.position, Vector3.up), CameraDeltaTime * 0.5f);
            }
            else
            {
                if (currentRotationDelay > -1)
                {
                    _newRotation =
                        Quaternion.Lerp(_newRotation, Quaternion.LookRotation(followTarget.position - transform.position, Vector3.up), Time.deltaTime * 2);
                }
                else
                {
                    _newRotation = Quaternion.LookRotation(followTarget.position - transform.position, Vector3.up);
                }
            }
            _currentTilt = _newRotation.eulerAngles.x;
        }
    }

    /// <summary>
    /// Actually adjust the rotation of the x axis of the camera, i.e, the tilt.
    /// </summary>
    /// <param name="distance"></param>
    private void AdjustTilt(RaycastHit hit)
    {
        float currentDistance = transform.position.y - hit.point.y;
        // Adjust current tilt to the target tilt take into considering whether it should be smooth adjusted or instantly adjusted
        if (smoothTilt)
            //  _currentTilt = Mathf.Lerp(_currentTilt, _targetTilt, tiltAdjustSpeed * CameraDeltaTime);
            _currentTilt = Mathf.SmoothDamp(_currentTilt, _targetTilt, ref tiltVel, tiltAdjustSpeed, Mathf.Infinity, CameraDeltaTime);
        else
            _currentTilt = _targetTilt;

        // Change the tilt based on the auto adjusting state and camera position
        if (autoAdjustState == AutoAdjustState.DistanceToGround && currentDistance <= distanceFromGroundStartAdjust && hit.collider != null)
        {
            _currentTilt = Mathf.Lerp(terrainAdjustTilt, _targetTilt, groundHeightTiltAdjustCurve.Evaluate(GetPercent(distanceFromGroundFinishAdjust, distanceFromGroundStartAdjust, currentDistance) / 100));

        }
        else if (autoAdjustState == AutoAdjustState.Height && transform.position.y <= adjustHeightMax)
        {
            _currentTilt = Mathf.Lerp(terrainAdjustTilt, _targetTilt, heightTiltAdjustCurve.Evaluate(GetPercent(adjustHeightMin, adjustHeightMax, transform.position.y) / 100));
        }
    }

    /// <summary>
    /// Adjusts the rotation of the Camera. Since this method will actually modify the transforms rotation and since tilt is part of rotation, the changes
    /// will be applied here along side rotation around the Y axis.
    /// </summary>
    private void AdjustRotation()
    {
        if (smoothRotate)
        {
            float smoothRot = Mathf.SmoothDampAngle(transform.eulerAngles.y, _newRotation.eulerAngles.y, ref rotateVel, rotateAdjustSpeed, Mathf.Infinity, CameraDeltaTime);
            transform.rotation = Quaternion.Euler(_currentTilt, smoothRot, _newRotation.eulerAngles.z);
        }
        else
            transform.rotation = Quaternion.Euler(_currentTilt, _newRotation.eulerAngles.y, _newRotation.eulerAngles.z);
    }

    #region MouseHeld

    private void MouseHeldInput(RaycastHit hit)
    {
        if (Input.GetMouseButton(2))
        {
            float mouseXInput = clampMiddleMouseInput ? Mathf.Clamp(Input.GetAxis(mouseXAxis), -1, 1) : Input.GetAxis(mouseXAxis);
            float mouseYInput = clampMiddleMouseInput ? Mathf.Clamp(Input.GetAxis(mouseYAxis), -1, 1) : Input.GetAxis(mouseYAxis);
            mouseXInput *= middleMouseInputMultiplier;
            mouseYInput *= middleMouseInputMultiplier;



            MoveMouseHeld(mouseXSetup, mouseXInput * (invertMouseX ? -1 : 1), hit);
            MoveMouseHeld(mouseYSetup, mouseYInput * (invertMouseY ? -1 : 1), hit);
        }

    }

    private void MoveMouseHeld(MiddleMouseSetup setup, float direction, RaycastHit hit)
    {
        switch (setup)
        {
            case MiddleMouseSetup.Directional:
                MoveDirection(direction, hit);
                break;
            case MiddleMouseSetup.Tilt:
                MoveTilt(tiltSpeed * direction);
                break;
            case MiddleMouseSetup.Rotate:
                MoveRotate(rotateSpeed * direction);
                break;
            case MiddleMouseSetup.Vertical:
                MoveVertical(direction * movementSpeed);
                break;
            case MiddleMouseSetup.Horizontal:
                MoveHorizontal(direction * movementSpeed);
                break;
            case MiddleMouseSetup.Orbit:
                MoveOrbitX(direction * movementSpeed);
                break;
        }
    }

    #endregion


    #region Direction
    /// <summary>
    /// Modify the position the camera should move to via Mouse input
    /// </summary>
    private void DirectionInput(RaycastHit hit)
    {
        switch (directionSetup)
        {
            case ControlSetup.Axis:
                // Only adjust the direction if we have an input. This way if our Height Adjust State is set to preserve height, it will preserve the height
                // And when we do actually want to adjust the position it will set the _newPosition to the minimum distance above the terrain as if
                // the Height Adjust State were set to change height
                if (Input.GetAxis(directionAxis) != 0)
                    MoveDirection(Input.GetAxis(directionAxis), hit);

                break;

            case ControlSetup.KeyCode:
                // Only adjust the direction if we have an input. This way if our Height Adjust State is set to preserve height, it will preserve the height
                // And when we do actually want to adjust the position it will set the _newPosition to the minimum distance above the terrain as if
                // the Height Adjust State were set to change height
                if (Input.GetKey(directionPositiveKey) || Input.GetKey(directionNegativeKey))
                {
                    if (Input.GetKey(directionPositiveKey))
                        MoveDirection(1, hit);

                    if (Input.GetKey(directionNegativeKey))
                        MoveDirection(-1, hit);

                }
                break;
        }

    }

    private void MoveDirection(float direction, RaycastHit hit)
    {
        Vector3 moveAmount = Quaternion.Euler(0, transform.localEulerAngles.y, transform.localEulerAngles.z) * Vector3.Scale(directionToMove, directionMultiplier) * direction * CameraDeltaTime;

        // The current minimum point that the camera is over
        float minPoint = hit.point.y + minHeightDistance;

        Vector3 moveTest = CameraTargetPosition + moveAmount;

        // If the movement amount brings the target position outside the bounds of the min or max height,
        // adjust the move amount to bring it exactly on par with either the min or max height.
        // This means scaling the move vector to adjust to the proper movement difference.
        if (moveTest.y < minPoint || moveTest.y > maxHeightDistance)
        {
            float positionDifference;
            if (moveTest.y < minPoint)
                positionDifference = CameraTargetPosition.y - minPoint;
            else
                positionDifference = maxHeightDistance - CameraTargetPosition.y;

            float percentDifference = Mathf.Abs(GetPercent(positionDifference, moveAmount.y)) / 100f;

            moveTest = CameraTargetPosition + (moveAmount * percentDifference);
            moveTest = new Vector3(moveTest.x, Mathf.Clamp(moveTest.y, minPoint, maxHeightDistance), moveTest.z);
        }

        CameraTargetPosition = moveTest;

        //Clamp the target Position
    }

    #endregion

    #region Orbit

    private void OrbitInput()
    {
        switch (orbitSetup)
        {
            case ControlSetup.Axis:
                MoveOrbitX(Input.GetAxis(orbitAxis) * movementSpeed);
                break;
            case ControlSetup.KeyCode:
                if (Input.GetKey(orbitLeftKey))
                    MoveOrbitX(-rotateSpeed);
                if (Input.GetKey(orbitRightKey))
                    MoveOrbitX(rotateSpeed);
                break;
        }
    }

    private void MoveOrbitX(float speed)
    {
        if (followTarget != null)
        {
            Vector3 offset = shouldFollow ? followTarget.transform.position : Vector3.zero;
            CameraTargetPosition = RotateAroundPoint(offset + CameraTargetPosition, followTarget.position, Vector3.up, speed * CameraDeltaTime) - offset;
        }
    }

    #endregion

    #region Vertical

    /// <summary>
    /// Modify the postion the camera should move to via vertical input. Eg: the vertical axis or the forward and backwards keys
    /// </summary>
    private void VerticalInput()
    {
        switch (verticalSetup)
        {
            case ControlSetup.Axis:
                MoveVertical(Input.GetAxis(verticalAxis) * movementSpeed);
                MoveVerticalTopDown(Input.GetAxis(tiltAxis) * movementSpeed);
                break;
            case ControlSetup.KeyCode:
                if (Input.GetKey(forwardKey))
                    MoveVertical(movementSpeed);
                if (Input.GetKey(backwardKey))
                    MoveVertical(-movementSpeed);
                break;
        }
    }

    private void MoveVertical(float speed)
    {
        CameraTargetPosition += Quaternion.Euler(0, transform.localEulerAngles.y, 0) * Vector3.forward * speed * CameraDeltaTime;
    }

    private void MoveVerticalTopDown(float speed)
    {
        CameraTargetPosition += Quaternion.Euler(0, transform.localEulerAngles.y, 0) * Vector3.up * speed * CameraDeltaTime;
    }


    #endregion

    #region Horizontal

    /// <summary>
    /// Modify the postion the camera should move to via horizontal input. Eg: the horizontal axis or the left and right keys
    /// </summary>
    private void HorizontalInput()
    {
        switch (horizontalSetup)
        {
            case ControlSetup.Axis:
                MoveHorizontal(Input.GetAxis(horizontalAxis) * movementSpeed);
                break;
            case ControlSetup.KeyCode:
                if (Input.GetKey(leftKey))
                    MoveHorizontal(-movementSpeed);

                if (Input.GetKey(rightKey))
                    MoveHorizontal(movementSpeed);
                break;
        }
    }

    private void MoveHorizontal(float speed)
    {
        CameraTargetPosition += Quaternion.Euler(0, transform.localEulerAngles.y, 0) * Vector3.right * speed * CameraDeltaTime;
    }

    #endregion

    #region Rotate

    private void RotateInput()
    {
        switch (rotateSetup)
        {
            case ControlSetup.Axis:
                MoveRotate(Input.GetAxis(rotateAxis) * rotateSpeed);
                break;
            case ControlSetup.KeyCode:
                if (Input.GetKey(rotateLeftKey))
                    MoveRotate(-rotateSpeed);
                if (Input.GetKey(rotateRightKey))
                    MoveRotate(rotateSpeed);
                break;

        }
    }

    private void MoveRotate(float speed)
    {
        if (!shouldLookAt)
            _newRotation = Quaternion.AngleAxis(speed * CameraDeltaTime, Vector3.up) * _newRotation;
    }

    #endregion

    #region Tilt

    /// <summary>
    /// Automatically adjust the tilt depending how close the camera is to the ground
    /// </summary>
    private void TiltInput()
    {
        // Adjust the target tilt based on user input
        switch (tiltSetup)
        {
            case ControlSetup.Axis:
                //MoveTilt(Input.GetAxis(tiltAxis));
                break;
            case ControlSetup.KeyCode:
                if (Input.GetKey(tiltDecKey))
                    MoveTilt(-tiltSpeed);
                if (Input.GetKey(tiltIncKey))
                    MoveTilt(tiltSpeed);
                break;
        }
    }

    private void MoveTilt(float speed)
    {
        //if (!shouldLookAt)
        _targetTilt = Mathf.Clamp(_targetTilt + speed * CameraDeltaTime, lowTilt, highTilt);
    }

    #endregion

    #endregion

    // Draw the clouds texture
    private void OnGUI()
    {
        if (cloudDrawState == CloudDrawState.Disabled)
            return;

        float min = 0.0f, max = 0.0f;
        switch (cloudDrawState)
        {
            case CloudDrawState.PercentOfMaxHeight:
                min = transform.position.y - PercentToNumber(cloudStartAtHeightPercent, maxHeightDistance);
                max = PercentToNumber(cloudFinishAtHeightPercent, maxHeightDistance) - PercentToNumber(cloudStartAtHeightPercent, maxHeightDistance);
                break;
            case CloudDrawState.AtHeight:
                min = transform.position.y - cloudStartAtHeight;
                max = cloudFinishAtHeight - cloudStartAtHeight;
                break;
        }


        // Get the alpha value to be displayed depending on the camera's position.
        //Also Ensure min doesn't go over max so we never get a percent value of over 100%
        GUI.color = new Color(1, 1, 1, (GetPercent(Mathf.Clamp(min, 0, max), max) / 100) * cloudMaxAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), cloudTexture);

    }

    #region HelperMethods

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
    {
        return Quaternion.Euler(axis * angle) * (point - pivot) + pivot;
    }

    /// <summary>
    /// Compares "a" and "maxNumber" and returns what percent "a" is of "maxNumber".
    /// </summary>
    /// <param name="a"></param>
    /// <param name="maxNumber"></param>
    /// <returns></returns>
    private float GetPercent(float a, float maxNumber)
    {
        return (a / maxNumber) * 100;
    }

    private float GetPercent(float min, float max, float a)
    {
        max -= min;
        a -= min;
        return GetPercent(a, max);
    }

    /// <summary>
    /// Takes in a percent value and returns the percent of the number passed in.
    /// Eg percent = 50, a = 200. 50% of a = 100
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    private float PercentToNumber(float percent, float a)
    {
        return (percent / 100) * a;
    }

    void GetCloudStartToCloudEndMinMax(out float min, out float max)
    {
        min = transform.position.y - PercentToNumber(cloudStartAtHeightPercent, maxHeightDistance);
        max = maxHeightDistance - PercentToNumber(cloudStartAtHeightPercent, maxHeightDistance);
    }

    #endregion

    #endregion


}
