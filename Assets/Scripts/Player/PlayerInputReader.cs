using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public float Move { get; private set; }
    public event Action JumpPressed;
    public event Action JumpReleased;
    public event Action AttackPressed;

    InputAction _move, _jump, _attack;

    void Awake()
    {
        _move = new InputAction("Move", InputActionType.Value);
        _move.AddCompositeBinding("1DAxis")
             .With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
        _move.AddCompositeBinding("1DAxis")
             .With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        _move.AddBinding("<Gamepad>/leftStick/x");

        _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        _attack = new InputAction("Attack", InputActionType.Button, "<Keyboard>/j");
        _attack.AddBinding("<Mouse>/leftButton");
    }

    void OnEnable()
    {
        _move.Enable(); _jump.Enable(); _attack.Enable();
        _jump.performed += _ => JumpPressed?.Invoke();
        _jump.canceled += _ => JumpReleased?.Invoke();
        _attack.performed += _ => AttackPressed?.Invoke();
    }

    void OnDisable()
    {
        _move.Disable(); _jump.Disable(); _attack.Disable();
    }

    void Update() => Move = _move.ReadValue<float>();
}