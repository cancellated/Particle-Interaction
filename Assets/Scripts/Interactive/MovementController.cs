using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;
    public float rayDistance = 2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        move += new Vector3(h, 0, v);

        if (Input.GetKey(KeyCode.LeftShift))
            move += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl))
            move += Vector3.down;

        move = move.normalized;
        Vector3 velocity = new Vector3(move.x * moveSpeed, move.y * verticalSpeed, move.z * moveSpeed);
        rb.velocity = velocity;

        // …‰œﬂºÏ≤‚
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.OnHitByRay();
            }
        }
    }
}
