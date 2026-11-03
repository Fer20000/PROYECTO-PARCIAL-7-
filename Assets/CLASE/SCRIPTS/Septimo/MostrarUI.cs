using UnityEngine;

public class MostrarUI : MonoBehaviour
{
    // Se llama cuando se activa el script
    private void OnEnable()
    {

        TestEventos.eventoUI += EventoMostrarUI;
    }

    // Se llama cuando se desactiva el script
    private void OnDisable()
    {
        TestEventos.eventoUI -= EventoMostrarUI;
    }

    public static void EventoMostrarUI()
    {
        Debug.Log("UI reaccionó al evento");
    }
}
