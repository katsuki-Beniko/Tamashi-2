using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var respawn = other.GetComponent<RespawnManager>();
        if (respawn != null && !respawn.IsInvulnerable())
        {
            respawn.Respawn();
        }
    }
}
