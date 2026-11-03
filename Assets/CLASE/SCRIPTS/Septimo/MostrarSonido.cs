using UnityEngine;

public class MostrarSonido : MonoBehaviour
{
    // Se llama cuando se activa el script
    private void OnEnable()
    {
        TestEventos.eventoSonido += Sonido;
    }

    // Se llama cuando se desactiva el script
    private void OnDisable()
    {
        TestEventos.eventoSonido -= Sonido;
    }


    public static void Sonido()
    {
        Debug.Log("Sonido reaccionó al evento");
    }
}
