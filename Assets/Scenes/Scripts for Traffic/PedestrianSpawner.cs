using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{

    /*public List<GameObject> pedestrianPrefabs; // List of pedestrian prefabs
    public int numberOfPedestrians;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPedestrians());
    }

    IEnumerator SpawnPedestrians()
    {
        for (int i = 0; i < numberOfPedestrians; i++)
        {
            // Pick a random pedestrian prefab from the list
            GameObject randomPedestrianPrefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Count)];
            GameObject pedestrian = Instantiate(randomPedestrianPrefab);

            // Pick a random waypoint from the child objects
            Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));
            pedestrian.GetComponent<WaypointNavigator>().currentWaypoint = child.GetComponent<Waypoint>();
            pedestrian.transform.position = child.position;

            yield return new WaitForEndOfFrame();
        }
    }*/

    public List<GameObject> pedestrianPrefabs; // List of pedestrian prefabs
    public int numberOfPedestrians;

    private List<GameObject> spawnedPedestrians = new List<GameObject>();
    private Coroutine spawnRoutine;

    void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnPedestrians());
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        // Destroy all previously spawned pedestrians
        foreach (GameObject pedestrian in spawnedPedestrians)
        {
            if (pedestrian != null)
                Destroy(pedestrian);
        }

        spawnedPedestrians.Clear();
    }

    IEnumerator SpawnPedestrians()
    {
        for (int i = 0; i < numberOfPedestrians; i++)
        {
            GameObject randomPedestrianPrefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Count)];
            GameObject pedestrian = Instantiate(randomPedestrianPrefab);

            Transform child = transform.GetChild(Random.Range(0, transform.childCount));
            pedestrian.GetComponent<WaypointNavigator>().currentWaypoint = child.GetComponent<Waypoint>();
            pedestrian.transform.position = child.position;

            spawnedPedestrians.Add(pedestrian);

            yield return new WaitForEndOfFrame();
        }
    }
}
