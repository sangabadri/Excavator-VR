using UnityEngine;

/// <summary>
/// Attach this to every brick prefab.
///
/// ORIENTATION REMINDER (matches WallBuilder.cs):
///   The prefab is spawned with  Quaternion.Euler(0, 90, 0)  applied on top of its
///   own rotation, so after spawning:
///       brick local-X  →  world Z  (wall depth direction)
///       brick local-Z  →  world X  (wall length / width direction)
///       brick local-Y  →  world Y  (vertical, unchanged)
///
///   Half-bricks are scaled on localScale.z (which is world-X), so their
///   footprint is halved along the wall length, NOT along depth.
///
/// SUPPORT LOGIC:
///   Two sample points are cast downward from the bottom face of this brick,
///   placed at the centres of the left-half and right-half of the bottom face.
///   These correspond to the two positions where a supporting brick would sit
///   (staggered bricklaying pattern).
///
///   NESTED CONDITION:
///   1. IF no brick is found below (neither left-half nor right-half ray hits)
///   2.   THEN check lateral neighbours (left side OR right side).
///          IF either neighbour is absent  →  isKinematic = false.
///
///   A brick that still has something below it is ALWAYS kept kinematic,
///   regardless of its side neighbours.
///
///   The check runs every <checkInterval> seconds so it stays cheap.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BrickSupportChecker : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Layer mask for bricks only – set this to your brick layer for performance.")]
    public LayerMask brickLayerMask = ~0; // default: everything

    [Tooltip("Maximum distance below the brick to look for a supporting brick.")]
    public float belowCheckDistance = 0.5f;

    [Tooltip("Maximum distance left / right to look for a lateral neighbour brick.")]
    public float sideCheckDistance = 0.6f;

    [Tooltip("Seconds between support checks. Lower = more responsive, higher = cheaper.")]
    public float checkInterval = 0.15f;

    // ── private ──────────────────────────────────────────────────────────────
    private Rigidbody rb;
    private BoxCollider brickCollider;
    private float timer;

    // Pre-computed in Start so we don't allocate every tick
    private Vector3 localLeftBottomPoint;   // in local space
    private Vector3 localRightBottomPoint;  // in local space

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        brickCollider = GetComponent<BoxCollider>();

        ComputeSamplePoints();

        // Stagger each brick's first check slightly so they don't all fire at
        // the same frame (avoids a CPU spike when the wall first spawns).
        timer = Random.Range(0f, checkInterval);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        // Already dynamic – nothing left to check.
        if (!rb.isKinematic) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;

        CheckSupport();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Pre-compute the two bottom sample points in LOCAL space so they follow
    /// scale changes (half-bricks etc.) automatically via TransformPoint().
    ///
    /// The collider's local centre + extents give us the bottom face in local
    /// space.  We then place the two points at the left-quarter and
    /// right-quarter of the bottom face along the local Z axis (= world X,
    /// i.e. wall-length direction – see orientation note at the top).
    /// </summary>
    void ComputeSamplePoints()
    {
        if (brickCollider == null)
        {
            // Fallback: treat the pivot as the centre, half-unit extents
            localLeftBottomPoint  = new Vector3(-0.25f, -0.5f, 0f);
            localRightBottomPoint = new Vector3( 0.25f, -0.5f, 0f);
            return;
        }

        Vector3 centre  = brickCollider.center;   // local-space collider centre
        Vector3 extents = brickCollider.size * 0.5f; // half-sizes in local space

        float bottomY = centre.y - extents.y;     // local Y of the bottom face

        // Along local-Z (= world-X after the 90° Y-rotation):
        //   left  = centre.z - half the extent  (quarter of the full width from centre)
        //   right = centre.z + half the extent
        // We use half the half-extent so the point sits at the quarter mark, i.e.
        // the centre of each sub-half — exactly where the brick below would be.
        float quarterZ = extents.z * 0.5f;

        // NOTE: we keep local-X = centre.x (mid-depth) so the ray starts inside
        // the footprint regardless of wall depth.
        localLeftBottomPoint  = new Vector3(centre.x, bottomY, centre.z - quarterZ);
        localRightBottomPoint = new Vector3(centre.x, bottomY, centre.z + quarterZ);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckSupport()
    {
        // Convert local sample points to world space (respects scale & rotation)
        Vector3 leftWorld  = transform.TransformPoint(localLeftBottomPoint);
        Vector3 rightWorld = transform.TransformPoint(localRightBottomPoint);

        bool leftBelow  = Physics.Raycast(leftWorld,  Vector3.down, belowCheckDistance, brickLayerMask);
        bool rightBelow = Physics.Raycast(rightWorld, Vector3.down, belowCheckDistance, brickLayerMask);

        // ── Condition 1: is there ANY brick directly below? ───────────────
        bool anyBelow = leftBelow || rightBelow;

        if (!anyBelow)
        {
            // ── Condition 2 (nested): check left and right side neighbours ─
            // The wall runs along world X, so neighbours sit in the +X / -X direction.
            // We cast horizontally from the collider's world-space centre.
            Vector3 centre = brickCollider != null
                ? transform.TransformPoint(brickCollider.center)
                : transform.position;

            bool hasLeftNeighbour  = Physics.Raycast(centre, Vector3.left,  sideCheckDistance, brickLayerMask);
            bool hasRightNeighbour = Physics.Raycast(centre, Vector3.right, sideCheckDistance, brickLayerMask);

            // If EITHER side neighbour is absent → unsupported → go dynamic
            if (!hasLeftNeighbour || !hasRightNeighbour)
            {
                EnableGravity();
            }
        }
        // If anyBelow == true we do nothing – brick is still supported, stay kinematic.
    }

    // ─────────────────────────────────────────────────────────────────────────
    void EnableGravity()
    {
        rb.isKinematic = false;
        // The check loop in Update() will exit early from now on.
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ── Editor visualisation ─────────────────────────────────────────────────
// #if UNITY_EDITOR
//     void OnDrawGizmosSelected()
//     {
//         // Show the two sample points and the downward rays so you can verify
//         // placement in the Scene view.
//         BoxCollider col = GetComponent<BoxCollider>();
//         if (col == null) return;

//         // Recompute live (editor may not have called Start)
//         Vector3 centre  = col.center;
//         Vector3 extents = col.size * 0.5f;
//         float bottomY   = centre.y - extents.y;
//         float quarterZ  = extents.z * 0.5f;

//         Vector3 lLocal = new Vector3(centre.x, bottomY, centre.z - quarterZ);
//         Vector3 rLocal = new Vector3(centre.x, bottomY, centre.z + quarterZ);

//         Vector3 lWorld = transform.TransformPoint(lLocal);
//         Vector3 rWorld = transform.TransformPoint(rLocal);

//         // Sample points
//         Gizmos.color = Color.cyan;
//         Gizmos.DrawSphere(lWorld, 0.04f);
//         Gizmos.DrawSphere(rWorld, 0.04f);

//         // Downward rays
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawRay(lWorld, Vector3.down * belowCheckDistance);
//         Gizmos.DrawRay(rWorld, Vector3.down * belowCheckDistance);

//         // Lateral rays from collider centre
//         Vector3 centreWorld = transform.TransformPoint(centre);
//         Gizmos.color = Color.magenta;
//         Gizmos.DrawRay(centreWorld, Vector3.left  * sideCheckDistance);
//         Gizmos.DrawRay(centreWorld, Vector3.right * sideCheckDistance);
//     }
// #endif
}
