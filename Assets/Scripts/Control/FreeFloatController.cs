using UnityEngine;

public class FreeFloatController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveForce = 30f; // 移动推力
    public float maxSpeed = 15f; // 最大速度限制
    public float drag = 3f; // 移动阻力

    private Rigidbody rb;
    private Vector3 inputDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = drag; // 初始化线性阻力
    }

    void Update()
    {
        // 获取键盘输入（所有输入范围自动归一化为[-1,1]）
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical"); // W/S
        float ascend = Input.GetKey(KeyCode.Space) ? 1 : 0; // 上升
        float descend = Input.GetKey(KeyCode.LeftControl) ? 1 : 0; // 下降

        // 组合标准化移动方向（局部坐标系）
        inputDirection = new Vector3(horizontal, ascend - descend, vertical).normalized;
    }

    void FixedUpdate()
    {
        // 将局部坐标系方向转换为世界坐标系
        Vector3 worldMove = transform.TransformDirection(inputDirection);

        // 施加力并限制速度
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(worldMove * moveForce, ForceMode.Force);
        }

        // 可选：手动限制速度（更严格）
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
