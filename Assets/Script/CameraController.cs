using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public bool cont = true;

    public bool focus = false;
    //カメラの操作スピード
    private Vector3 speed_m;
    private Vector3 speed_k;
    private Vector3 speed_p;

    // プレイヤー追従
    public GameObject TargetObject; //プレイヤーオブジェクト
    public float Height = 1.5f; //カメラ高さのオフセット
    public float Distance = 15.0f;//カメラとのオフセット
    public float RotAngle = 0.0f;//水平(横)方向のカメラ角度
    public float HeightAngle = 10.0f;//垂直(縦)方向のカメラ角度
    public float dis_min = 5.0f; //見上げた時のカメラ距離（任意）
    public float dis_mdl = 10.0f; //通常のカメラ距離（任意）
    public Vector3 nowPos;//現在のプレイヤー位置
    public float nowRotAngle;//現在の水平(横)方向のカメラ角度
    public float nowHeightAngle; //現在の垂直(縦)方向のカメラ角度

    //減衰挙動
    public bool EnableAtten = true;//減衰挙動用フラグ
    public float AttenRate = 3.0f;
    public float ForwardDistance = 2.0f;
    private Vector3 addForward;
    private Vector3 prevTargetPos;
    public float RotAngleAttenRate = 5.0f;
    public float AngleAttenRate = 1.0f;

    //ロックオン機能
    public bool rock = false; //ロックオン用フラグ
    public bool aim = false; //ロックオンへの注目用フラグ
    public bool icon = true; //ターゲットアイコン
    public GameObject RockonTarget; //ターゲット対象
    public GameObject targetIcon; //ターゲットアイコン
    public GameObject targetPoint; //エイムアイコン
    public GameObject aimObject;

    //壁めり込み防止
    public bool EnableWallDetection = true; //壁めり込み防止用フラグ
    [SerializeField]
    public float wallDetectionDistance = 0.3f;//壁を検知する距離

    public bool zoom = false; //ズーム用フラグ
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
            RotAngle -= (speed_m.x + speed_k.x + speed_p.x) * Time.unscaledDeltaTime * 100.0f;//キー入力による水平角度の加算
            HeightAngle += (speed_m.z + speed_k.z + speed_p.z) * Time.unscaledDeltaTime * 40.0f; //キー入力による垂直角度の加算
            HeightAngle = Mathf.Clamp(HeightAngle, -40.0f, 60.0f); //垂直方向の角度制限
            Distance = Mathf.Clamp(Distance, 2.0f, 40.0f);//カメラ距離制限

            if (EnableAtten)
            {
                var target = TargetObject.transform.position; //ターゲット位置をプレイヤーに設定する

                if (rock)
                {
                    if (RockonTarget != null)
                    {
                        target = RockonTarget.transform.position; //ターゲットをロックオン対象の位置にする
                    }
                    else
                    {
                        rock = false;
                    }
                }

                var halfPoint = (TargetObject.transform.position + target) / 2;
                var deltaPos = halfPoint - prevTargetPos;//位置の微小増加量
                prevTargetPos = halfPoint;
                deltaPos *= ForwardDistance;

                addForward += deltaPos * Time.deltaTime * 20.0f;
                addForward = Vector3.Lerp(addForward, Vector3.zero, Time.unscaledDeltaTime * AttenRate);//追加分の移動量

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
