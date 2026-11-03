
using UnityEngine;

public class Ejercicio3 : MonoBehaviour
{
    // Se llama cuando se activa el script
    private void OnEnable()
    {
        TestEventos.eventoSuscriptores += MostrarLuz;
    }

    // Se llama cuando se desactiva el script
    private void OnDisable()
    {
        TestEventos.eventoSuscriptores -= MostrarLuz;
    }
    public static void MostrarLuz()
    {
        Debug.Log("Luz reaccionó al evento");
    }

}