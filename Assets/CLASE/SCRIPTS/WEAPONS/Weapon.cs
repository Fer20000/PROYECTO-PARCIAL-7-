using Fusion;
using UnityEngine;

/// <summary>
/// Esta clase contendrá todo lo básico de un arma
/// Variables
/// Compoirtamiento
/// 
/// Pero todo de manera genérica. El comportamiento de cada arma
/// se dará en la herencia
/// 
/// Una clase abstracta nos permite crear métodos que de principio no contienen lógica
/// para depués escribir su funcionamiento en la herencia.
/// Y que la implementación de esos métodos sea obligatoria.
/// El abstracto lo usas cuando quieres un método que si o si obligatoriamente exista en toda la herencia
/// 
/// Cuando hago un metodo virtual
/// Yo ese método le puedo escribir una lógica
/// Y posteriormente en herencia sobreescribir esa lógica pero pudiendo mantener la lógica base
/// El virtual lo usas cuando puede o no pasar en herencia
/// </summary>


public abstract class Weapon : NetworkBehaviour
{
    public ShootType shootType;
    [SerializeField] protected Transform cameraPos;


    public int damage; // Municion de mi cargador actual
    public int actualAmmo; // Tamaño del cargador
    public int magazineSize; // Capacidad del cargador
    public int maxAmmoCapacity; // Capacidad máxima de almaccenamiento
    public float maxDistance;
    public LayerMask layers;
    public Transform shootPoint;
    public NetworkPrefabRef projectible;
    public abstract void RaycastShoot(); // Disparo con raycast
    public abstract void PhysicShoot(); // Disparo con proyectil físico
    public abstract void Reload(); // Recarga




}



public enum ShootType
{
    Raycast, Physic
}

