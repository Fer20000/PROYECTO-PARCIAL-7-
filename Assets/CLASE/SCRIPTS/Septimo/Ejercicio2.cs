
using UnityEngine;

public class Ejercicio2 : MonoBehaviour
{
    private void OnEnable()
    {
      TestEventos.eventoDinamico += MostrarA;
      TestEventos.eventoDinamico += MostrarB;
    }
    private void OnDisable()
    {
        TestEventos.eventoDinamico -= MostrarA;
        TestEventos.eventoDinamico -= MostrarB;
    }

    public static void MostrarA()
    {
        Debug.Log("Suscriptor A reaccionó");
    }

    public static void MostrarB()
    {
        Debug.Log("Suscriptor B reaccionó");
    }
}