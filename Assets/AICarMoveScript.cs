using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
This is where the main logic of both the car and the ai driver is performed. 
The physics of the car as well as the parameters of the car is also set and done here.
*/
public class AICarMoveScript : MonoBehaviour
{
    // Start is called before the first frame update

    //Note: if this is the car driven by the user, add it to UserControlledCar layer in unity

    //Parts of a car:
    //###################################################
    public InputListener ipl;
    public WheelSteeringScript wss;
    public Rigidbody2D carRigidbody;
    //###################################################
    
    //physical attributes of the car
    //#########################################################################################
    float carAccelScale = 35f;
    float carTurnRate = 0.08f;
    float tyreGrip = 35f; // to represent how strong the tyre is
    float steerInfluence = 0.25f; // factor between steering and tyreGrip
    float steerToDir = 150; // how much steering would point the front wheel at how many degrees;
    float throttleInfluence = 0.3f; // factor between throttle and Grip of the rear tyre
    float dragFactor = 0.65f; //use quadratic drag to prevent infinite speed;
    float driftHelpRate = 0.36f; //how much will the system correct the wheels to help drifting
    float dragExponent = 1.41f; 
    float angVelocityFactor = 10f;
    float angVelocityLeniency = 3f;//how lenient is getting out of a drift
    float slippingThrustRate = 0.74f; //what rate of thrust will be delivered if slipping
    float slippingThrustSpinRate = 0.22f; //how much will thrust cause the car to spin
    float slippingSteerRate = 0.29f; //how much steering will have an effect on direction
    float spinoutAngDrag = 0.2f; //in spinout, how much angular drag are there
    float driftSteerFactor = 2.42f; //how much steering control are there in drifting;
    float driftThrustSpinRate = 0.1f; //how much thrust will induce steering in drifting state
    bool isHumanOperated = false;
    float driftCorrectionRate = 0.03f; //how much will the drift self correct back into going straight
    //###############################################################################################




    //Local variables to keep track the car's stats
    //##################################################################################
    float accelInput = 0;
    float steerInput = 0;
    public float GetSteerInput(){
        return steerInput;
    }
    float rotation = 0;//the accumulative value
    bool frontTraction = true;
    public bool GetFrontTraction(){
        return frontTraction;
    }
    bool rearTraction = true;
    public bool GetRearTraction(){
        return rearTraction;
    }
    float driftAngle = 0;
    int spinoutDirection = 0;
    //######################################################################################


    


    void Start()
    {
        ipl = GameObject.FindGameObjectWithTag("InputListen").GetComponent<InputListener>();
        wss = GameObject.FindGameObjectWithTag("SteeringWheel").GetComponent<WheelSteeringScript>();
        parseInput();
        
        
    }
    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {//fixed update interval to fix frame rate issue

        /*
        1. get the input and parse it using previous traction info
        3. parse traction with the given input
        4. apply throttle and steering
        5. calculate and set/apply drag
        */
        parseInput();
        setTractionState();
        ApplyForces();
        
        
        

        
        
        

    }


    //functions
    
    //step 1
    void parseInput(){//this is to parse both steering and throttle input
        parseSteerInput();
        //dealing with throttle input below

        if (Input.GetKey(KeyCode.Space))//space and w/s means full throttle or break
        {//working by inspection
            if(Input.GetKey(KeyCode.W)){
                accelInput = 1;
            }else if(Input.GetKey(KeyCode.S)){
                accelInput = -1;
            }else{
                accelInput = 0;
            }
        }else if(!frontTraction || !rearTraction)//if the car is drifting.
        {//without spacebar, w and s would mean half throttle/break
            if(Input.GetKey(KeyCode.W)){
                accelInput = 0.5f;
            }else if(Input.GetKey(KeyCode.S)){
                accelInput = -0.5f;
            }else{
                accelInput = 0;
            }
        }else//given that the car is not drifting at the moment
        {//adjust throttle or break so that 
            //this chain of multiplication is basically current turn rate x speed x factor
            //working by inspection
            float remainGrip = tyreGrip - steerInfluence * (float)Math.Abs(steerInput) * carTurnRate * (float)(Math.Pow(carRigidbody.velocity.magnitude, 2));//calculate how much grip is left from steering
            if(remainGrip > 0){
                if(Input.GetKey(KeyCode.W)){//give allowed throttle less than 1
                accelInput = (float)Math.Min(remainGrip/throttleInfluence/carAccelScale, 1);
                }else if(Input.GetKey(KeyCode.S)){
                    accelInput = (float)Math.Max(-1 * remainGrip/throttleInfluence/carAccelScale, -1);
                }else{
                    accelInput = 0;
                }
            }else{
                accelInput = 0;//set accelInput if remaining grip is 0
            }
        }
        if(Vector2.Dot(carRigidbody.velocity, transform.right) <= 0 && accelInput < 0){//this eliminates the backward driving, which simplifies the control.
            accelInput = 0;
        }

    }


