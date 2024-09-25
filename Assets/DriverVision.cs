using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DriverVision : MonoBehaviour
{
    //for the car vision:
    public float maxDistance = 300f; // Maximum distance the rays will check
    public LayerMask wallLayer; // Layer to detect the walls (tires)
    private float[] visionArray; // Array to store distances
    // Start is called before the first frame update
    void Awake()
    {
        // Calculate the size of the vision array based on the number of rays
        int centralRays = 60; // 60 rays in the central region
        int sideRays = 30; // 30 rays on each side (60 degrees total, 1 ray every 2 degrees)
        visionArray = new float[centralRays + sideRays * 2 +1]; // Total 120 rays
    }
    //The implementation of the vision can be done through See()
    public float[] See(){
        PerformVision();
        return visionArray;
    }
    //vision for the car
    void PerformVision()
    {
        Vector2 carPosition = transform.position;
        int arrayIndex = 0;

        // Left side region (-90 to -30 degrees)
        for (int angle = -90; angle < -30; angle += 2)
        {
            CastRayAtAngle(carPosition, angle, ref arrayIndex);
        }

        // Central region (-30 to +30 degrees)
        for (int angle = -30; angle <= 30; angle += 1)
        {
            CastRayAtAngle(carPosition, angle, ref arrayIndex);
        }

        // Right side region (+32 to +90 degrees)
        for (int angle = 32; angle <= 90; angle += 2)
        {
            CastRayAtAngle(carPosition, angle, ref arrayIndex);
        }
    }

    void CastRayAtAngle(Vector2 carPosition, float angle, ref int arrayIndex)
    {
        // Calculate the direction based on the angle
        Vector2 direction = Quaternion.Euler(0, 0, angle) * transform.right;

        // Cast the ray
        RaycastHit2D hit = Physics2D.Raycast(carPosition, direction, maxDistance, wallLayer);

        // Store the distance in the array
        if (hit.collider != null)
        {
            visionArray[arrayIndex] = hit.distance;
        }
        else
        {
            visionArray[arrayIndex] = maxDistance; // No hit, assume clear up to maxDistance
        }

        // Draw the ray in the Scene view for 0.1 seconds
        float distance = hit.collider != null ? hit.distance : maxDistance;
        Debug.DrawRay(carPosition, direction * distance, Color.red, 0.1f);


        // Increment the array index
        arrayIndex++;
    }

    public float[] GetVisionArray()
    {
        return visionArray;
    }
}
