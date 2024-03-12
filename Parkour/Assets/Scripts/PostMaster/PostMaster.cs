using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eMessage
{
    EdgeClimb,
    Vault,
    Count
}

public class PostMaster : MonoBehaviour
{
    public static PostMaster Instance { get; private set; }
    List<List<Observer>> myObservers = new List<List<Observer>>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < (int)eMessage.Count; i++)
        {
            myObservers.Add(new List<Observer>());
        }
    }

    public void Subscribe(eMessage aMsg, Observer aObserver)
    {
        myObservers[(int)aMsg].Add(aObserver);
    }

    public void SendMessage(Message aMsg)
    {
        for (int i = 0; i < myObservers[(int)aMsg.GetMsg()].Count; i++)
        {
            myObservers[(int)aMsg.GetMsg()][i].Recive(aMsg);
        }
    }
}
