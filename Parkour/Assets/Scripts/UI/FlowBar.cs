using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FlowBar : MonoBehaviour
{
    float myFlow = 0.0f;
    float myVisualFlow = 0.0f;
    [SerializeField] float myFlowIncreaseMultiplier = 5.0f;

    [SerializeField] Image myFlowbar;
    [SerializeField] Image myFlowStage1;
    [SerializeField] Image myFlowStage2;
    [SerializeField] Image myFlowStage3;

    void Update()
    {
        myFlowStage1.enabled = false;
        myFlowStage2.enabled = false;
        myFlowStage3.enabled = false;

        myVisualFlow = Mathf.Lerp(myVisualFlow, myFlow, myFlowIncreaseMultiplier * Time.deltaTime);
        myFlowbar.fillAmount = myVisualFlow;

        if (myFlow > 0.25f) myFlowStage1.enabled = true;
        if (myFlow > 0.50f) myFlowStage2.enabled = true;
        if (myFlow > 0.75f) myFlowStage3.enabled = true;
    }

    public void SetFlowPercentage(float aValue01)
    {
        myFlow = Mathf.Clamp01(aValue01);
    }

    public void SetFlowPercentageInstant(float aValue01)
    {
        myFlow = aValue01;
        myVisualFlow = aValue01;
        myFlowbar.fillAmount = aValue01;
    }
}