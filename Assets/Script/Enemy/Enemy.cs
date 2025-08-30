using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 10f;
    public float stoppingDistance = 0.5f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer = 1 << 3; // Ground layer (layer 3)
    public float obstacleDetectionDistance = 1.5f;
    public float avoidanceForce = 2f;
    public float raycastSpread = 45f; // Degrees for side raycasts
    public int avoidanceRays = 3; // Number of rays for detection

    [Header("Sight Settings")]
    public float sightRange = 6f;
    public float sightAngle = 90f;
    public LayerMask sightMask;
    public Transform eyes;
    
    [Header("Dynamic Sight")]
    public float sightUpdateSpeed = 5f; // How fast sight direction updates
    public float minimumMovementForSight = 0.1f; // Minimum speed to update sight direction

    [Header("Patrol Settings")] // ADDED: Missing patrol variables
    public float searchTime = 2f;
    public float roamRadius = 4f;
    public float roamInterval = 3f;
    public float minPatrolDistance = 1f;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 roamTarget;
    private float roamTimer;
    private Vector2 lastSeenPos;
    private float searchTimer;

    // Dynamic player tracking
    private PlayerSwitcher playerSwitcher;
    private Player currentTarget;

    // Obstacle avoidance
    private Vector2 avoidanceDirection;
    private bool isAvoiding = false;
    
    // Dynamic sight direction
    private Vector2 currentSightDirection = Vector2.right; // Current sight direction
    private Vector2 lastMovementDirection = Vector2.right; // Last significant movement direction

    private enum State { Patrol, Chase, Search }
    private State current = State.Patrol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        // Find the PlayerSwitcher to track active player
        playerSwitcher = FindFirstObjectByType<PlayerSwitcher>();

        // Configure Rigidbody2D for smooth movement
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        PickNewRoamTarget();

        Debug.Log($"Enemy initialized at {startPos}, first roam target: {roamTarget}");
    }

    void Update()
    {
        UpdateCurrentTarget();
        UpdateSightDirection(); // Update sight based on movement
        
        switch (current)
        {
            case State.Patrol:
                Patrol();
                if (CanSeePlayer())
                {
                    current = State.Chase;
                    Debug.Log($"Enemy spotted {currentTarget.name}!");
                }
                break;

            case State.Chase:
                Chase();
                if (!CanSeePlayer())
                {
                    current = State.Search;
                    lastSeenPos = currentTarget.transform.position;
                    searchTimer = searchTime;
                    Debug.Log($"Lost sight of {currentTarget.name}, searching...");
                }
                break;

            case State.Search:
                Search();
                break;
        }
    }
    
    private void UpdateSightDirection()
    {
        // Get current movement velocity
        Vector2 currentVelocity = rb.linearVelocity;
        
        // Only update sight direction if moving fast enough
        if (currentVelocity.magnitude > minimumMovementForSight)
        {
            // Update the target sight direction to movement direction
            lastMovementDirection = currentVelocity.normalized;
        }
        
        // Smoothly rotate sight direction towards movement direction
        currentSightDirection = Vector2.Lerp(currentSightDirection, lastMovementDirection, 
                                           sightUpdateSpeed * Time.deltaTime).normalized;
    }

    private void UpdateCurrentTarget()
    {
        if (playerSwitcher != null)
        {
            Player activePlayer = playerSwitcher.GetActivePlayer();
            if (activePlayer != currentTarget)
            {
                currentTarget = activePlayer;
                if (current == State.Chase)
                {
                    Debug.Log($"Enemy now chasing {currentTarget.name}!");
                }
            }
        }
    }

    void Patrol()
    {
        roamTimer -= Time.deltaTime;

        // Move towards current roam target with obstacle avoidance
        MoveTowardsWithAvoidance(roamTarget);

        // Check if reached target or timer expired
        float distanceToTarget = Vector2.Distance(transform.position, roamTarget);
        if (distanceToTarget < stoppingDistance || roamTimer <= 0f)
        {
            PickNewRoamTarget();
            Debug.Log($"Enemy picking new patrol target: {roamTarget}");
        }
    }

    void Chase()
    {
        if (currentTarget != null)
        {
            MoveTowardsWithAvoidance(currentTarget.transform.position);
        }
    }

    void Search()
    {
        if (searchTimer > 0f)
        {
            searchTimer -= Time.deltaTime;
            MoveTowardsWithAvoidance(lastSeenPos);

            // Check if can see player again during search
            if (CanSeePlayer())
            {
                current = State.Chase;
                Debug.Log($"Found {currentTarget.name} again during search!");
                return;
            }
        }
        else
        {
            current = State.Patrol;
            PickNewRoamTarget();
            Debug.Log("Search completed, returning to patrol");
        }
    }

    void PickNewRoamTarget()
    {
        roamTimer = roamInterval;

        // Generate a better random target that avoids obstacles
        Vector2 randomDirection;
        Vector2 newTarget;
        int attempts = 0;

        do
        {
            randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minPatrolDistance, roamRadius);
            newTarget = startPos + (randomDirection * randomDistance);
            attempts++;

            // Check if the new target is not inside an obstacle
            if (!Physics2D.OverlapCircle(newTarget, 0.5f, obstacleLayer))
            {
                break;
            }
        }
        while (attempts < 20);

        roamTarget = newTarget;

        Debug.Log($"New patrol target: {roamTarget}, Distance: {Vector2.Distance(transform.position, roamTarget)}");
    }

    void MoveTowardsWithAvoidance(Vector2 target)
    {
        Vector2 currentPos = transform.position;
        Vector2 desiredDirection = (target - currentPos).normalized;

        // Check for obstacles and calculate avoidance
        Vector2 avoidance = CalculateObstacleAvoidance(desiredDirection);

        // Combine desired direction with avoidance
        Vector2 finalDirection = (desiredDirection + avoidance).normalized;

        float distance = Vector2.Distance(currentPos, target);

        // Only move if not at target
        if (distance > stoppingDistance)
        {
            Vector2 targetVelocity = finalDirection * moveSpeed;
            Vector2 velocityChange = targetVelocity - rb.linearVelocity;

            // Apply force with acceleration limit
            Vector2 force = velocityChange * acceleration;
            rb.AddForce(force, ForceMode2D.Force);

            // Clamp velocity to max speed
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            // Slow down when close to target
            rb.linearVelocity *= 0.8f;
        }
    }

    Vector2 CalculateObstacleAvoidance(Vector2 desiredDirection)
    {
        Vector2 avoidance = Vector2.zero;
        Vector2 currentPos = transform.position;

        // Cast multiple rays to detect obstacles
        for (int i = 0; i < avoidanceRays; i++)
        {
            float angle = 0f;

            if (avoidanceRays > 1)
            {
                // Spread rays from -raycastSpread to +raycastSpread
                angle = Mathf.Lerp(-raycastSpread, raycastSpread, (float)i / (avoidanceRays - 1));
            }

            // Calculate ray direction
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * desiredDirection;

            // Cast ray to detect obstacles
            RaycastHit2D hit = Physics2D.Raycast(currentPos, rayDirection, obstacleDetectionDistance, obstacleLayer);

            if (hit.collider != null)
            {
                // Calculate avoidance force perpendicular to the obstacle
                Vector2 hitNormal = hit.normal;
                float avoidanceWeight = 1f - (hit.distance / obstacleDetectionDistance);

                // Add weighted avoidance in the direction away from obstacle
                avoidance += hitNormal * avoidanceWeight * avoidanceForce;

                Debug.DrawRay(currentPos, rayDirection * hit.distance, Color.red, 0.1f);
                isAvoiding = true;
            }
            else
            {
                Debug.DrawRay(currentPos, rayDirection * obstacleDetectionDistance, Color.green, 0.1f);
            }
        }

        // If no obstacles detected, reset avoidance flag
        if (avoidance == Vector2.zero)
        {
            isAvoiding = false;
        }

        return avoidance.normalized;
    }

    bool CanSeePlayer()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            return false;

        Vector2 dirToPlayer = (currentTarget.transform.position - eyes.position).normalized;
        float distToPlayer = Vector2.Distance(eyes.position, currentTarget.transform.position);

        // Range check
        if (distToPlayer > sightRange) return false;

        // Angle check - now uses dynamic sight direction instead of transform.right
        float angle = Vector2.Angle(currentSightDirection, dirToPlayer);
        if (angle > sightAngle * 0.5f) return false;

        // Raycast check (for walls and obstacles)
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, dirToPlayer, distToPlayer, sightMask | obstacleLayer);
        if (hit.collider != null && hit.collider.transform != currentTarget.transform)
            return false;

        return true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Player hitPlayer = col.collider.GetComponent<Player>();
        if (hitPlayer != null && playerSwitcher != null)
        {
            if (playerSwitcher.IsMainPlayer(hitPlayer))
            {
                // Main player got hit - game over
                playerSwitcher.OnMainPlayerHit();
            }
            else
            {
                // Player 2 got hit - force switch to main player with cooldown
                playerSwitcher.OnPlayer2Hit();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize sight range
        if (eyes != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyes.position, sightRange);

            // Draw dynamic sight cone based on movement direction
            Vector3 sightDir3D = new Vector3(currentSightDirection.x, currentSightDirection.y, 0);
            Vector3 rightBoundary = Quaternion.Euler(0, 0, sightAngle * 0.5f) * sightDir3D * sightRange;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, -sightAngle * 0.5f) * sightDir3D * sightRange;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary);
            Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary);
            
            // Draw center sight line to show current direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(eyes.position, eyes.position + sightDir3D * sightRange);
        }

        // Show roam area and current target
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPos, roamRadius);

        // Show current patrol target
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(roamTarget, 0.2f);
        Gizmos.DrawLine(transform.position, roamTarget);

        // Show obstacle detection range
        Gizmos.color = isAvoiding ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionDistance);
        
        // Show current movement direction
        if (Application.isPlaying && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.white;
            Vector3 velocityDir = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0).normalized;
            Gizmos.DrawLine(transform.position, transform.position + velocityDir * 2f);
        }
    }

    // Add this method to your existing EnemyChaseAI class
    public bool CanSeeAnyPlayer()
    {
        if (playerSwitcher != null)
        {
            // Check if we can see any player (active or inactive)
            for (int i = 0; i < playerSwitcher.players.Length; i++)
            {
                Player player = playerSwitcher.players[i];
                if (player != null && CanSeeSpecificPlayer(player))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanSeeSpecificPlayer(Player player)
    {
        if (player == null || !player.gameObject.activeInHierarchy) 
            return false;

        Vector2 dirToPlayer = (player.transform.position - eyes.position).normalized;
        float distToPlayer = Vector2.Distance(eyes.position, player.transform.position);

        // Range check
        if (distToPlayer > sightRange) return false;

        // Angle check - now uses dynamic sight direction instead of transform.right
        float angle = Vector2.Angle(currentSightDirection, dirToPlayer);
        if (angle > sightAngle * 0.5f) return false;

        // Raycast check (for walls and obstacles)
        RaycastHit2D hit = Physics2D.Raycast(eyes.position, dirToPlayer, distToPlayer, sightMask | obstacleLayer);
        if (hit.collider != null && hit.collider.transform != player.transform)
            return false;

        return true;
    }
}
