using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(RTSCamera))]
public class RTSCameraInspector : Editor
{
    #region Inspector Variables

    #region Tabs Foldouts
    private bool controlTabFoldout = false;
    private bool camConfigTabFoldout = false;
    private bool behaviourTabFoldout = false;
    #endregion

    #region Behaviour Foldouts
    private bool autoAdjustBehavFoldout = false;
    private bool smoothBehavFoldout = false;
    private bool followBehavFoldout = false;
    private bool heightAdjustBehavFoldout = false;
    private bool edgeScrollingBehavFoldout = false;
    private bool cloudBehavFoldout = false;

    #endregion

    #region Controls Foldout
    private bool verticalContFoldout = false;
    private bool horizontalContFoldout = false;
    private bool rotateContFoldout = false;
    private bool tiltContFoldout = false;
    private bool directionalContFoldout = false;
    private bool middleMouseContFoldout = false;
    private bool miscContFoldout = false;
    private bool mouseContFoldout = false;
    #endregion

    #region Camera Config Foldout
    //private bool speedConfFoldout = false;
    //private bool tiltConfFoldout = false;
    //private bool heightConfFoldout = false;
    //private bool rotateConfFoldout = false;
    #endregion

    #region SerializedProperties

    #region Control Properties

    private SerializedProperty verticalSetup;
    private SerializedProperty verticalAxis;
    private SerializedProperty forwardKey;
    private SerializedProperty backwardKey;

    private SerializedProperty horizontalSetup;
    private SerializedProperty horizontalAxis;
    private SerializedProperty moveLeftKey;
    private SerializedProperty moveRightKey;

    private SerializedProperty rotateSetup;
    private SerializedProperty rotateAxis;
    private SerializedProperty rotateLeftKey;
    private SerializedProperty rotateRightKey;

    private SerializedProperty tiltSetup;
    private SerializedProperty tiltAxis;
    private SerializedProperty tiltIncKey;
    private SerializedProperty tiltDecKey;

    private SerializedProperty orbitSetup;
    private SerializedProperty orbitAxis;
    private SerializedProperty orbitLeftKey;
    private SerializedProperty orbitRightKey;

    private SerializedProperty directionalSetup;
    private SerializedProperty directionalAxis;
    private SerializedProperty directionalPositiveKey;
    private SerializedProperty directionalNegativeKey;
    private SerializedProperty directionToMove;
    private SerializedProperty directionMultiplier;

    private SerializedProperty middleMouseLocksCursor;
    private SerializedProperty invertMouseX;
    private SerializedProperty invertMouseY;
    private SerializedProperty mouseXAxis;
    private SerializedProperty mouseYAxis;
    private SerializedProperty mouseXSetup;
    private SerializedProperty mouseYSetup;
    private SerializedProperty clampMiddleMouseInput;
    private SerializedProperty middleMouseInputMultiplier;
    private SerializedProperty invertOrbit;


    private SerializedProperty resetKey;

    #endregion

    #region Conf Properties
    private SerializedProperty movementSpeed;
    private SerializedProperty rotateSpeed;
    private SerializedProperty tiltSpeed;
    private SerializedProperty minTilt;
    private SerializedProperty maxTilt;
    private SerializedProperty minHeight;
    private SerializedProperty maxHeight;
    private SerializedProperty delaTimeIgnoresTimeScale;
    private SerializedProperty groundLayer;
    #endregion

    #region Behaviour Properties

    private SerializedProperty autoAdjustState;
    private SerializedProperty terrainAdjustTilt;
    private SerializedProperty distanceFromGroundFinishAdjust;
    private SerializedProperty distanceFromGroundStartAdjust;
    private SerializedProperty groundHeightTiltAdjustCurve;
    private SerializedProperty adjustHeightMin;
    private SerializedProperty adjustHeightMax;
    private SerializedProperty heightTiltAdjustCurve;

    private SerializedProperty smoothPosition;
    private SerializedProperty movementAdjustSpeed;
    private SerializedProperty smoothRotate;
    private SerializedProperty rotateAdjustSpeed;
    private SerializedProperty smoothTilt;
    private SerializedProperty tiltAdjustSpeed;

    private SerializedProperty heightAdjustState;

    private SerializedProperty followTarget;
    private SerializedProperty followOffset;
    private SerializedProperty shouldFollow;
    private SerializedProperty shouldLookAt;
    private SerializedProperty movementAdjustsOffset;

    private SerializedProperty xEdgeSpeed;
    private SerializedProperty yEdgeSpeed;

    private SerializedProperty cloudDrawState;
    private SerializedProperty cloudStartAtHeightPercent;
    private SerializedProperty cloudFinishAtHeightPercent;
    private SerializedProperty cloudStartAtHeight;
    private SerializedProperty cloudFinishAtHeight;
    private SerializedProperty cloudMaxAlpha;



    #endregion

    #endregion

    private int directionContPreset = 0;
    private RTSCamera RTSCameraInstance;
    #endregion

