using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    private eMessage myMessage;
    Vector3 myVec3;

    public Message(eMessage aMsg, Vector3 aVec)
    {
        myMessage = aMsg;
        myVec3 = aVec;
    }

    public eMessage GetMsg()
    {
        return myMessage;
    }

    public Vector3 GetVector3()
    {
        return myVec3;
    }
}
