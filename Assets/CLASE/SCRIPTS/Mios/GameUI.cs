using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject panelGanaste;
    [SerializeField] private GameObject panelPerdiste;


    public void MostrarGanaste()
    {
        panelGanaste.SetActive(true);
        panelPerdiste.SetActive(false);
    }


    public void MostrarPerdiste()
    {
        panelGanaste.SetActive(false);
        panelPerdiste.SetActive(true);
    }
}
