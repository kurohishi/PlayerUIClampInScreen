using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public bool cont = true;

    public bool focus = false;
    //�J�����̑���X�s�[�h
    private Vector3 speed_m;
    private Vector3 speed_k;
    private Vector3 speed_p;

    // �v���C���[�Ǐ]
    public GameObject TargetObject; //�v���C���[�I�u�W�F�N�g
    public float Height = 1.5f; //�J���������̃I�t�Z�b�g
    public float Distance = 15.0f;//�J�����Ƃ̃I�t�Z�b�g
    public float RotAngle = 0.0f;//����(��)�����̃J�����p�x
    public float HeightAngle = 10.0f;//����(�c)�����̃J�����p�x
    public float dis_min = 5.0f; //���グ�����̃J���������i�C�Ӂj
    public float dis_mdl = 10.0f; //�ʏ�̃J���������i�C�Ӂj
    public Vector3 nowPos;//���݂̃v���C���[�ʒu
    public float nowRotAngle;//���݂̐���(��)�����̃J�����p�x
    public float nowHeightAngle; //���݂̐���(�c)�����̃J�����p�x

    //��������
    public bool EnableAtten = true;//���������p�t���O
    public float AttenRate = 3.0f;
    public float ForwardDistance = 2.0f;
    private Vector3 addForward;
    private Vector3 prevTargetPos;
    public float RotAngleAttenRate = 5.0f;
    public float AngleAttenRate = 1.0f;

    //���b�N�I���@�\
    public bool rock = false; //���b�N�I���p�t���O
    public bool aim = false; //���b�N�I���ւ̒��ڗp�t���O
    public bool icon = true; //�^�[�Q�b�g�A�C�R��
    public GameObject RockonTarget; //�^�[�Q�b�g�Ώ�
    public GameObject targetIcon; //�^�[�Q�b�g�A�C�R��
    public GameObject targetPoint; //�G�C���A�C�R��
    public GameObject aimObject;

    //�ǂ߂荞�ݖh�~
    public bool EnableWallDetection = true; //�ǂ߂荞�ݖh�~�p�t���O
    [SerializeField]
    public float wallDetectionDistance = 0.3f;//�ǂ����m���鋗��

    public bool zoom = false; //�Y�[���p�t���O
    public bool reverse = false;
    void Start()
    {
        nowPos = TargetObject.transform.position + Vector3.up * Height;
        transform.position = nowPos;
        nowRotAngle = RotAngle;
        nowHeightAngle = HeightAngle;
        zoom = false;
    }
    void LateUpdate()
    {
        if (!cont)
        {
            if (aimObject)
            {
                var dir = (aimObject.transform.position - TargetObject.transform.position).normalized;

                nowPos = Vector3.Lerp(nowPos, TargetObject.transform.position + Vector3.up * Height, Mathf.Clamp01(Time.unscaledDeltaTime * AttenRate));

                transform.position = nowPos - dir * 10.0f + Vector3.up * 3.0f;
                transform.rotation = Quaternion.LookRotation((nowPos - transform.position).normalized);
            }
        }
        else
        {
            RotAngle -= (speed_m.x + speed_k.x + speed_p.x) * Time.unscaledDeltaTime * 100.0f;//�L�[���͂ɂ�鐅���p�x�̉��Z
            HeightAngle += (speed_m.z + speed_k.z + speed_p.z) * Time.unscaledDeltaTime * 40.0f; //�L�[���͂ɂ�鐂���p�x�̉��Z
            HeightAngle = Mathf.Clamp(HeightAngle, -40.0f, 60.0f); //���������̊p�x����
            Distance = Mathf.Clamp(Distance, 2.0f, 40.0f);//�J������������

            if (EnableAtten)
            {
                var target = TargetObject.transform.position; //�^�[�Q�b�g�ʒu���v���C���[�ɐݒ肷��

                if (rock)
                {
                    if (RockonTarget != null)
                    {
                        target = RockonTarget.transform.position; //�^�[�Q�b�g�����b�N�I���Ώۂ̈ʒu�ɂ���
                    }
                    else
                    {
                        rock = false;
                    }
                }

                var halfPoint = (TargetObject.transform.position + target) / 2;
                var deltaPos = halfPoint - prevTargetPos;//�ʒu�̔���������
                prevTargetPos = halfPoint;
                deltaPos *= ForwardDistance;

                addForward += deltaPos * Time.deltaTime * 20.0f;
                addForward = Vector3.Lerp(addForward, Vector3.zero, Time.unscaledDeltaTime * AttenRate);//�ǉ����̈ړ���

                nowPos = Vector3.Lerp(nowPos, halfPoint + Vector3.up * Height + addForward, Mathf.Clamp01(Time.unscaledDeltaTime * AttenRate));
            }
            else nowPos = TargetObject.transform.position + Vector3.up * Height;

            if (EnableAtten)
            {
                if (aimObject)
                {
                    var dir = (aimObject.transform.position - TargetObject.transform.position).normalized;
                    dir.x = 0;
                    var angle = Vector3.SignedAngle((aimObject.transform.position - TargetObject.transform.position).normalized, Vector3.forward, Vector3.up);
                    RotAngle = angle;
                    nowRotAngle = RotAngle;
                }
                else
                {
                    nowRotAngle = Mathf.Lerp(nowRotAngle, RotAngle, Time.unscaledDeltaTime * RotAngleAttenRate);
                }
            }
            else nowRotAngle = RotAngle;

            if (EnableAtten) nowHeightAngle = Mathf.Lerp(nowHeightAngle, HeightAngle, Time.unscaledDeltaTime * RotAngleAttenRate);
            else nowHeightAngle = HeightAngle;

            if (HeightAngle > 30)
            {
                Distance = Mathf.Lerp(Distance, dis_mdl * HeightAngle / 30.0f, Time.unscaledDeltaTime);
            }
            else if (HeightAngle <= 30 && HeightAngle >= -10)
            {
                Distance = Mathf.Lerp(Distance, dis_mdl, Time.unscaledDeltaTime);
            }
            else if (HeightAngle < -10)
            {
                Distance = Mathf.Lerp(Distance, dis_min, Time.unscaledDeltaTime);
            }

            var deg = Mathf.Deg2Rad;
            var cx = Mathf.Sin(nowRotAngle * deg) * Mathf.Cos(nowHeightAngle * deg) * Distance;
            var cz = -Mathf.Cos(nowRotAngle * deg) * Mathf.Cos(nowHeightAngle * deg) * Distance;
            var cy = Mathf.Sin(nowHeightAngle * deg) * Distance;
            transform.position = nowPos + new Vector3(cx, cy, cz);

            var rot = Quaternion.LookRotation((nowPos - transform.position).normalized);
            transform.rotation = rot;
        }
    }
    public void OnMauseCamera(InputAction.CallbackContext context)
    {
        if (Input.GetMouseButton(1))
        {
            if (cont)
            {
                if (!reverse) speed_m = new Vector3(context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
                else speed_m = new Vector3(-context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            speed_m = Vector3.zero;
        }
    }
    public void OnKeyCamera(InputAction.CallbackContext context)
    {
        if (cont)
        {
            if (!reverse) speed_k = new Vector3(context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
            else speed_k = new Vector3(-context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
        }
    }
    public void OnPadCamera(InputAction.CallbackContext context)
    {
        if (cont)
        {
            if (!reverse) speed_p = new Vector3(context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
            else speed_p = new Vector3(-context.ReadValue<Vector2>().x, 0f, -context.ReadValue<Vector2>().y) * 2.0f;
        }
    }
    public void ResetCamera()
    {
        EnableAtten = false;
        HeightAngle = 10.0f;
        RotAngle = Vector3.SignedAngle(TargetObject.transform.forward, Vector3.forward, Vector3.up);
        EnableAtten = true;
    }
    public void ResetAngle()
    {
        EnableAtten = false;
        var ro = RotAngle;
        if (ro < -180.0f)
        {
            var ang = ro - (int)(ro / 360.0f) * 360.0f;
            if (ang < -180.0f)
            {
                RotAngle = 360.0f + ang;
            }
            else
            {
                RotAngle = ang;
            }
        }
        else if (ro > 180.0f)
        {
            var ang = ro - (int)(ro / 360.0f) * 360.0f;
            if (ang > 180.0f)
            {
                RotAngle = -360.0f + ang;
            }
            else
            {
                RotAngle = ang;
            }
        }
    }
    public void AimObject(GameObject obj)
    {
        var dir = (obj.transform.position - TargetObject.transform.position).normalized;
        dir.x = 0;
        var angle = Vector3.SignedAngle((obj.transform.position - TargetObject.transform.position).normalized, Vector3.forward, Vector3.up);

        if (Mathf.Abs(angle) + Mathf.Abs(RotAngle) >= 180.0f)
        {
            if (RotAngle > 0.0f && angle < 0.0f)
            {
                angle = -1 * angle;
                angle = 360.0f - angle;
            }
            else if (angle > 0.0f && RotAngle < 0.0f)
            {
                angle = -1 * angle;
                angle = angle - 360.0f;
            }
        }
        EnableAtten = true;
        RotAngle = angle;
        nowRotAngle = Mathf.Lerp(nowRotAngle, RotAngle, Time.unscaledDeltaTime * RotAngleAttenRate);

        if (dir.z > 0)
        {
            //HeightAngle = -Vector3.SignedAngle(dir, Vector3.forward, Vector3.right);
            HeightAngle = 10.0f;
        }
        else
        {
            //HeightAngle = Vector3.SignedAngle(dir, -Vector3.forward, -Vector3.right);
            HeightAngle = 10.0f;
        }
        nowHeightAngle = Mathf.Lerp(nowHeightAngle, HeightAngle, Time.unscaledDeltaTime * RotAngleAttenRate);
    }
    public void Sig_ContOn() { cont = true; }
}