    void parseSteerInput()//used in parseInputr()
    {//get input from inputListener and set up the accel and steer input
/*
The steering input is completely not affected by whether the car is drifting
The drift status will affect how the steering is applied to the physical car
That is handled in ApplySteering()
*/
        float deltaSteer = ipl.GetMouseMotionX();

        steerInput += deltaSteer;//since the input is in the form of velocity, add them to get the mouse's location
        
        if (steerInput > 1)
        {
            steerInput = 1;
        }
        if (steerInput < -1)
        {
            steerInput = -1;
        }
        if(isHumanOperated){
            wss.SetSteerInput(steerInput);
        }
        
    }


    //step 2

    void setTractionState(){
        if(frontTraction && rearTraction){//when they are both in traction
            float currTyreLoad = steerInfluence * (float)Math.Abs(steerInput) * carTurnRate * (float)Math.Pow(carRigidbody.velocity.magnitude, 2) + (float)Math.Abs(accelInput*carAccelScale*throttleInfluence);
            if(currTyreLoad > tyreGrip){//if there are more load that the car can handle then slip out
                frontTraction = false;
                rearTraction = false;
                if(steerInput < 0){
                    spinoutDirection = -1;
                }else{
                    spinoutDirection = 1;
                }
            }

        }else{//if in a drifting state
            if(carRigidbody.velocity.magnitude < 6) {//if we stop, we get traction
                frontTraction = true;
                rearTraction = true;
                spinoutDirection = 0;
            }
            float newDriftAngle = Vector2.SignedAngle(carRigidbody.velocity.normalized, transform.right);//somewhat the angle of attack of cars
            if(Math.Abs(steerInput * steerToDir - newDriftAngle) < 20){//front recovery of traction for stable drifts
                frontTraction = true;
                driftSustain();
            }else{
                frontTraction = false;
                driftHelp();//mimic the tendency of front wheel straightening out
            }
            

            float angularV = (driftAngle - newDriftAngle) * angVelocityFactor;//how fast is the car spinning
            if(Math.Abs(angularV)/carRigidbody.velocity.magnitude*100 < angVelocityLeniency){
                Debug.Log(Math.Abs(newDriftAngle));
            }
            
            if(Math.Abs(angularV)/carRigidbody.velocity.magnitude*100 < angVelocityLeniency && Math.Abs(newDriftAngle) < 30 ){//if the car isnt spinning that much and the back wheel lines up with the direction of motion
                
                frontTraction = true;
                rearTraction = true;//regain traction
                spinoutDirection = 0;
            }

            driftAngle = newDriftAngle;
        }
    }
    void driftSustain(){
        float driftAngle = Vector2.SignedAngle(carRigidbody.velocity.normalized, transform.right);
        float difference = steerInput * steerToDir - driftAngle;
        float steerHelp = -difference/steerToDir*driftHelpRate;
        steerInput  += steerHelp*0.5f;
        if(isHumanOperated){
            wss.SetSteerInput(steerInput);
        }
    }

