using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    private static InputManager _instance = null;
    
    public static InputManager Instance { get => _instance; private set => _instance = value;}
    
    
    private PlayerControls playerControls;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("Instance created");
        }
        playerControls = new PlayerControls();

    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    // DISPARO
    private void OnDisable()
    {
        playerControls.Disable();
    }
    // Consigue el movimienot en XY
    public Vector2 GetMoveInput()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }
    
    // CAMARA
    public bool IsMoveInputPressed()
    {
        return playerControls.Player.Move.IsPressed();
    }
    
    // Si estas presionando el boton de correr
    public bool WasRunInputPressed()
    {
        return playerControls.Player.Run.IsPressed();
    }
    
    // Si te estas moviendo hacia atras
    public bool IsMovingBackwards()
    {
        return playerControls.Player.Move.ReadValue<Vector2>().y < 0;
    }
    
    // Si te estas moviendo hacia los lados 
    public bool IsMovingOnXAxis()
    {
        return playerControls.Player.Move.ReadValue<Vector2>().x != 0;
    }

    // CAMARA
    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    //public bool IsShooting()
    //{
    //    return playerControls.Player.Fire.IsPressed();
    //}

    internal bool IsMainFirePressed()
    {
        return playerControls.Player.Fire.IsPressed();
    }
}
