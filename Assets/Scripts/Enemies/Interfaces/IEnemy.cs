/// <summary>
/// Cosas en comun entre enemigos
/// </summary>
public interface IEnemy
{
    // Propiedad para gestionar la salud del enemigo
    int Health { get; set; }

    // Método para recibir daño
    void TakeDamage(int damage);

    // Método de movimiento o deambule
    void Wander();
}