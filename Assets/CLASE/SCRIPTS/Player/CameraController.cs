
using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Fusion.Addons.SimpleKCC;
using UnityEngine.Windows;

public class CameraController : NetworkBehaviour
{
    [SerializeField] Transform cameraPivot;
    [SerializeField] KCC kcc;

    [Header("Camera Settings")]
    [SerializeField] private Transform player;

    [SerializeField] private float mouseSensitivity = 1;

    private float cam;

    [FormerlySerializedAs("smooth")]
    [SerializeField] private float smoothnes;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;

    private Vector2 camVelociy;
    private Vector2 smoothVelocity;

    [Header("Blob Movement")]
    [SerializeField] private float walkingSpeed = 1f;

    [SerializeField, Range(0, 0.1f)]
    private float walkingAmplitude = 0.015f; // Que tanto se mueve hacia los lados al caminar

    [SerializeField, Range(0, 0.1f)]
    private float runningAmplitude = 0.015f; // Que tanto se mueve hacia los lados al correr

    [SerializeField, Range(0, 15)]
    private float walkingFrequency = 10.0f; // La frecuencia con la que se mueve al caminar

    [SerializeField, Range(10, 20)]
    private float runningFrequency = 18f; // La frecuencia con la que se mueve al correr

    [SerializeField] private float resetPosSpeed = 3.0f; // Cuando dejas de moverte que regrese al centro
    //[SerializeField] private float toggleSpeed = 3.0f;

    private Vector3 startPos; // Posicion inicial de la cabeza , el centro

    [SerializeField] private bool moveHead;

    private Vector2 head;

    private InputManager inputManager;

    private SimpleKCC playerkcc;

    private void Awake()
    {
        startPos = transform.localPosition;
    }

    //Cuando el jugador hace Spawn
    public override void Spawned()
    {
        playerkcc = GetComponentInParent<SimpleKCC>();
        inputManager = InputManager.Instance;

        ConfigurePlayerLayers();

        if (!HasInputAuthority) // Si no tenemos autoridad de input, bos desactiva la camara , audio listener y demas
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;

            //player = FindObjectOfType<MovementController>().transform;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && GetInput(out NetworkInfoData input))
        {
            RotateCamera(input);
        }
    }

    //public override void FixedUpdateNetwork()
    //{
    //    if (GetInput(out NetworkInfoData input))
    //    {
    //        RotateCamera(input);
    //        if (!moveHead) return;
    //        BlobMove();
    //        ResetPosition();
    //    }
    //    //else
    //    //{
    //    //    transform.localRotation = Quaternion.AngleAxis(-cam, Vector3.right);
    //    //}
    //}

    private void RotateCamera(NetworkInfoData input)
    {
        Vector2 rawFrameVelocity = Vector2.Scale(
            input.mousePos,
            Vector2.one * mouseSensitivity
        );

        float smoothing = Mathf.Max(1f, smoothnes);

        smoothVelocity = Vector2.Lerp(
            smoothVelocity,
            rawFrameVelocity,
            1f / smoothing
        );

        camVelociy.x += smoothVelocity.x;

        playerkcc.SetLookRotation(
            Quaternion.AngleAxis(camVelociy.x, Vector3.up)
        );

        //cameraPivot.localRotation = Quaternion.Euler(-camVelociy.y, 0, 0);

        //if (kcc != null)
        //{
        //    Quaternion yawRotation = Quaternion.Euler(0, camVelociy.x, 0);
        //    kcc.SetLookRotation(yawRotation);
        //}

        //transform.localRotation = Quaternion.AngleAxis(-camVelociy.y, Vector3.right); // Rota la camara hacia arriba y abajo. La rotacion esta en X.
        //player.localRotation = Quaternion.AngleAxis(camVelociy.x, Vector3.up);
    }

    private void LateUpdate()
    {
        if (!Object || !HasInputAuthority)
            return;

        if (inputManager == null)
            inputManager = InputManager.Instance;

        if (inputManager == null)
            return;

        Vector2 mouseDelta = inputManager.GetMouseDelta();

        cam += mouseDelta.y * mouseSensitivity;

        cam = Mathf.Clamp(cam, minAngleY, maxAngleY);

        transform.localRotation = Quaternion.AngleAxis(
            -cam,
            Vector3.right
        );

        if (moveHead)
        {
            BlobMove();
            ResetPosition();
        }
    }

    public override void Render()
    {

    }

    private void BlobMove()
    {
        if (!inputManager.IsMoveInputPressed()) // Si no presiono ningun input
        {
            return; // termina el metodo
        }

        if (inputManager.IsMoveInputPressed()) // Pregunto si me estoy moviendo
        {
            if (inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis()) // Me estoy moviendo hacia atras o hacia los lados?
            {
                transform.localPosition += FootStepMotion();
            }
            else // Entonces me muevo hacia adelante
            {
                if (inputManager.WasRunInputPressed()) // Estoy corriendo?
                {
                    transform.localPosition += RunningFootStepMotion();
                }
                else // Estoy caminando
                {
                    transform.localPosition += FootStepMotion();
                }
            }
        }

        if (inputManager.IsMoveInputPressed())
        {
            transform.localPosition += inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis()
                ? FootStepMotion()
                : inputManager.WasRunInputPressed()
                    ? RunningFootStepMotion()
                    : FootStepMotion();
        }
    }

    private void ResetPosition()
    {
        if (transform.localPosition == startPos) return; // Si la camara ya esta en la pos inicial, no hace nada

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            startPos,
            resetPosSpeed * Time.deltaTime
        );
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;

        return pos;
    }

    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;

        return pos;
    }

    private void ConfigurePlayerLayers()
    {
        Transform root = transform.root;

        int layer = HasInputAuthority
            ? LayerMask.NameToLayer("Self")
            : LayerMask.NameToLayer("Default");

        foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.gameObject.layer = layer;
        }
    }
}
