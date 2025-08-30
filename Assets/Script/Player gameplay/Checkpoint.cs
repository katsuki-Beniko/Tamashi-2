using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var respawn = other.GetComponent<RespawnManager>();
        if (respawn != null)
        {
            respawn.SetCheckpoint(transform.position);
            // Optional: play VFX/SFX, change sprite to “activated”
        }
    }
}
