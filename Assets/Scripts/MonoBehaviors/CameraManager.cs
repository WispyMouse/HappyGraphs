using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera AttachedCamera;

    Rect usedPlayingArea;
    Rect buffer;

    public Vector3 HandLocation;
    Vector3 HandLocationOffset;

    #region Calculated Values
    // These values are generated in UpdateTargetViewingArea, and are used for smooth camera movement.
    Rect targetViewingArea;
    float targetOrthographicSize;
    Vector3 targetPosition;
    float orthographicTransitionSpeed;
    #endregion

    float positionTransitionSpeed = .6f; // Camera meters per second

    private void Awake()
    {
        AttachedCamera = GetComponent<Camera>();
        buffer = new Rect(-3f, -5f, 6f, 9f);
        HandLocationOffset = new Vector3(-.5f, 1.75f, 0);
        ResetCamera();
    }

    public void ResetCamera()
    {
        usedPlayingArea = new Rect(0, 0, 0, 0);
        UpdateTargetViewingArea();
        SnapCameraPosition();
    }

    public void NewPlacement(Vector3 location)
    {
        if (location.x > usedPlayingArea.xMax)
        {
            usedPlayingArea.width = Mathf.Abs(location.x - usedPlayingArea.x);
        }

        if (location.x < usedPlayingArea.xMin)
        {
            usedPlayingArea.width = usedPlayingArea.width + (usedPlayingArea.x - location.x);
            usedPlayingArea.x = location.x;
        }

        if (location.y > usedPlayingArea.yMax)
        {
            usedPlayingArea.height = Mathf.Abs(location.y - usedPlayingArea.y);
        }

        if (location.y < usedPlayingArea.yMin)
        {
            usedPlayingArea.height = usedPlayingArea.height + (usedPlayingArea.y - location.y);
            usedPlayingArea.y = location.y;
        }

        UpdateTargetViewingArea();
    }

    void UpdateTargetViewingArea()
    {
        targetViewingArea = new Rect(usedPlayingArea.x + buffer.x, usedPlayingArea.y + buffer.y, usedPlayingArea.width + buffer.width, usedPlayingArea.height + buffer.height);
        HandLocation = new Vector3(targetViewingArea.xMax + HandLocationOffset.x, targetViewingArea.yMin + HandLocationOffset.y, 0);

        targetPosition = new Vector3(targetViewingArea.center.x, targetViewingArea.center.y, -10);

        float verticalOrthographicSize = targetViewingArea.height / 2f;
        float horizontalOrthographicSize = (targetViewingArea.width + buffer.width) / AttachedCamera.aspect / 2f;

        targetOrthographicSize = Mathf.Max(verticalOrthographicSize, horizontalOrthographicSize);

        if (targetOrthographicSize != AttachedCamera.orthographicSize)
        {
            float positionDistance = Vector3.Distance(AttachedCamera.transform.position, targetPosition);
            float orthographicDistance = Mathf.Abs(AttachedCamera.orthographicSize - targetOrthographicSize);
            orthographicTransitionSpeed = orthographicDistance / (positionDistance / positionTransitionSpeed);
        }
    }

    void SnapCameraPosition()
    {
        AttachedCamera.transform.position = targetPosition;
        AttachedCamera.orthographicSize = targetOrthographicSize;

        orthographicTransitionSpeed = 0;
    }

    private void Update()
    {
        AttachedCamera.transform.position = Vector3.MoveTowards(AttachedCamera.transform.position, targetPosition, Time.deltaTime * positionTransitionSpeed);
        AttachedCamera.orthographicSize = Mathf.MoveTowards(AttachedCamera.orthographicSize, targetOrthographicSize, Time.deltaTime * orthographicTransitionSpeed);
    }

    public Vector3 GetHandPosition(int index, int maxIndex)
    {
        float leftMost = .4f * (float)(maxIndex - 1);
        float offset = (float)index * .8f;

        return HandLocation + Vector3.right * (offset - leftMost) + 
            Vector3.forward * (float)index * .02f; // Bring the card forward towards the camera a little bit, to reduce overlapping
    }
}
