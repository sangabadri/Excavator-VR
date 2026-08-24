using UnityEngine;

public class WallBuilder : MonoBehaviour
{
    [Header("Wall Settings")]
    public GameObject brickPrefab;
    public int wallWidth = 10;
    public int wallHeight = 5;
    [Tooltip("How many layers thick the wall should be.")]
    public int wallDepth = 2; // Added Depth parameter!

    [Header("Physics Settings")]
    [Tooltip("How much force it takes to break the bricks apart.")]
    public float jointBreakForce = 8000f;

    void Start()
    {
        BuildTheWall();
    }

    void BuildTheWall()
    {
        if (brickPrefab == null) return;

        // Rotate 90° on Y so the long side runs along the wall (world X)
        Quaternion spawnRotation = brickPrefab.transform.rotation * Quaternion.Euler(0f, 90f, 0f);

        // Measure bounds on a ROTATED temp brick
        GameObject tempBrick = Instantiate(brickPrefab, Vector3.zero, spawnRotation);
        Vector3 visualSize = tempBrick.GetComponent<Collider>().bounds.size;
        Destroy(tempBrick);

        float brickWidth = visualSize.x;   // along wall length  (world X)
        float brickHeight = visualSize.y;  // vertical           (world Y)
        float brickDepth = visualSize.z;   // thickness          (world Z)
        float halfWidth = brickWidth / 2f;

        Vector3 startPos = transform.position;

        // Upgrade our array to 3 dimensions [depth][height][width]
        Rigidbody[][][] spawnedBricks = new Rigidbody[wallDepth][][];

        // ── LOOP 1: DEPTH (Z-Axis) ───────────────────────────────────
        for (int z = 0; z < wallDepth; z++)
        {
            spawnedBricks[z] = new Rigidbody[wallHeight][];

            // Offset each layer backward by the thickness of the brick
            float zPos = startPos.z + (z * brickDepth);

            // ── LOOP 2: HEIGHT (Y-Axis) ──────────────────────────────
            for (int y = 0; y < wallHeight; y++)
            {
                // MAGIC STAGGER FIX: By adding 'z' and 'y', the odd/even pattern 
                // perfectly alternates both vertically AND between front/back layers!
                bool isOddRow = ((y + z) % 2 != 0);

                int bricksInThisRow = isOddRow ? wallWidth + 1 : wallWidth;
                spawnedBricks[z][y] = new Rigidbody[bricksInThisRow];

                float yPos = startPos.y + (y * brickHeight);

                // ── LOOP 3: WIDTH (X-Axis) ───────────────────────────
                for (int x = 0; x < bricksInThisRow; x++)
                {
                    float xPos = 0f;
                    bool isHalfBrick = false;

                    if (!isOddRow)
                    {
                        // Even rows: Full bricks
                        xPos = startPos.x + (x * brickWidth);
                    }
                    else
                    {
                        if (x == 0)
                        {
                            // Left half-brick
                            isHalfBrick = true;
                            xPos = startPos.x - halfWidth;
                        }
                        else if (x == bricksInThisRow - 1)
                        {
                            // Right half-brick
                            isHalfBrick = true;
                            float rightEdgeOfWall = startPos.x + (wallWidth - 1) * brickWidth;
                            xPos = rightEdgeOfWall;
                        }
                        else
                        {
                            // Middle staggered bricks
                            xPos = startPos.x + halfWidth + ((x - 1) * brickWidth);
                        }
                    }

                    Vector3 spawnPos = new Vector3(xPos, yPos, zPos);
                    GameObject newBrick = Instantiate(brickPrefab, spawnPos, spawnRotation);
                    newBrick.transform.parent = this.transform;

                    if (isHalfBrick)
                    {
                        Vector3 scale = newBrick.transform.localScale;
                        scale.z *= 0.5f;
                        newBrick.transform.localScale = scale;
                    }

                    // Shrink collider 2%
                    BoxCollider col = newBrick.GetComponent<BoxCollider>();
                    if (col != null)
                        col.size = col.size * 0.98f;

                    Rigidbody rb = newBrick.GetComponent<Rigidbody>();
                    spawnedBricks[z][y][x] = rb;

                    // 1. Horizontal joint (Left)
                    if (x > 0)
                    {
                        FixedJoint jLeft = newBrick.AddComponent<FixedJoint>();
                        jLeft.connectedBody = spawnedBricks[z][y][x - 1];
                        jLeft.breakForce = jointBreakForce;
                        jLeft.breakTorque = jointBreakForce;
                    }

                    // 2. Vertical joint (Below)
                    if (y > 0)
                    {
                        FixedJoint jBelow = newBrick.AddComponent<FixedJoint>();
                        int indexBelow = x;
                        if (isOddRow && x > 0) indexBelow = x - 1;
                        indexBelow = Mathf.Clamp(indexBelow, 0, spawnedBricks[z][y - 1].Length - 1);

                        jBelow.connectedBody = spawnedBricks[z][y - 1][indexBelow];
                        jBelow.breakForce = jointBreakForce;
                        jBelow.breakTorque = jointBreakForce;
                    }

                    // 3. Depth joint (Behind) - Connects layers together so it's a solid wall!
                    if (z > 0)
                    {
                        FixedJoint jBehind = newBrick.AddComponent<FixedJoint>();
                        int indexBehind = x;
                        // Uses the exact same staggering alignment as the row below us
                        if (isOddRow && x > 0) indexBehind = x - 1;
                        indexBehind = Mathf.Clamp(indexBehind, 0, spawnedBricks[z - 1][y].Length - 1);

                        jBehind.connectedBody = spawnedBricks[z - 1][y][indexBehind];
                        jBehind.breakForce = jointBreakForce;
                        jBehind.breakTorque = jointBreakForce;
                    }
                }
            }
        }
    }
}