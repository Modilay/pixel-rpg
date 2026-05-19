using Godot;
public interface IDamageable
{
    bool IsAlive { get; }
    public DamageResult TakeDamage(
        float damage,
        float knockbackForce = 0f,
        Vector2? knockbackDirection = null,
        Node2D damageSource = null,
        float hurtDuration = 0.2f
    );
}