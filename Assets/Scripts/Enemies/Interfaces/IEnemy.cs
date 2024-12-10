/// <summary>
/// Cosas en comun entre enemigos
/// </summary>
public interface IEnemy
{
    // Propiedad para gestionar la salud del enemigo
    int Health { get; set; }

    // Método para recibir daño
    void TakeDamage(int damage);

    // Método para realizar alguna acción (por ejemplo, atacar)
    void PerformAction();

    // Método de ataque
    void Attack();

    // Método de movimiento o deambule
    void Wander();
}