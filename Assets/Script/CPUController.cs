using UnityEngine;
using System.Collections;

public class CPUController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("Movement Settings")]
    public Transform centerPoint; // 中心点X
    public float radius = 5f;      // 半径r
    public float changeTargetDistance = 2f; // Pを超えて離れたらターゲット更新
    public float distance = 0f;
    public bool act = false;

    public float moveSpeed = 3f;   // 移動速度
    public float accelRate = 1.0f;
    private Vector3 targetDirection;
    private Vector3 direction;
    private Vector3 currentTarget;

    public bool attack = false;
    public GameObject target;

    [SerializeField] private float[] elasptime;
    void Start()
    {
        act = true;
        rb = GetComponent<Rigidbody>();
        targetDirection = (SetNewTarget() - transform.position).normalized;
    }
    void Update()
    {
        MoveTowardsTarget(direction);

        if (act)
        {

            distance = Vector3.Distance(centerPoint.position, transform.position);

            if (distance > changeTargetDistance)
            {
                if (!attack)
                {
                    targetDirection = (SetNewTarget() - transform.position).normalized;
                }
                else
                {
                    attack = false;
                    targetDirection = (SetNewTarget() - transform.position).normalized;
                }
            }
            else
            {
                if (target) targetDirection = (target.transform.position - transform.position).normalized;
                else
                {
                    attack = false;
                    targetDirection = (SetNewTarget() - transform.position).normalized;
                }
            }

            Vector3 currentDir = rb.linearVelocity.normalized;
            Vector3 up = Vector3.up;
            Vector3 cross = Vector3.Cross(currentDir, targetDirection);
            float rotationSign = Mathf.Sign(Vector3.Dot(cross, up));
            float maxRadiansDelta = Time.deltaTime * 2.0f;

            if (rotationSign > 0f) // 左回り
            {
                direction = Vector3.RotateTowards(currentDir, targetDirection, maxRadiansDelta, 0.0f);
            }
            else // 右回り
            {
                direction = Vector3.Lerp(rb.linearVelocity.normalized, targetDirection, Time.deltaTime * 2.0f);
            }

            if (distance > 53)
            {
                direction = Vector3.Lerp(rb.linearVelocity.normalized, targetDirection, Time.deltaTime * 10.0f);
            }

            rb.angularVelocity = rb.angularVelocity.magnitude * direction.normalized;

        }
        if (accelRate != 1.0f)
        {
            elasptime[1] += Time.deltaTime;

            if (elasptime[1] >= 5.0f)
            {
                accelRate = 1.0f;
                elasptime[1] = 0.0f;
            }
        }
    }
    private Vector3 SetNewTarget()
    {
        if (rb == null) return transform.position; // 安全策

        Vector2 randomCircle = Random.insideUnitCircle * radius;
        if (!attack) currentTarget = new Vector3(centerPoint.position.x + randomCircle.x, centerPoint.position.y, centerPoint.position.z + randomCircle.y);
        else currentTarget = target.transform.position;
        direction = Vector3.Lerp(rb.linearVelocity.normalized, targetDirection, Time.deltaTime * 2.0f);
        return currentTarget;
    }

    void MoveTowardsTarget(Vector3 direction)
    {
        if (act)
        {
            Vector3 movement = direction * moveSpeed * accelRate;
            rb.linearVelocity = movement;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -7.6f - 0.22f * transform.localScale.magnitude, rb.linearVelocity.z);
        }
    }
    public void ActiveOn()
    {
        act = true;
        targetDirection = (SetNewTarget() - transform.position).normalized;
    }
}