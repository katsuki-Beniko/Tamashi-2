using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    Rigidbody2D rb;
    Vector2 move;
    private Animator _animator;

    // declare animator parameter
    public const string _horizontal = "Horizontal";
    public const string _vertical = "Vertical";
    public const string _lastHorizontal = "LastHorizontal";
    public const string _lastVertical = "LastVertical";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        MoveAction.Enable();

        // animator define
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        //Camera.main.GetComponent<CameraController>().Shake(0.5f, 0.3f, 0, true);
    

        // character moving position
        _animator.SetFloat(_horizontal, move.x);
        _animator.SetFloat(_vertical, move.y);

        if (move != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, move.x);
            _animator.SetFloat(_lastVertical, move.y);
        }
        // end here
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)transform.position + 3.0f * Time.deltaTime * move;
        transform.position = position;
    }
}