    void driftHelp(){
        //This is to slight nudge the input toward the angle that will let the front wheel have traction so to make drifting easier. Will not affect bots
        float driftAngle = Vector2.SignedAngle(carRigidbody.velocity.normalized, transform.right);
        float difference = steerInput * steerToDir - driftAngle;
        float steerHelp = difference/steerToDir*driftHelpRate;
        steerInput  -= steerHelp;
        if(isHumanOperated){
            wss.SetSteerInput(steerInput);
        }

    }


    //step 3

    void ApplyForces()
    {//parse the throttle value and put that info into physics engine\
    /*
    for each scenarios:
    1. compute how thrust will affect the car
    2. compute steering and the rotation of the car
    3. set appropriate drag and traction both horizontally and vertically 
    */
        if(frontTraction && rearTraction){
            ApplyForceDrive();
        }else if(frontTraction){//front wheel have traction, therefore it is drifting
            ApplyForceDrift();
        }else{//completely out of control, spinning out
            ApplyForceSpinout();
        }
        
    }

    void ApplyForceDrive(){
        //thrust
        Vector2 thrust = transform.right * accelInput * carAccelScale; //find thrust
        carRigidbody.AddForce(thrust, ForceMode2D.Force);
        
        //steering
        rotation -= steerInput * carTurnRate * carRigidbody.velocity.magnitude;//find the current rotation
        carRigidbody.MoveRotation(rotation);
        //set drag and traction
        fullTractionDrag();
    }
    void ApplyForceDrift(){
        //thrust
        Vector2 thrust = transform.right * accelInput * carAccelScale * slippingThrustRate; //find thrust
        carRigidbody.AddForce(thrust, ForceMode2D.Force);
        //steering
        float deltaSteer = ipl.GetMouseMotionX();
        rotation -= (float)Math.Abs(accelInput * carAccelScale * driftThrustSpinRate) * spinoutDirection;//how thrust will cause rotation
        rotation -= driftAngle * driftCorrectionRate;//self correction tendency
        float driftSteer = driftAngle/steerToDir - steerInput;
        rotation -= (float)Math.Abs(driftAngle) * driftSteerFactor * driftSteer;
        carRigidbody.MoveRotation(rotation);
        //set drag and traction
        driftDrag();
    }
    void ApplyForceSpinout(){
        //thrust
            Vector2 thrust = transform.right * accelInput * carAccelScale * slippingThrustRate; //find thrust
            carRigidbody.AddForce(thrust, ForceMode2D.Force);
            //thrust spinning effect
            
            rotation -= (float)Math.Abs(accelInput * carAccelScale * slippingThrustSpinRate) * spinoutDirection;
            //steering effect, reduced
            float deltaSteer = ipl.GetMouseMotionX();
            rotation -= (float)Math.Abs(accelInput * carAccelScale * slippingThrustSpinRate) * spinoutDirection;//how thrust will cause rotation
            
            float driftSteer = driftAngle/steerToDir - steerInput;
            rotation -= (float)Math.Abs(driftAngle) * slippingSteerRate * driftSteer;
            carRigidbody.MoveRotation(rotation);
            //drag, mostly sideways and some directional
            slippingDrag();
    }



    void fullTractionDrag()
    {//sets the car to go straight when it does not slip
        carRigidbody.velocity = transform.right * Vector2.Dot(carRigidbody.velocity, transform.right);
        float speed = carRigidbody.velocity.magnitude;
        carRigidbody.drag = (float)Math.Max((float)Math.Pow(speed, dragExponent) * dragFactor / 1000, 0.2);

    }

    void slippingDrag(){
        carRigidbody.angularDrag = spinoutAngDrag;
        float speed = carRigidbody.velocity.magnitude;
        carRigidbody.drag = (float)Math.Pow(speed, dragExponent) * dragFactor / 2000 + 0.8f;
    }
    void driftDrag(){
        carRigidbody.angularDrag = spinoutAngDrag;
        float speed = carRigidbody.velocity.magnitude;
        carRigidbody.drag = (float)Math.Pow(speed, dragExponent) * dragFactor / 2000 + 0.6f;
    }

}