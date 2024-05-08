using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FlowBar : MonoBehaviour
{
    float myFlow = 0.0f;

    [SerializeField] Image myFlowbar;
    [SerializeField] Image myFlowStage1;
    [SerializeField] Image myFlowStage2;
    [SerializeField] Image myFlowStage3;

    void Update()
    {
        myFlow += Time.deltaTime * 0.05f;
        myFlow = Mathf.Clamp01(myFlow);

        myFlowStage1.enabled = false;
        myFlowStage2.enabled = false;
        myFlowStage3.enabled = false;

        myFlowbar.fillAmount = myFlow;

        if (myFlow > 0.25f) myFlowStage1.enabled = true;
        if (myFlow > 0.50f) myFlowStage2.enabled = true;
        if (myFlow > 0.75f) myFlowStage3.enabled = true;
    }
}