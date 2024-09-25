using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WheelSteeringScript : MonoBehaviour
{
    public InputListener ipl;
    public RectTransform rectTransform;
    public float angleMultiplier;
    float lastSteerTotal = 0;
    float steerTotal = 0;//the accumulative value
    // Start is called before the first frame update
    void Start()
    {
        ipl = GameObject.FindGameObjectWithTag("InputListen").GetComponent<InputListener>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
        if(Math.Abs(steerTotal) < 1){
            float delta = steerTotal - lastSteerTotal;
            float steerAngle = -delta * angleMultiplier;
            rectTransform.Rotate(0f, 0f, steerAngle);
        }else if(steerTotal >= 1){
            steerTotal = 1;
            float delta = steerTotal - lastSteerTotal;
            float steerAngle = -delta * angleMultiplier;
            rectTransform.Rotate(0f, 0f, steerAngle);
        }else{
            steerTotal = -1;
            float delta = steerTotal - lastSteerTotal;
            float steerAngle = -delta * angleMultiplier;
            rectTransform.Rotate(0f, 0f, steerAngle);
        }

        lastSteerTotal = steerTotal;
        
    }
    public void ChangeSteerInput(float delta){
        steerTotal += delta;
    }
    public void SetSteerInput(float sInput){
        steerTotal = sInput;
    }
}
