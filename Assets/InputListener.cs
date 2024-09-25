using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputListener : MonoBehaviour
{
    float xSens = 0.2f;
    float ySens = 0.8f;
    // Start is called before the first frame update

    /*
    To use the listener in other classes:
    public InputListener ipl;
    
    void Start()
    {
        
        ipl = GameObject.FindGameObjectWithTag("InputListen").GetComponent<InputListener>();
        
    }
    */
    
    //This method only reports the current motion of the mouse, it is not responsible to memorize past locations.
    public Vector2 getMouseMotion(){
        Vector2 result;
        result.x = Input.GetAxis("Mouse X") * xSens;
        result.y = Input.GetAxis("Mouse Y") * ySens;
        return result;
    }

    public float GetMouseMotionX(){
        float f = Input.GetAxis("Mouse X") * xSens;
        
        return f;
    }

    public float GetMouseMotionY(){
        return Input.GetAxis("Mouse Y") * ySens;
    }

    public void setSensitive(float x, float y){
        xSens = x;
        ySens = y;
    }

    
}
