using System.Collections;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 moveDirection;
    private float moveDistance;
    private float scaleAmountX, scaleAmountY, scaleAmountZ;
    private Vector3 rotationAxis;
    private float rotationSpeed;

    private enum ActionType { Move, Scale, Rotate }
    private ActionType chosenAction;

    void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;

        // Randomly choose one action: Move, Scale, or Rotate
        chosenAction = (ActionType)Random.Range(0, 3);

        // Setup random parameters based on the chosen action
        switch (chosenAction)
        {
            case ActionType.Move:
                SetupMove();
                break;

            case ActionType.Scale:
                SetupScale();
                break;

            case ActionType.Rotate:
                SetupRotation();
                break;
        }
    }

    void Update()
    {
        // Perform the chosen action in every frame
        switch (chosenAction)
        {
            case ActionType.Move:
                PerformMove();
                break;

            case ActionType.Scale:
                PerformScale();
                break;

            case ActionType.Rotate:
                PerformRotation();
                break;
        }
    }

    void SetupMove()
    {
        // Randomize movement between 5 and 25 meters
        moveDistance = Random.Range(5f, 25f);

        // Pick a random direction for movement on one of the three axes
        int axis = Random.Range(0, 3);
        moveDirection = Vector3.zero;
        moveDirection[axis] = moveDistance;
    }

    void PerformMove()
    {
        transform.position = initialPosition + moveDirection * Mathf.PingPong(Time.time, 1);
    }

    void SetupScale()
    {
        // Randomize scaling between 0.5 and 15 for each axis
        scaleAmountX = Random.Range(0.5f, 15f);
        scaleAmountY = Random.Range(0.5f, 15f);
        scaleAmountZ = Random.Range(0.5f, 15f);

        // Randomly decide which axes to scale
        bool scaleX = Random.Range(0, 2) == 1;
        bool scaleY = Random.Range(0, 2) == 1;
        bool scaleZ = Random.Range(0, 2) == 1;

        // Apply scaling only to the chosen axes
        if (!scaleX) scaleAmountX = initialScale.x;
        if (!scaleY) scaleAmountY = initialScale.y;
        if (!scaleZ) scaleAmountZ = initialScale.z;
    }

    void PerformScale()
    {
        // Ping-pong scaling effect over time
        float t = Mathf.PingPong(Time.time, 1);
        transform.localScale = Vector3.Lerp(initialScale, new Vector3(scaleAmountX, scaleAmountY, scaleAmountZ), t);
    }

    void SetupRotation()
    {
        // Randomize a rotation axis
        rotationAxis = new Vector3(Random.value, Random.value, Random.value).normalized;

        // Randomize rotation speed between 10 and 100 degrees per second
        rotationSpeed = Random.Range(10f, 100f);
    }

    void PerformRotation()
    {
        // Rotate around the chosen axis
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
