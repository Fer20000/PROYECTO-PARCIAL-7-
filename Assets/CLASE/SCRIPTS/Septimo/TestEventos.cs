
using System;
using UnityEngine;

public class TestEventos : MonoBehaviour
{
    public static event Action eventoDeSaludo;
    public static event Action eventoDinamico;
    public static event Action eventoSuscriptores;
    public static event Action eventoUI;
    public static event Action eventoSonido;

    private bool primeraInvocacion = true;


    private void Update()
    {
        if (InputManager.Instance.IsMainFirePressed())
        {
            // Ejercicio 1
            eventoDeSaludo?.Invoke();

            // Ejercicio 2
            eventoDinamico?.Invoke();

            if (primeraInvocacion)
            {
                primeraInvocacion = false;
                eventoDinamico -= Ejercicio2.MostrarB;
            }

            // Ejercicio 3
            eventoSuscriptores?.Invoke();

            eventoUI?.Invoke();
            eventoSonido?.Invoke();

        }
    }
}