    void OnEnable()
    {
        RTSCameraInstance = target as RTSCamera;

        //Configuration Properties
        movementSpeed = serializedObject.FindProperty("movementSpeed");
        rotateSpeed = serializedObject.FindProperty("rotateSpeed");
        tiltSpeed = serializedObject.FindProperty("tiltSpeed");
        minTilt = serializedObject.FindProperty("lowTilt");
        maxTilt = serializedObject.FindProperty("highTilt");
        minHeight = serializedObject.FindProperty("minHeightDistance");
        maxHeight = serializedObject.FindProperty("maxHeightDistance");
        delaTimeIgnoresTimeScale = serializedObject.FindProperty("useDeltaTimeToOne");
        groundLayer = serializedObject.FindProperty("groundMask");

        //Controller Properties
        verticalSetup = serializedObject.FindProperty("verticalSetup");
        verticalAxis = serializedObject.FindProperty("verticalAxis");
        forwardKey = serializedObject.FindProperty("forwardKey");
        backwardKey = serializedObject.FindProperty("backwardKey");

        horizontalSetup = serializedObject.FindProperty("horizontalSetup");
        horizontalAxis = serializedObject.FindProperty("horizontalAxis");
        moveLeftKey = serializedObject.FindProperty("leftKey");
        moveRightKey = serializedObject.FindProperty("rightKey");

        rotateSetup = serializedObject.FindProperty("rotateSetup");
        rotateAxis = serializedObject.FindProperty("rotateAxis");
        rotateLeftKey = serializedObject.FindProperty("rotateLeftKey");
        rotateRightKey = serializedObject.FindProperty("rotateRightKey");

        tiltSetup = serializedObject.FindProperty("tiltSetup");
        tiltAxis = serializedObject.FindProperty("tiltAxis");
        tiltIncKey = serializedObject.FindProperty("tiltIncKey");
        tiltDecKey = serializedObject.FindProperty("tiltDecKey");

        orbitSetup = serializedObject.FindProperty("orbitSetup");
        orbitAxis = serializedObject.FindProperty("orbitAxis");
        orbitLeftKey = serializedObject.FindProperty("orbitLeftKey");
        orbitRightKey = serializedObject.FindProperty("orbitRightKey");

        directionalSetup = serializedObject.FindProperty("directionSetup");
        directionalAxis = serializedObject.FindProperty("directionAxis");
        directionalPositiveKey = serializedObject.FindProperty("directionPositiveKey");
        directionalNegativeKey = serializedObject.FindProperty("directionNegativeKey");
        directionToMove = serializedObject.FindProperty("directionToMove");
        directionMultiplier = serializedObject.FindProperty("directionMultiplier");

        middleMouseLocksCursor = serializedObject.FindProperty("middleMouseLocksMouse");
        invertMouseX = serializedObject.FindProperty("invertMouseX");
        invertOrbit = serializedObject.FindProperty("invertOrbit");
        invertMouseY = serializedObject.FindProperty("invertMouseY");
        mouseXAxis = serializedObject.FindProperty("mouseXAxis");
        mouseYAxis = serializedObject.FindProperty("mouseYAxis");
        mouseXSetup = serializedObject.FindProperty("mouseXSetup");
        mouseYSetup = serializedObject.FindProperty("mouseYSetup");
        clampMiddleMouseInput = serializedObject.FindProperty("clampMiddleMouseInput");
        middleMouseInputMultiplier = serializedObject.FindProperty("middleMouseInputMultiplier");

        resetKey = serializedObject.FindProperty("resetKey");

        //Behaviour Properties
        autoAdjustState = serializedObject.FindProperty("autoAdjustState");
        terrainAdjustTilt = serializedObject.FindProperty("terrainAdjustTilt");
        distanceFromGroundFinishAdjust = serializedObject.FindProperty("distanceFromGroundFinishAdjust");
        distanceFromGroundStartAdjust = serializedObject.FindProperty("distanceFromGroundStartAdjust");
        groundHeightTiltAdjustCurve = serializedObject.FindProperty("groundHeightTiltAdjustCurve");
        adjustHeightMin = serializedObject.FindProperty("adjustHeightMin");
        adjustHeightMax = serializedObject.FindProperty("adjustHeightMax");
        heightTiltAdjustCurve = serializedObject.FindProperty("heightTiltAdjustCurve");

        smoothPosition = serializedObject.FindProperty("smoothPosition");
        movementAdjustSpeed = serializedObject.FindProperty("movementAdjustSpeed");
        smoothRotate = serializedObject.FindProperty("smoothRotate");
        rotateAdjustSpeed = serializedObject.FindProperty("rotateAdjustSpeed");
        smoothTilt = serializedObject.FindProperty("smoothTilt");
        tiltAdjustSpeed = serializedObject.FindProperty("tiltAdjustSpeed");

        heightAdjustState = serializedObject.FindProperty("heightAdjustState");

        followTarget = serializedObject.FindProperty("followTarget");
        followOffset = serializedObject.FindProperty("followOffset");
        shouldFollow = serializedObject.FindProperty("shouldFollow");
        shouldLookAt = serializedObject.FindProperty("shouldLookAt");
        movementAdjustsOffset = serializedObject.FindProperty("movementAdjustsOffset");

        xEdgeSpeed = serializedObject.FindProperty("xEdgeSpeed");
        yEdgeSpeed = serializedObject.FindProperty("yEdgeSpeed");

        cloudDrawState = serializedObject.FindProperty("cloudDrawState");
        cloudStartAtHeightPercent = serializedObject.FindProperty("cloudStartAtHeightPercent");
        cloudFinishAtHeightPercent = serializedObject.FindProperty("cloudFinishAtHeightPercent");
        cloudStartAtHeight = serializedObject.FindProperty("cloudStartAtHeight");
        cloudFinishAtHeight = serializedObject.FindProperty("cloudFinishAtHeight");
        cloudMaxAlpha = serializedObject.FindProperty("cloudMaxAlpha");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.EndHorizontal();

        GUIStyle smallButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        smallButtonStyle.fixedWidth = 50;

        GUIStyle helpButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        helpButtonStyle.fixedWidth = 25;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Controls", EditorStyles.toolbarDropDown)) controlTabFoldout = !controlTabFoldout;
        if (GUILayout.Button("[+]", smallButtonStyle))
        {
            controlTabFoldout = true;

            horizontalContFoldout = true;
            rotateContFoldout = true;
            tiltContFoldout = true;
            verticalContFoldout = true;
            directionalContFoldout = true;
            middleMouseContFoldout = true;
            miscContFoldout = true;
        }
        if (GUILayout.Button("[-]", smallButtonStyle))
        {
            controlTabFoldout = false;

            horizontalContFoldout = false;
            rotateContFoldout = false;
            tiltContFoldout = false;
            verticalContFoldout = false;
            directionalContFoldout = false;
            middleMouseContFoldout = false;
            miscContFoldout = false;
        }

        EditorGUILayout.EndHorizontal();
        if (controlTabFoldout)
        {
            ControlEditor();
        }


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Behaviour", EditorStyles.toolbarDropDown)) behaviourTabFoldout = !behaviourTabFoldout;

        if (GUILayout.Button("[+]", smallButtonStyle))
        {
            behaviourTabFoldout = true;

            autoAdjustBehavFoldout = true;
            smoothBehavFoldout = true;
            followBehavFoldout = true;
            heightAdjustBehavFoldout = true;
            edgeScrollingBehavFoldout = true;
            cloudBehavFoldout = true;


        }
        if (GUILayout.Button("[-]", smallButtonStyle))
        {
            behaviourTabFoldout = false;

            autoAdjustBehavFoldout = false;
            smoothBehavFoldout = false;
            followBehavFoldout = false;
            heightAdjustBehavFoldout = false;
            edgeScrollingBehavFoldout = false;
            cloudBehavFoldout = false;
        }


        EditorGUILayout.EndHorizontal();
        if (behaviourTabFoldout)
        {
            BehaviourEditor();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Camera Configuration", EditorStyles.toolbarDropDown)) camConfigTabFoldout = !camConfigTabFoldout;
        if (GUILayout.Button("[+]", smallButtonStyle))
        {
            camConfigTabFoldout = true;

            //heightConfFoldout = true;
            //speedConfFoldout = true;
            //tiltConfFoldout = true;
        }
        if (GUILayout.Button("[-]", smallButtonStyle))
        {
            camConfigTabFoldout = false;

            //heightConfFoldout = false;
            //speedConfFoldout = false;
            //tiltConfFoldout = false;
        }

        EditorGUILayout.EndHorizontal();
        if (camConfigTabFoldout)
        {
            CameraConfigEditor();
        }



        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private void ControlEditor()
    {

        #region Vertical
        EditorGUILayout.BeginVertical("box");

        verticalContFoldout = GUILayout.Toggle(verticalContFoldout, new GUIContent("Vertical Controls", "Control the forward and backward movements of the Camera"), EditorStyles.foldout);

        if (verticalContFoldout)
        {
            EditorGUI.indentLevel++;



            GUILayout.BeginHorizontal();
            //RTSCameraInstance.verticalSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Vertical Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.verticalSetup);
            EditorGUILayout.PropertyField(verticalSetup, new GUIContent("Vertical Setup", "The control setup, should this movement use an axis or should keys be specified"));
            GUILayout.EndHorizontal();

            if (RTSCameraInstance.verticalSetup == RTSCamera.ControlSetup.Axis)
            {

                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.verticalAxis = EditorGUILayout.TextField(new GUIContent("Vertical Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.verticalAxis);
                EditorGUILayout.PropertyField(verticalAxis, new GUIContent("Vertical Axis", "Specify the name of the axis to use, to control this movement"));
                GUILayout.EndHorizontal();
            }
            else if (RTSCameraInstance.verticalSetup == RTSCamera.ControlSetup.KeyCode)
            {
                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.forwardKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key (Forward)", RTSCameraInstance.forwardKey);
                EditorGUILayout.PropertyField(forwardKey, new GUIContent("Forward Key", "Controls forward movement"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                // RTSCameraInstance.backwardKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key (Backward)", RTSCameraInstance.backwardKey);
                EditorGUILayout.PropertyField(backwardKey, new GUIContent("Backward Key", "Controls backward movement"));
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Horizontal
        EditorGUILayout.BeginVertical("box");

        horizontalContFoldout = GUILayout.Toggle(horizontalContFoldout, new GUIContent("Horizontal Controls", "Control the left and right movements of the Camera"), EditorStyles.foldout);
        if (horizontalContFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //   RTSCameraInstance.horizontalSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Horizontal Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.horizontalSetup);
            EditorGUILayout.PropertyField(horizontalSetup, new GUIContent("Horizontal Setup", "The control setup, should this movement use an axis or should keys be specified"));
            GUILayout.EndHorizontal();


            if (RTSCameraInstance.horizontalSetup == RTSCamera.ControlSetup.Axis)
            {

                GUILayout.BeginHorizontal();
                // RTSCameraInstance.horizontalAxis = EditorGUILayout.TextField(new GUIContent("Horizontal Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.horizontalAxis);
                EditorGUILayout.PropertyField(horizontalAxis, new GUIContent("Horizontal Axis", "Specify the name of the axis to use, to control this movement"));
                GUILayout.EndHorizontal();
            }
            else if (RTSCameraInstance.horizontalSetup == RTSCamera.ControlSetup.KeyCode)
            {
                GUILayout.BeginHorizontal();
                //  RTSCameraInstance.leftKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key (Left)", RTSCameraInstance.leftKey);
                EditorGUILayout.PropertyField(moveLeftKey, new GUIContent("Left Key", "Controls left movement"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                //  RTSCameraInstance.rightKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key (Right)", RTSCameraInstance.rightKey);
                EditorGUILayout.PropertyField(moveRightKey, new GUIContent("Right Key", "Controls right movement"));
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Rotate
        EditorGUILayout.BeginVertical("box");

        rotateContFoldout = GUILayout.Toggle(rotateContFoldout, new GUIContent("Rotate Controls", "Controls Camera Rotation, i.e, looking around the current direction"), EditorStyles.foldout);
        if (rotateContFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //  RTSCameraInstance.rotateYSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Rotate Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.rotateYSetup);
            EditorGUILayout.PropertyField(rotateSetup, new GUIContent("Rotate Setup", "The control setup, should this movement use an axis or should keys be specified"));
            GUILayout.EndHorizontal();


            if (RTSCameraInstance.rotateSetup == RTSCamera.ControlSetup.Axis)
            {

                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.rotateAxis = EditorGUILayout.TextField(new GUIContent("Rotate Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.rotateAxis);
                EditorGUILayout.PropertyField(rotateAxis, new GUIContent("Rotate Axis", "Specify the name of the axis to use, to control this movement"));
                GUILayout.EndHorizontal();
            }
            else if (RTSCameraInstance.rotateSetup == RTSCamera.ControlSetup.KeyCode)
            {
                GUILayout.BeginHorizontal();
                //RTSCameraInstance.rotateLeftKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key (Left)", RTSCameraInstance.rotateLeftKey);
                EditorGUILayout.PropertyField(rotateLeftKey, new GUIContent("Rotate Left", "Rotates the Camera left"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                //  RTSCameraInstance.rotateRightKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key (Right)", RTSCameraInstance.rotateRightKey);
                EditorGUILayout.PropertyField(rotateRightKey, new GUIContent("Rotate Right", "Rotates the Camera right"));
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Tilt
        EditorGUILayout.BeginVertical("box");

        tiltContFoldout = GUILayout.Toggle(tiltContFoldout, new GUIContent("Tilt Controls", "Control the camera tilting, i.e, looking up (Max Tilt) and looking down (Min Tilt)"), EditorStyles.foldout);
        if (tiltContFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //    RTSCameraInstance.tiltSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Tilt Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.tiltSetup);
            EditorGUILayout.PropertyField(tiltSetup, new GUIContent("Tilt Setup", "The control setup, should this movement use an axis or should keys be specified"));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(invertOrbit, new GUIContent("Invert Orbit X", "An easy way to invert orbit X input"));

            GUILayout.EndHorizontal();


            if (RTSCameraInstance.tiltSetup == RTSCamera.ControlSetup.Axis)
            {

                GUILayout.BeginHorizontal();
                //  RTSCameraInstance.tiltAxis = EditorGUILayout.TextField(new GUIContent("Tilt Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.tiltAxis);
                EditorGUILayout.PropertyField(tiltAxis, new GUIContent("Tilt Axis", "Specify the name of the axis to use, to control this movement"));
                GUILayout.EndHorizontal();
            }
            else if (RTSCameraInstance.tiltSetup == RTSCamera.ControlSetup.KeyCode)
            {

                GUILayout.BeginHorizontal();
                //   RTSCameraInstance.tiltIncKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key (Up)", RTSCameraInstance.tiltIncKey);
                EditorGUILayout.PropertyField(tiltIncKey, new GUIContent("Increase Tilt", "Tilt upwards"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                //     RTSCameraInstance.tiltDecKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key (Down)", RTSCameraInstance.tiltDecKey);
                EditorGUILayout.PropertyField(tiltDecKey, new GUIContent("Decrease Tilt", "Tilt downwards"));
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Orbit
        EditorGUILayout.BeginVertical("box");

        tiltContFoldout = GUILayout.Toggle(tiltContFoldout, new GUIContent("Orbit Controls", "Control the camera orbiting around a point"), EditorStyles.foldout);
        if (tiltContFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //     RTSCameraInstance.orbitSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Orbit Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.orbitSetup);
            EditorGUILayout.PropertyField(orbitSetup, new GUIContent("Orbit Setup", "The control setup, should this movement use an axis or should keys be specified"));
            GUILayout.EndHorizontal();


            if (RTSCameraInstance.orbitSetup == RTSCamera.ControlSetup.Axis)
            {

                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.orbitAxis = EditorGUILayout.TextField(new GUIContent("Orbit Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.tiltAxis);
                EditorGUILayout.PropertyField(orbitAxis, new GUIContent("Orbit Axis", "Specify the name of the axis to use, to control this movement"));
                GUILayout.EndHorizontal();
            }
            else if (RTSCameraInstance.orbitSetup == RTSCamera.ControlSetup.KeyCode)
            {

                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.orbitLeftKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key (Left)", RTSCameraInstance.orbitLeftKey);
                EditorGUILayout.PropertyField(orbitLeftKey, new GUIContent("Orbit Left", "Orbit around to the left"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                //    RTSCameraInstance.orbitRightKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key (Right)", RTSCameraInstance.orbitRightKey);
                EditorGUILayout.PropertyField(orbitRightKey, new GUIContent("Orbit Right", "Orbit around to the right"));
                GUILayout.EndHorizontal();

            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region DirectionalControl
        EditorGUILayout.BeginVertical("box");

        directionalContFoldout = GUILayout.Toggle(directionalContFoldout, new GUIContent("Direction Controls", "Move the camera in a user specified local direction"), EditorStyles.foldout);
        if (directionalContFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //   RTSCameraInstance.directionSetup = (RTSCamera.ControlSetup)EditorGUILayout.EnumPopup(new GUIContent("Direction Setup", "The control setup, should this movement use an axis or should keys be specified"), RTSCameraInstance.directionSetup);
            EditorGUILayout.PropertyField(directionalSetup, (new GUIContent("Direction Setup", "The control setup, should this movement use an axis or should keys be specified")));
            GUILayout.EndHorizontal();

            switch (RTSCameraInstance.directionSetup)
            {

                case RTSCamera.ControlSetup.Axis:

                    GUILayout.BeginHorizontal();
                    //   RTSCameraInstance.directionAxis = EditorGUILayout.TextField(new GUIContent("Direction Axis", "Specify the name of the axis to use, to control this movement"), RTSCameraInstance.directionAxis);
                    EditorGUILayout.PropertyField(directionalAxis, new GUIContent("Direction Axis", "Specify the name of the axis to use, to control this movement"));
                    GUILayout.EndHorizontal();
                    break;
                case RTSCamera.ControlSetup.KeyCode:

                    GUILayout.BeginHorizontal();
                    // RTSCameraInstance.directionPositiveKey = (KeyCode)EditorGUILayout.EnumPopup("Positive Key", RTSCameraInstance.directionPositiveKey);
                    EditorGUILayout.PropertyField(directionalPositiveKey, new GUIContent("Positive Key", "The key to move in the positive direction"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    //  RTSCameraInstance.directionNegativeKey = (KeyCode)EditorGUILayout.EnumPopup("Negative Key", RTSCameraInstance.directionNegativeKey);
                    EditorGUILayout.PropertyField(directionalNegativeKey, new GUIContent("Negative Key", "The key to move in the negative direction"));
                    GUILayout.EndHorizontal();
                    break;
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            directionToMove.vector3Value = ClampVector(EditorGUILayout.Vector3Field(new GUIContent("Move Direction", "The direction to move the camera in response to the input i.e. when the input is positive it will move in this direction times the direction multiplier and when the input is negative it will move in the inverse direction times the input multiplier"), RTSCameraInstance.directionToMove));
            // EditorGUILayout.PropertyField(directionToMove, new GUIContent("Move Direction", "The direction to move the camera in response to the input i.e. when the input is positive it will move in this direction times the direction multiplier and when the input is negative it will move in the inverse direction times the input multiplier"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            //   RTSCameraInstance.directionMultiplier = EditorGUILayout.Vector3Field(new GUIContent("Direction Multiplier", "Think of this as direction speed. The camera will move in the previously specified direction times this amount"), RTSCameraInstance.directionMultiplier);
            EditorGUILayout.PropertyField(directionMultiplier, new GUIContent("Direction Multiplier", "Think of this as direction speed. The camera will move in the previously specified direction times this amount"));
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Middle Mouse Control
        EditorGUILayout.BeginVertical("box");

        middleMouseContFoldout = GUILayout.Toggle(middleMouseContFoldout, new GUIContent("Middle Mouse Controls", "Configure what movements should be executed while the middle mouse button is held."), EditorStyles.foldout);
        if (middleMouseContFoldout)
        {
            EditorGUI.indentLevel++;


            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(clampMiddleMouseInput, new GUIContent("Clamp Mouse Input", "Should mouse input be clamped from -1 to 1"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(middleMouseInputMultiplier, new GUIContent("Mouse Input Multiplier", "Multiply input recieved from the mouse axis by this amount"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(middleMouseLocksCursor, new GUIContent("Middle Mouse Locks Cursor", "Should the mouse cursor be locked when the middle mouse is held down and unlocked when it is up."));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(invertMouseX, new GUIContent("Invert Mouse X", "An easy way to invert mouse X input"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(invertMouseY, new GUIContent("Invert Mouse Y", "An easy way to invert mouse Y input"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(mouseXAxis, new GUIContent("Mouse X Axis", "The Axis to use when the mouse is held down. By default this is Mouse X but it can be adjusted to suit your setup"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(mouseYAxis, new GUIContent("Mouse Y Axis", "The Axis to use when the mouse is held down. By default this is Mouse Y but it can be adjusted to suit your setup"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(mouseXSetup, new GUIContent("Mouse X Controls", "What type of movement should the X axis of the mouse control"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(mouseYSetup, new GUIContent("Mouse Y Controls", "What type of movement should the Y axis of the mouse control"));
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Middle Mouse Control
        EditorGUILayout.BeginVertical("box");

        mouseContFoldout = GUILayout.Toggle(mouseContFoldout, new GUIContent("Mouse Controls", "Configure what movements should be executed while the left or right mouse buttons are held"), EditorStyles.foldout);
        if (mouseContFoldout)
        {
            EditorGUI.indentLevel++;

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region Misc Controls
        EditorGUILayout.BeginVertical("box");

        miscContFoldout = GUILayout.Toggle(miscContFoldout, new GUIContent("Misc Controls", "Miscellaneous controls for the camera."), EditorStyles.foldout);
        if (miscContFoldout)
        {
            EditorGUI.indentLevel++;


            GUILayout.BeginHorizontal();
            //  RTSCameraInstance.resetKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Reset Key", "When this key is pressed all of the Camera's values will reset to the initial Camera's setup. This only includes Camera Configuration variables an not behavioural or control variables"), RTSCameraInstance.resetKey);
            EditorGUILayout.PropertyField(resetKey, new GUIContent("Reset Key", "When this key is pressed all of the Camera's configuration values will reset to the initial Camera's setup."));
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        #endregion

    }

    private void BehaviourEditor()
    {
        #region Auto Adjust
        EditorGUILayout.BeginVertical("box");

        autoAdjustBehavFoldout = GUILayout.Toggle(autoAdjustBehavFoldout, "Tilt Auto Adjust", EditorStyles.foldout);
        if (autoAdjustBehavFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //  RTSCameraInstance.autoAdjustState = (RTSCamera.AutoAdjustState)EditorGUILayout.EnumPopup("Auto Adjust State", RTSCameraInstance.autoAdjustState);
            EditorGUILayout.PropertyField(autoAdjustState, new GUIContent("Auto Adjust State", "The auto adjust state that should be used. This can either adjust to height above ground or just a general camera height"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            // RTSCameraInstance.terrainAdjustTilt = EditorGUILayout.FloatField("Adjust To Tilt", RTSCameraInstance.terrainAdjustTilt);
            EditorGUILayout.PropertyField(terrainAdjustTilt, new GUIContent("Adjust To Tilt", "The Tilt the camera should try and adjust to the closer it gets to the minimum height"));
            GUILayout.EndHorizontal();

            switch (RTSCameraInstance.autoAdjustState)
            {

                case RTSCamera.AutoAdjustState.DistanceToGround:

                    GUILayout.BeginHorizontal();
                    distanceFromGroundFinishAdjust.floatValue = EditorGUILayout.FloatField(new GUIContent("Finish Adjusting At Distance", "The distance from ground level that the camera should finish adjusting at and adjust to the value specified in the \"Adjust To Tilt\" field"), RTSCameraInstance.distanceFromGroundFinishAdjust);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    distanceFromGroundStartAdjust.floatValue = Mathf.Max(RTSCameraInstance.distanceFromGroundFinishAdjust, EditorGUILayout.FloatField(new GUIContent("Start Adjusting At Distance", "The distance from ground level that the camera should begin adjusting towards the value specified in the \"Adjust To Tilt\" field."), RTSCameraInstance.distanceFromGroundStartAdjust));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    groundHeightTiltAdjustCurve.animationCurveValue = EditorGUILayout.CurveField(new GUIContent("Adjust Curve", "The tilt curve for use with ground height"), RTSCameraInstance.groundHeightTiltAdjustCurve);
                    GUILayout.EndHorizontal();
                    break;
                case RTSCamera.
                AutoAdjustState.Height:
                    GUILayout.BeginHorizontal();
                    adjustHeightMin.floatValue = EditorGUILayout.FloatField(new GUIContent("Finish Adjusting At", "The height that the camera should finish adjusting at and adjust to the value specified in the \"Adjust To Tilt\" field"), RTSCameraInstance.adjustHeightMin);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    adjustHeightMax.floatValue = EditorGUILayout.FloatField(new GUIContent("Start Adjusting At", "The height that the camera should begin adjusting towards the value specified in the \"Adjust To Tilt\" field."), RTSCameraInstance.adjustHeightMax);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    //  heightTiltAdjustCurve.animationCurveValue = EditorGUILayout.CurveField(new GUIContent("Adjust Curve", "The tilt curve for use with height"), RTSCameraInstance.heightTiltAdjustCurve);
                    EditorGUILayout.PropertyField(heightTiltAdjustCurve, new GUIContent("Adjust Curve", "The tilt curve for use with height"));
                    GUILayout.EndHorizontal();
                    break;
            }


            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

        #region Smooth Adjust
        EditorGUILayout.BeginVertical("box");

        smoothBehavFoldout = GUILayout.Toggle(smoothBehavFoldout, "Smooth Adjust", EditorStyles.foldout);
        if (smoothBehavFoldout)
        {
            EditorGUI.indentLevel++;

            #region Movement

            GUILayout.BeginHorizontal();
            smoothPosition.boolValue = EditorGUILayout.Toggle(new GUIContent("Smooth Movement", "Should movement be smoothed"), RTSCameraInstance.smoothPosition);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            movementAdjustSpeed.floatValue = EditorGUILayout.FloatField("Movement Adjust Time", RTSCameraInstance.movementAdjustSpeed);
            GUILayout.EndHorizontal();


            #endregion

            EditorGUILayout.Separator();

            #region Rotation

            GUILayout.BeginHorizontal();
            smoothRotate.boolValue = EditorGUILayout.Toggle("Smooth Rotation", RTSCameraInstance.smoothRotate);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rotateAdjustSpeed.floatValue = EditorGUILayout.FloatField("Rotate Adjust Time", RTSCameraInstance.rotateAdjustSpeed);
            GUILayout.EndHorizontal();

            #endregion

            EditorGUILayout.Separator();

            #region Tilt

            GUILayout.BeginHorizontal();
            smoothTilt.boolValue = EditorGUILayout.Toggle("Smooth Tilt", RTSCameraInstance.smoothTilt);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            tiltAdjustSpeed.floatValue = EditorGUILayout.FloatField("Tilt Adjust Time", RTSCameraInstance.tiltAdjustSpeed);
            GUILayout.EndHorizontal();

            #endregion

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

        #region Height Adjust
        EditorGUILayout.BeginVertical("box");

        heightAdjustBehavFoldout = GUILayout.Toggle(heightAdjustBehavFoldout, "Height Adjust", EditorStyles.foldout);
        if (heightAdjustBehavFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //  RTSCameraInstance.heightAdjustState = (RTSCamera.HeightAdjustState)EditorGUILayout.EnumPopup("Height Adjust State", RTSCameraInstance.heightAdjustState);
            EditorGUILayout.PropertyField(heightAdjustState, new GUIContent("Height Adjust State", "Each option here will adjust height from ground distance based off miniumum height although the two options act differently. Change Height will change the height and keep it there unless adjusted again, preserve height will attempt to move back down to the original camera's position when it can do so"));
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

        #region Follow Transform
        EditorGUILayout.BeginVertical("box");

        followBehavFoldout = GUILayout.Toggle(followBehavFoldout, new GUIContent("Follow Transform", "Using this Behaviour you can specify a target to follow or to look at. While following a target you can specify a position offset from the target. eg offset (0, 50, 0) will attempt to place the camera at 50 units above the target (Max and Min height rules still apply). While looking at a target min tilt and max tilt rules still apply."), EditorStyles.foldout);
        if (followBehavFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            followTarget.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(new GUIContent("Target To Follow", "The target the camera should follow"), RTSCameraInstance.followTarget, typeof(Transform));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            followOffset.vector3Value = EditorGUILayout.Vector3Field(new GUIContent("Position Offset", "The positional offset from the target"), RTSCameraInstance.followOffset);
            GUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Set Current Offset", "Sets the offset to the camera's current position")))
            {

                followOffset.vector3Value = (RTSCameraInstance.transform.position - RTSCameraInstance.followTarget.position);
            }

            GUILayout.BeginHorizontal();
            shouldFollow.boolValue = EditorGUILayout.Toggle(new GUIContent("Should Follow Target", "If ticked the Camera will follow the target, assuming there is one"), RTSCameraInstance.shouldFollow);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            shouldLookAt.boolValue = EditorGUILayout.Toggle(new GUIContent("LookAt Target", "If ticked the camera will attempt to look at the target. Note min and max tilt rules still apply"), RTSCameraInstance.shouldLookAt);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            movementAdjustsOffset.boolValue = EditorGUILayout.Toggle(new GUIContent("Movement Adjusts Offset", "If ticked the camera will be able to move like normal except this movement will affect the offset from the current target"), RTSCameraInstance.movementAdjustsOffset);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

        #region Mouse Behaviour
        EditorGUILayout.BeginVertical("box");

        edgeScrollingBehavFoldout = GUILayout.Toggle(edgeScrollingBehavFoldout, "Edge Scrolling", EditorStyles.foldout);
        if (edgeScrollingBehavFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            xEdgeSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Scroll Speed X", "The scroll speed for when the mouse move into or past the far Left or the far Right sides of the screen. Set to 0 to disable functionality"), RTSCameraInstance.xEdgeSpeed);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            yEdgeSpeed.floatValue = EditorGUILayout.FloatField(new GUIContent("Scroll Speed Y", "The scroll speed for when the mouse move into or past the Top or the Bottom of the screen. Set to 0 to disable functionality"), RTSCameraInstance.yEdgeSpeed);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

        #region Cloud Drawing
        EditorGUILayout.BeginVertical("box");

        cloudBehavFoldout = GUILayout.Toggle(cloudBehavFoldout, "Clouds", EditorStyles.foldout);
        if (cloudBehavFoldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.BeginHorizontal();
            //   RTSCameraInstance.cloudDrawState = (RTSCamera.CloudDrawState)EditorGUILayout.EnumPopup(new GUIContent("Cloud Draw State", "The behaviour that decides how the clouds should be drawn. Draw At Percent will draw at a percent of the max height ( 100% equals max height). AtHeight will start and end the clouds at specific heights"), RTSCameraInstance.cloudDrawState);
            EditorGUILayout.PropertyField(cloudDrawState, new GUIContent("Cloud Draw State", "The behaviour that decides how the clouds should be drawn. Draw At Percent will draw at a percent of the max height ( 100% equals max height). AtHeight will start and end the clouds at specific heights"));
            GUILayout.EndHorizontal();

            switch (RTSCameraInstance.cloudDrawState)
            {
                case RTSCamera.CloudDrawState.PercentOfMaxHeight:
                    GUILayout.BeginHorizontal();
                    cloudStartAtHeightPercent.floatValue = EditorGUILayout.FloatField(new GUIContent("Start At Percent", "Start drawing clouds at this percent of height"), RTSCameraInstance.cloudStartAtHeightPercent);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    cloudFinishAtHeightPercent.floatValue = Math.Min(EditorGUILayout.FloatField(new GUIContent("Finish At Percent", "Finish drawing clouds at this percent of height. Note the finished alpha will be the alpha value specified in the Cloud Max Transparency value"), RTSCameraInstance.cloudFinishAtHeightPercent), 100f);
                    GUILayout.EndHorizontal();

                    break;
                case RTSCamera.CloudDrawState.AtHeight:
                    GUILayout.BeginHorizontal();
                    cloudStartAtHeight.floatValue = EditorGUILayout.FloatField(new GUIContent("Start At Height", "Start drawing clouds at this height"), RTSCameraInstance.cloudStartAtHeight);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    cloudFinishAtHeight.floatValue = Mathf.Min(EditorGUILayout.FloatField(new GUIContent("Finish At Height", "Finish drawing clouds at this height. Note the finished alpha will be the alpha value specified in the Cloud Max Transparency value"), RTSCameraInstance.cloudFinishAtHeight), RTSCameraInstance.maxHeightDistance);
                    GUILayout.EndHorizontal();
                    break;
            }

            GUILayout.BeginHorizontal();
            cloudMaxAlpha.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Cloud Max Transparency", "The transparency of the cloud when it reaches the maximum height. Value should be between 0 and 1. Note the transparency will interperlate the transparency from 0 to this number starting at the minimum point and ending at the maximum point."), RTSCameraInstance.cloudMaxAlpha), 0, 1);
            GUILayout.EndHorizontal();


            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        #endregion

    }

    private void CameraConfigEditor()
    {

        #region Movement
        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;

        GUILayout.BeginHorizontal();
        //RTSCameraInstance.movementSpeed = EditorGUILayout.FloatField(new GUIContent("Movement Speed", "The movement speed of the camera. This controls speed for Vertical and Horizontal movements"), RTSCameraInstance.movementSpeed);
        EditorGUILayout.PropertyField(movementSpeed, new GUIContent("Movement Speed", "The movement speed of the camera. This controls speed for Vertical and Horizontal movements"));
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        #endregion

        #region Rotate

        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;

        GUILayout.BeginHorizontal();
        //   RTSCameraInstance.rotateSpeed = EditorGUILayout.FloatField(new GUIContent("Rotate Speed", "Camera Rotation speed around the Y axis i.e. rotating left and right"), RTSCameraInstance.rotateSpeed);
        EditorGUILayout.PropertyField(rotateSpeed, new GUIContent("Rotate Speed", "Camera Rotation speed around the Y axis i.e. rotating left and right"));
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        #endregion

        #region Tilt
        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;

        GUILayout.BeginHorizontal();
        //    RTSCameraInstance.tiltSpeed = EditorGUILayout.FloatField(new GUIContent("Tilt Speed", "The tilt speed i.e. tilting the camera up and down"), RTSCameraInstance.tiltSpeed);
        EditorGUILayout.PropertyField(tiltSpeed, new GUIContent("Tilt Speed", "The tilt speed i.e. tilting the camera up and down"));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //  RTSCameraInstance.lowTilt = EditorGUILayout.FloatField(new GUIContent("Min Tilt", "The minimum tilt of the camera. A tilt of -90 will be facing upwards while a tilt of 90 will be facing downwards"), RTSCameraInstance.lowTilt);
        EditorGUILayout.PropertyField(minTilt, new GUIContent("Min Tilt", "The minimum tilt of the camera. A tilt of -90 will be facing upwards while a tilt of 90 will be facing downwards"));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //  RTSCameraInstance.highTilt = EditorGUILayout.FloatField(new GUIContent("Max Tilt", "The maxmimum tilt of the camera. A tilt -90 will be facing upwards while a tilt of 90 will be facing downwards"), RTSCameraInstance.highTilt);
        EditorGUILayout.PropertyField(maxTilt, new GUIContent("Max Tilt", "The maxmimum tilt of the camera. A tilt -90 will be facing upwards while a tilt of 90 will be facing downwards"));
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        #endregion

        #region Height

        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;

        GUILayout.BeginHorizontal();
        //  RTSCameraInstance.minHeightDistance = EditorGUILayout.FloatField(new GUIContent("Min Height", "The minimum height above ground level the camera can be"), RTSCameraInstance.minHeightDistance);
        EditorGUILayout.PropertyField(minHeight, new GUIContent("Min Height", "The minimum height above ground level the camera can be"));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //  RTSCameraInstance.maxHeight = EditorGUILayout.FloatField(new GUIContent("Max Height", "The maximum height the camera can go. Note this is not affected by ground level"), RTSCameraInstance.maxHeight);
        EditorGUILayout.PropertyField(maxHeight, new GUIContent("Max Height", "The maximum height the camera can go. Note this is not affected by ground level"));
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        #endregion

        #region Misc Details

        EditorGUI.indentLevel++;


        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        //   RTSCameraInstance.useDeltaTimeToOne = EditorGUILayout.Toggle(new GUIContent("Delta Time Ignores TimeScale", "If this is enabled the delta time will not be affected by the TimeScale"), RTSCameraInstance.useDeltaTimeToOne);
        EditorGUILayout.PropertyField(delaTimeIgnoresTimeScale, new GUIContent("Delta Time Ignores TimeScale", "If this is enabled the delta time will not be affected by the TimeScale"));
        EditorGUILayout.EndHorizontal();


        if (RTSCameraInstance.groundMask == 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Ensure that this is properly set to your ground layer to avoid complications", MessageType.Warning);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        groundLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Ground Layer", "The ground layer(s) the camera should interact with"), RTSCameraInstance.groundMask);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel--;


        #endregion

    }

    #region Helper Methods

    private Vector3 ClampVector(Vector3 vector)
    {
        return new Vector3(Mathf.Clamp(vector.x, -1, 1), Mathf.Clamp(vector.y, -1, 1), Mathf.Clamp(vector.z, -1, 1));
    }

    #endregion

}

