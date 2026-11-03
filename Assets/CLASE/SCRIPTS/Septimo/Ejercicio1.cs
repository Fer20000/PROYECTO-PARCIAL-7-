
using UnityEngine;

public class Ejercicio1 : MonoBehaviour
{

    // Se llama cuando se activa el script
    private void OnEnable()
    {
        TestEventos.eventoDeSaludo += MostrarSaludo;

    }
    private void OnDisable()
    {
        TestEventos.eventoDeSaludo -= MostrarSaludo;

    }
    public static void MostrarSaludo()
    {
        Debug.Log("Hola, el evento fue activado");
    }
}