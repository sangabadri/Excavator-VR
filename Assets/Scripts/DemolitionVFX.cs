using UnityEngine;

public class DemolitionVFX : MonoBehaviour
{
    [Header("VFX Assignment")]
    public GameObject dustPrefab;

    private bool hasSpawned = false;
    private bool isShuttingDown = false;

    // This prevents dust from spawning everywhere when you close Play Mode
    void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    // This triggers the EXACT moment the 'Breakable' script deletes the brick
    void OnDestroy()
    {
        // Don't spawn if the game is closing or we aren't in Play Mode
        if (isShuttingDown || !Application.isPlaying) return;

        SpawnDust(transform.position);
    }

    // Keep this as a backup just in case a brick takes a hard hit but doesn't fully shatter
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 4f)
        {
            SpawnDust(collision.contacts[0].point);
        }
    }

    void SpawnDust(Vector3 spawnPoint)
    {
        // The !hasSpawned check ensures we don't accidentally spawn two dust clouds for one brick
        if (!hasSpawned && dustPrefab != null)
        {
            Instantiate(dustPrefab, spawnPoint, Quaternion.identity);
            hasSpawned = true;
        }
    }
}