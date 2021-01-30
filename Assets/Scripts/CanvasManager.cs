using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class CanvasManager : MonoBehaviour
{

    public GameObject canvas;
    public Hand leftHand;
    public Hand rightHand;
    public float scaleFactor = 1.0f;
    public float moveFactor = 1.0f;
    public SteamVR_Action_Vibration hapticAction;
    public Transform cameraTransform;

    Vector3 initialLeftHandPosition;
    Vector3 initialRightHandPosition;

    Vector3 lastLeftHandPosition;
    Vector3 lastRightHandPosition;

    Vector3 lastUpperTrackPointPosition;
    Vector3 lastLowerTrackPointPosition;

    Quaternion lastLeftHandRotation;

    Transform leftHandUpperTrackPoint;
    Transform leftHandLowerTrackPoint;
    

    bool initMoving = true;
    bool inGrabing;
    bool inPressingB = false;

    bool leftGrabHaptic = true;
    bool rightGrabHaptic = true;
    // Start is called before the first frame update
    void Start()
    {
        leftHandUpperTrackPoint = leftHand.transform.Find("UpperPoint");
        leftHandLowerTrackPoint = leftHand.transform.Find("LowerPoint");
    }

    // Update is called once per frame
    void Update()
    {
        float strengthThreshold = 0.4f;
        if (inGrabing)
        {
            strengthThreshold = 0.25f;
        }


        if (SteamVR_Actions.default_PressB.GetState(SteamVR_Input_Sources.LeftHand))
        { 
            inPressingB = true;
        }
        else
        {
            inPressingB = false;
        }
        

        if (!inGrabing)
        {
            if (leftGrabHaptic & SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.LeftHand) > strengthThreshold)
            {
                hapticAction.Execute(0, 0.075f, 100, 120, SteamVR_Input_Sources.LeftHand);
                leftGrabHaptic = false;
            }
            if (rightGrabHaptic & SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.RightHand) > strengthThreshold)
            {
                hapticAction.Execute(0, 0.075f, 100, 120, SteamVR_Input_Sources.RightHand);
                rightGrabHaptic = false;
            }

            if ((SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.LeftHand) < strengthThreshold &&
                SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.RightHand) < strengthThreshold))
            {
                rightGrabHaptic = true;
                leftGrabHaptic = true;
            }
        }
        if (SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.LeftHand) > strengthThreshold &&
            SteamVR_Actions.default_Squeeze.GetAxis(SteamVR_Input_Sources.RightHand) > strengthThreshold)
        {
            
            inGrabing = true;
            rightGrabHaptic = true;
            leftGrabHaptic = true;
        }
        else
        {

            inGrabing = false;
            initMoving = true;
            initialLeftHandPosition = leftHand.transform.position;
            initialRightHandPosition = rightHand.transform.position;
        }
        lastLeftHandPosition = leftHand.transform.position;
        lastRightHandPosition = rightHand.transform.position;
        //lastLeftHandRotation = leftHand.transform.rotation;
        lastUpperTrackPointPosition = leftHandUpperTrackPoint.position;
        lastLowerTrackPointPosition = leftHandLowerTrackPoint.position;
    }


    void ChangeScale()
    {
        Vector3 currentLeftHandPosition = leftHand.transform.position;
        Vector3 currentRightHandPosition = rightHand.transform.position;
        float lastDelta = (lastLeftHandPosition - lastRightHandPosition).magnitude;
        float currentDelta = (currentLeftHandPosition - currentRightHandPosition).magnitude;

        float delta = currentDelta - lastDelta;

        if (initMoving && Mathf.Abs(delta) < 0.005f)
        {
            delta = 0;
            initMoving = false;
        }

        canvas.transform.localScale = new Vector3(Mathf.Min(10, Mathf.Max(-10, canvas.transform.localScale.x + delta * scaleFactor)),
                                                   canvas.transform.localScale.y + delta * scaleFactor,
                                                   Mathf.Min(10, Mathf.Max(-10, canvas.transform.localScale.z )));
    }

    void ChangeRotation()
    {

        Vector3 currentLeftHandPosition = leftHand.transform.position;
        Vector3 currentRightHandPosition = rightHand.transform.position;
        Vector3 lastVector = Vector3.ProjectOnPlane(lastLeftHandPosition - lastRightHandPosition,Vector3.up);
        Vector3 currentVector = Vector3.ProjectOnPlane(currentLeftHandPosition - currentRightHandPosition, Vector3.up);


        float y_angle = Vector3.Angle(lastVector, currentVector); 
        Vector3 normal = Vector3.Cross(lastVector, currentVector);
        y_angle *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));

        Vector3 currentEuler = canvas.transform.rotation.eulerAngles;

        Quaternion currentLeftHandRotation = leftHand.transform.rotation;
        //float x_angle = (currentLeftHandRotation * Quaternion.Inverse(lastLeftHandRotation)).eulerAngles.x;

        //Debug.Log(x_angle);

        //Quaternion x_quaternion = Quaternion.Euler(x_angle, 0, 0);

        canvas.transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y + y_angle, currentEuler.z);
        //canvas.transform.rotation = canvas.transform.rotation * x_quaternion;

        ChangeXRotation();
    }

    void ChangeXRotation()
    {
        Vector3 currentUpperPosition = leftHandUpperTrackPoint.position;
        Vector3 currentLowerdPosition = leftHandLowerTrackPoint.transform.position;
        Vector3 lastVector_xy = lastUpperTrackPointPosition - lastLowerTrackPointPosition;
        Vector3 currentVector_xy = currentUpperPosition - currentLowerdPosition;
        Vector3 normal_plane = Vector3.Cross(lastVector_xy, currentVector_xy);

        lastVector_xy = Vector3.ProjectOnPlane(lastVector_xy,normal_plane);
        currentVector_xy = Vector3.ProjectOnPlane(currentVector_xy, normal_plane);



        float x_angle_xy = Vector3.Angle(lastVector_xy, currentVector_xy);
        x_angle_xy *= Mathf.Sign(Vector3.Dot(normal_plane, cameraTransform.forward));


        Quaternion x_quaternion = Quaternion.Euler(x_angle_xy, 0, 0);
        canvas.transform.rotation = canvas.transform.rotation * x_quaternion;

    }

    void ChangePosition()
    {
        Vector3 currentLeftHandPosition = leftHand.transform.position;
        Vector3 currentRightHandPosition = rightHand.transform.position;
        Vector3 currentCenterPosition = (currentLeftHandPosition + currentRightHandPosition) / 2;
        Vector3 lastCenterPosition = (lastLeftHandPosition + lastRightHandPosition) / 2;

        Vector3 currentDelta = (currentCenterPosition - lastCenterPosition);

        canvas.transform.position += currentDelta * moveFactor;
    }

    void CanvasTransforming()
    {
        ChangeScale();
        ChangePosition();
        

    }

    private void FixedUpdate()
    {
        if (inGrabing)
        {
            CanvasTransforming();
            if (inPressingB)
            {
                ChangeRotation();
            }
        }

    }
}
