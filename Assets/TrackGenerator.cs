using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class TrackGenerator : MonoBehaviour
{
    public GameObject tirePrefab;
    public string fileName;
    public float tireSpacing = 1.0f; // Minimum distance between tires
    public float deleteRate = 0.1f; // Time between deletions (10 tires per second)

    private List<Vector2> tirePositions = new List<Vector2>();
    private List<GameObject> tireInstances = new List<GameObject>();
    private Vector2 lastTirePosition;
    private Coroutine deleteCoroutine;

    void Update()
    {
        // Place tire on left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            PlaceTireAtMousePosition();
        }

        // Continuously place tires while holding the right mouse button
        if (Input.GetMouseButton(1)) // Right mouse button held down
        {
            TryPlaceTireContinuously();
        }

        // Start or stop continuous deletion when space bar is pressed or released
        if (Input.GetKeyDown(KeyCode.Space))
        {
            deleteCoroutine = StartCoroutine(DeleteTiresContinuously());
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopCoroutine(deleteCoroutine);
        }
    }

    void OnApplicationQuit()
    {
        SaveTrackData();
    }

    void PlaceTireAtMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        PlaceTire(mousePosition);
    }

    void PlaceTire(Vector2 position)
    {
        GameObject newTire = Instantiate(tirePrefab, position, Quaternion.identity);
        tirePositions.Add(position);
        tireInstances.Add(newTire);
        lastTirePosition = position;
    }

    void TryPlaceTireContinuously()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Vector2.Distance(mousePosition, lastTirePosition) >= tireSpacing)
        {
            PlaceTire(mousePosition);
        }
    }

    IEnumerator DeleteTiresContinuously()
    {
        while (true)
        {
            RemoveLastTire();
            yield return new WaitForSeconds(deleteRate);
        }
    }

    void RemoveLastTire()
    {
        if (tireInstances.Count > 0)
        {
            // Remove the last tire instance from the scene
            GameObject lastTire = tireInstances[tireInstances.Count - 1];
            Destroy(lastTire);

            // Remove the last tire position from the list
            tireInstances.RemoveAt(tireInstances.Count - 1);
            tirePositions.RemoveAt(tirePositions.Count - 1);

            // Update the last tire position
            if (tirePositions.Count > 0)
            {
                lastTirePosition = tirePositions[tirePositions.Count - 1];
            }

            // Update the CSV file
            SaveTrackData();
        }
    }

    void SaveTrackData()
    {
        string path = Path.Combine(Application.dataPath, fileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (Vector2 position in tirePositions)
            {
                writer.WriteLine($"{position.x},{position.y}");
            }
        }

        Debug.Log($"Track data saved to {path}");
    }
}
