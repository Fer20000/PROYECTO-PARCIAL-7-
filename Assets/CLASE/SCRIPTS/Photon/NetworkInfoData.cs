using UnityEngine;
using Fusion;

/// <summary>
/// Este struct va a contener toda la información que queremos mandar al servidor.
/// Debeb heredar de INetworkInput para que el servidor la pueda leer.
/// 
/// Esta información se envia cada que se registra un input
/// 
/// Photon no sabe si lo que se manda aqui es un input o no
/// </summary>

public struct NetworkInfoData : INetworkInput
{
    public Vector2 movementAxis;
    public Vector3 rotation;

    public Vector3 shootPosition;
    public Vector3 shootDirection;

    public bool runInputPressed;
    public bool movingBackwards;
    public bool movingOnXAxis;
    public bool moveInputPressed;

    public bool isShooting;
    public Vector2 mousePos;

    public Vector3 handPosition;
    public Quaternion handRotation;

}
