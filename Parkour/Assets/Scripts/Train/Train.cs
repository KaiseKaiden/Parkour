using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField] float myTrainCoolDown;
    [SerializeField] int myWaggonAmount;

    [SerializeField] float mySpeed;

    [Space(16)]
    [Header("DONT TOUCH!")]

    [SerializeField] Transform myStart;
    [SerializeField] Transform myEnd;

    [SerializeField] Transform myTrainTransform;
    [SerializeField] GameObject myWaggonObject;
    const float myWaggonLength = 12.0f;
    float myRealLength = 0.0f;

    const float myRummbleDistance = 10.0f * 10.0f;

    float myTime;

    Transform myPlayerTransform;
    PlayerStateMachine myPlayerStateMachine;

    [SerializeField] bool myIsTutorial;

    private void Awake()
    {
        // Create Waggon
        for (int i = 0; i < myWaggonAmount - 1; i++)
        {
            GameObject waggon = Instantiate(myWaggonObject, transform.position, Quaternion.identity);
            waggon.transform.parent = myTrainTransform;
            waggon.transform.localPosition = (Vector3.back * myWaggonLength * 0.5f) + (Vector3.back * (myWaggonLength * (i + 1.0f) + (i + 1.0f) * 0.5f)) + Vector3.up * 3.5f;
            waggon.transform.localRotation = Quaternion.identity;

            waggon.GetComponent<Waggon>().SetTrainObject(this);
        }
    }

    private void Start()
    {
        myRealLength = myWaggonLength * myWaggonAmount + 0.5f * myWaggonAmount;
        myTime = myTrainCoolDown;

        // Set Train Looking In The Right Direction
        Vector3 direction = myEnd.localPosition - myStart.localPosition;
        myTrainTransform.localRotation = Quaternion.LookRotation(direction.normalized);
        myTrainTransform.localPosition = myStart.localPosition;

        // Get Player Transform
        myPlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        myPlayerStateMachine = myPlayerTransform.GetComponent<PlayerStateMachine>();
    }

    private void Update()
    {
        if (!myIsTutorial)
        {
            myTime -= Time.deltaTime;
        }

        if (myTime < 0.0f)
        {
            TrainRunning();
        }
    }

    void TrainRunning()
    {
        myTrainTransform.position += myTrainTransform.forward * mySpeed * Time.deltaTime;

        CreateRummbleEffect();

        if ((myTrainTransform.position - myStart.position).magnitude > (myEnd.position - myStart.position).magnitude + myRealLength)
        {
            myTime = myTrainCoolDown;
            myTrainTransform.position = myStart.position;
        }
    }

    void CreateRummbleEffect()
    {
        if ((myTrainTransform.position - myPlayerTransform.position).sqrMagnitude < myRummbleDistance)
        {
            myPlayerStateMachine.SetScreenShakeIntensity(0.35f);
        }
    }

    public void HitPlayer()
    {
        myPlayerStateMachine.Respawn();
    }

    public float GetSpeed()
    {
        return mySpeed;
    }

    public void EndTutorial()
    {
        if (myIsTutorial)
        {
            myIsTutorial = false;
            myTime = 0.0f;
        }
    }
}
