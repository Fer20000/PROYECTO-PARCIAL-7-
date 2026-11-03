using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System.Xml.Serialization;
using Fusion.Addons.SimpleKCC;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck))]
public class MovementController : NetworkBehaviour
{
    private InputManager inputManager;
    private Rigidbody rbPlayer;

    [SerializeField] private Animator _animator;

    [SerializeField] private SimpleKCC kcc;

    public override void Spawned() //Se manda a llamar si el objeto es spawneado
    {
        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<SimpleKCC>();

    }

    private void Start()
    {
        inputManager = InputManager.Instance;
    }
    


    public override void FixedUpdateNetwork()
    {

        if (!HasStateAuthority)
            return; //Si no tenemos autoridad de Input que termine la ejecución


        if (GetInput (out NetworkInfoData input))
        {
            Movement(input);
            _animator.SetBool("IsWalking", input.moveInputPressed);
            _animator.SetBool("IsRunning", input.runInputPressed);
            _animator.SetFloat("WalkingZ", input.movementAxis.y);
            _animator.SetFloat("WalkingX", input.movementAxis.x);
        }
       
    }


    #region Movimiento

    [SerializeField] private float walkSpeed = 500f;
    [SerializeField] private float runSpeed = 900f;
    //[SerializeField] private float crouchSpeed = 3.9f;

    private void Movement(NetworkInfoData input)
    {
        kcc.Move(kcc.TransformRotation *
            new Vector3(input.movementAxis.x, 0, input.movementAxis.y) *
            (Time.deltaTime * Speed(input)));

        //rbPlayer.lineartvelocity = transform.localRotation *
        //                    new Vector3(input.movementAxis.x, 0, input.movementAxis.y) *
        //                    (Time.deltaTime * Speed(input)));
    }



    private float Speed(NetworkInfoData input)
    {
        return input.movingBackwards || input.movingOnXAxis ? walkSpeed :
            input.runInputPressed ? runSpeed : walkSpeed;
    }


    #endregion

  
}