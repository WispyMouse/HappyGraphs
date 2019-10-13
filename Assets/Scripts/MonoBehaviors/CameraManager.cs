using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera AttachedCamera;

    Rect usedPlayingArea;
    float buffer;

    public Vector3 HandLocation;
    Vector3 HandLocationOffset;

    #region Calculated Values
    // These values are generated in UpdateTargetViewingArea, and are used for smooth camera movement.
    Rect targetViewingArea;
    float targetOrthographicSize;
    Vector3 targetPosition;

    float orthographicTransitionSpeed;
    float positionTransitionSpeed = .5f;
    #endregion

    private void Awake()
    {
        AttachedCamera = GetComponent<Camera>();
        usedPlayingArea = new Rect(0, 0, 0, 0);
        buffer = 3f;
        HandLocationOffset = new Vector3(-.5f, 1.25f, 0);
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
        targetViewingArea = new Rect(usedPlayingArea.x - buffer, usedPlayingArea.y - buffer, usedPlayingArea.width + buffer * 2f, usedPlayingArea.height + buffer * 2f);
        HandLocation = new Vector3(targetViewingArea.xMax + HandLocationOffset.x, targetViewingArea.yMin + HandLocationOffset.y, 0);

        targetPosition = new Vector3(targetViewingArea.center.x, targetViewingArea.center.y, -10);

        float verticalOrthographicSize = targetViewingArea.height / 2f;
        float horizontalOrthographicSize = AttachedCamera.aspect * targetViewingArea.width / 2f;

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
}
