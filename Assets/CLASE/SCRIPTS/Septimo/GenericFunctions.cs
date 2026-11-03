using UnityEngine;


// Referente a creacion de nombres
public class GenericFunctions : MonoBehaviour
{
    ///<summary>
    /// Nombre de la Partida
    /// Nombre de bots para un modo de practica
    /// Codigo de activacion para bomba
    /// Codigo para desbloquear puertas
    /// </summary>  
    
    public static string RandomName(int nameLength)
    {
        string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; //De aqui sacamos todos los caracteres que querramos
        string nombre = "";

        for(int i = 0; i < nameLength; i++)
        {
            int index = Random.Range(0, caracteres.Length);
            nombre += caracteres[index];
        }


        return nombre;
    }

}
