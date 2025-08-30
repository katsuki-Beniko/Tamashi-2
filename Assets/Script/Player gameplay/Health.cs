using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    public UnityEvent onDeath;

    int _hp;

    void Awake() => _hp = maxHealth;

    public void TakeDamage(int amount)
    {
        if (_hp <= 0) return;
        _hp = Mathf.Max(0, _hp - amount);
        if (_hp == 0) Die();
    }

    void Die()
    {
        onDeath?.Invoke();
    }

    public void HealFull() => _hp = maxHealth;
    public int Current => _hp;
}
