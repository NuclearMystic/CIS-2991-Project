namespace CIS2991Project.Core
{
    // Marker so different projectile implementations (player-fired vs enemy-fired) can recognize
    // and ignore each other on trigger overlap, without either side needing to depend on the
    // other's concrete (and currently private, nested) type.
    public interface IProjectile
    {
    }
}
