using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputAction MoveAction;
    Rigidbody2D rb;
    Vector2 move;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        MoveAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        Camera.main.GetComponent<CameraController>().Shake(0.5f, 0.3f, 0, true);
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)transform.position + 3.0f * Time.deltaTime * move;
        transform.position = position;
    }
}
