using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RearDriftMark : MonoBehaviour
{
    public CarMoveScript cms;
    public TrailRenderer trailRend;
    void Awake(){
       cms = GetComponentInParent<CarMoveScript>();
       trailRend = GetComponent<TrailRenderer>();
       trailRend.emitting = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cms.GetRearTraction() == false){//if there are no traction, there are drifting marks
            trailRend.emitting = true;
        }else{
            trailRend.emitting = false;
        }
    }
}
