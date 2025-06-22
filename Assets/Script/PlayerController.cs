using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    public GameObject target;

    public GameObject player;

    public Rigidbody rb;
    public Vector3 move;
    private Vector3 moveForward;
    private Vector3 cameraForward;
    public float moveRate = 1.0f;

    private Vector3 _prevPosition;
    private Vector3 velocity;
    public bool isGround = false;
    public bool isSlope = false;

    [Header("Parameter")]
    public float moveSpeed = 10.0f;
    public float turnTimeRate = 8.0f;
    public float jumpPower = 10.0f;
    public float runRate = 1.5f;

    [Header("Control Flug")]//�g�p�\���ǂ���
    public bool cont = false;
    public bool mov = true;
    public bool rot = true;

    [Header("Input Flug")]//������͒����ǂ���
    [SerializeField]
    private PlayerInput playerInput;
    public bool openInp = false;
    public bool avoidInp = false;
    public int dash = 0;
    public bool reverse = false;

    [Header("ActionParameter")]
    public AnimationCurve accelRate;
    public float movetime = 0.0f;

    [Header("SE")]
    public AudioSource[] se;

    [Header("RayCast")]
    [SerializeField]
    private float stepDistance = 0.3f;  //���C���΂�����
    RaycastHit resultFloor;
    Ray rayUp;
    Ray rayDown;
    Ray rayFoward;
    Ray rayBack;
    Ray rayRight;
    Ray rayLeft;
    private void Awake()
    {
        target = player;
        _prevPosition = target.transform.position;
    }
    void Update()
    {
        if (cont)
        {
            if (target != player)
            {
                target = player;
            }

            if (target == null) return;
            else
            {
                if (mov) Move();
                Ray();
            }
        }
        else
        {
            se[0].volume = 0.0f;
            se[1].volume = 0.0f;
            se[0].pitch = 0.0f;
            se[1].pitch = 0.0f;
        }
    }
    private void Move()
    {
        cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0.0f, 1)).normalized;
        moveForward = cameraForward * move.z + Camera.main.transform.right * move.x;
        moveForward = moveForward.normalized;

        if (move.magnitude > 0)
        {
            if (movetime <= 1.0f) movetime += Time.deltaTime;
            rb.linearVelocity = moveForward * moveSpeed * move.magnitude *  runRate *accelRate.Evaluate(movetime) + new Vector3(0, rb.linearVelocity.y, 0);
            if (isSlope) rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, resultFloor.normal);
        }
        else
        {
            if (isGround || isSlope) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            else rb.linearVelocity = rb.linearVelocity;
            if (movetime != 0.0f) movetime = 0.0f;
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (cont)
        {
            if (!reverse) move = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y) * moveRate;
            else move = new Vector3(-context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y) * moveRate;
        }
    }
    public void OnAccel(InputAction.CallbackContext context)
    {
        if(context.started) runRate = 2.0f;
        if(context.canceled) runRate = 1.0f;
    }
    public void Ray()
    {
        rayUp = new Ray(target.transform.position, Vector3.up);
        rayDown = new Ray(target.transform.position, Vector3.down);
        rayFoward = new Ray(target.transform.position, Vector3.forward);
        rayBack = new Ray(target.transform.position, -Vector3.forward);
        rayRight = new Ray(target.transform.position, Vector3.right);
        rayLeft = new Ray(target.transform.position, -Vector3.right);

        var dis = stepDistance * target.transform.localScale.magnitude * 1.3f;

        Debug.DrawRay(rayUp.origin, rayUp.direction * dis, Color.green);
        Debug.DrawRay(rayDown.origin, rayDown.direction * dis, Color.red);
        Debug.DrawRay(rayFoward.origin, rayFoward.direction * dis, Color.blue);
        Debug.DrawRay(rayBack.origin, rayBack.direction * dis, Color.green);
        Debug.DrawRay(rayRight.origin, rayRight.direction * dis, Color.red);
        Debug.DrawRay(rayLeft.origin, rayLeft.direction * dis, Color.blue);

        if (Physics.Raycast(rayUp, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           || Physics.Raycast(rayDown, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           || Physics.Raycast(rayFoward, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           || Physics.Raycast(rayBack, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           || Physics.Raycast(rayRight, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           || Physics.Raycast(rayLeft, out resultFloor, dis, LayerMask.GetMask("Field", "Move")))
        {
            isGround = true;
        }
        else if (!Physics.Raycast(rayUp, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           & !Physics.Raycast(rayDown, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           & !Physics.Raycast(rayFoward, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           & !Physics.Raycast(rayBack, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           & !Physics.Raycast(rayRight, out resultFloor, dis, LayerMask.GetMask("Field", "Move"))
           & !Physics.Raycast(rayLeft, out resultFloor, dis, LayerMask.GetMask("Field", "Move")))
        {
            isGround = false;
        }

        if (Physics.Raycast(rayUp, out resultFloor, dis, LayerMask.GetMask("Slope"))
   || Physics.Raycast(rayDown, out resultFloor, dis, LayerMask.GetMask("Slope"))
   || Physics.Raycast(rayFoward, out resultFloor, dis, LayerMask.GetMask("Slope"))
   || Physics.Raycast(rayBack, out resultFloor, dis, LayerMask.GetMask("Slope"))
   || Physics.Raycast(rayRight, out resultFloor, dis, LayerMask.GetMask("Slope"))
   || Physics.Raycast(rayLeft, out resultFloor, dis, LayerMask.GetMask("Slope")))
        {
            isSlope = true;
        }
        else if (!Physics.Raycast(rayUp, out resultFloor, dis, LayerMask.GetMask("Slope"))
           & !Physics.Raycast(rayDown, out resultFloor, dis, LayerMask.GetMask("Slope"))
           & !Physics.Raycast(rayFoward, out resultFloor, dis, LayerMask.GetMask("Slope"))
           & !Physics.Raycast(rayBack, out resultFloor, dis, LayerMask.GetMask("Slope"))
           & !Physics.Raycast(rayRight, out resultFloor, dis, LayerMask.GetMask("Slope"))
           & !Physics.Raycast(rayLeft, out resultFloor, dis, LayerMask.GetMask("Slope")))
        {
            isSlope = false;
        }
    }
    public void Sig_ContOn() { cont = true; }
    public void Sig_ContOff() { cont = false; }
}
