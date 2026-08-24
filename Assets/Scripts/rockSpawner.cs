using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Add your different rock prefabs to this list")]
    public GameObject[] rockPrefabs; // This creates the list/columns in the Inspector

    [Tooltip("Total number of rocks to spawn")]
    public int numberOfRocks = 30;

    [Header("Spawn Volume Area")]
    [Tooltip("The size of the box area where rocks will randomly appear")]
    public Vector3 spawnAreaSize = new Vector3(2f, 1f, 4f);

    void Start()
    {
        // Check if the list has at least one rock in it before trying to spawn
        if (rockPrefabs != null && rockPrefabs.Length > 0)
        {
            SpawnRocks();
        }
        else
        {
            Debug.LogWarning("Rock Spawner has no rock prefabs assigned!");
        }
    }

    void SpawnRocks()
    {
        for (int i = 0; i < numberOfRocks; i++)
        {
            // Generate a random position within the box
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            Vector3 spawnPos = transform.position + randomOffset;

            // Pick a random rock prefab from the array
            int randomIndex = Random.Range(0, rockPrefabs.Length);
            GameObject rockToSpawn = rockPrefabs[randomIndex];

            // Spawn the chosen rock and make it a child of the spawner
            Instantiate(rockToSpawn, spawnPos, Random.rotation, transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.4f);
        Gizmos.DrawCube(transform.position, spawnAreaSize);
    }
}