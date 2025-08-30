using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Header("Optional")]
    public float respawnInvulnerableTime = 0.75f;

    private Vector3 _spawnPoint;
    private Rigidbody2D _rb;
    private Collider2D _col;
    private MonoBehaviour[] _movementScripts; // e.g., your Player controller, dash, etc.
    private bool _invulnerable;
    private float _timer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _movementScripts = GetComponents<MonoBehaviour>(); // crude but fine for jams
        _spawnPoint = transform.position; // initial spawn
    }

    public void SetCheckpoint(Vector3 pos)
    {
        _spawnPoint = pos;
    }

    public void Respawn()
    {
        // Reset physics
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // Teleport and re-enable
        transform.position = _spawnPoint;

        // Optional short invulnerability window
        if (respawnInvulnerableTime > 0f)
        {
            _invulnerable = true;
            _timer = respawnInvulnerableTime;
            SetMovementEnabled(true); // keep movement
            // You could flash sprite here
        }
    }

    void Update()
    {
        if (_invulnerable)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) _invulnerable = false;
        }
    }

    public bool IsInvulnerable() => _invulnerable;

    private void SetMovementEnabled(bool enabled)
    {
        // If you want to freeze controls during respawn, toggle your movement scripts here.
        // For jam speed, we leave them enabled.
        // Example:
        // foreach (var m in _movementScripts) if (m != this) m.enabled = enabled;
    }
}
