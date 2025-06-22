using UnityEngine;

public class UIClampControl : MonoBehaviour
{
    RectTransform rectTransform = null;
    [Header("追従")]
    public Transform target = null;

    [Header("調整パラメータ")]
    [SerializeField] float rate_pos = 10.0f;
    [SerializeField] float rate_offset = 2000.0f;
    [SerializeField] float rate_scale = 4.5f;
    public Vector2 edgeBuffer = Vector2.zero; // クランプの余白

    [Header("距離")]
    public float distanceMin = 10.0f;
    public float distanceMax = 30.0f;

    [Header("オフセット")]
    [SerializeField] Vector2 offset = Vector2.zero;
    public float offsetMinY = 50.0f;
    public float offsetMaxY = 200.0f;

    [Header("スケール")]
    public float scaleMin = 0.5f;
    public float scaleMax = 1.5f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        rect_name = nameUI.GetComponent<RectTransform>();
        if (arrowUI) rect_arrow = arrowUI.GetComponent<RectTransform>();
        if (charaUI) rect_chara = charaUI.GetComponent<RectTransform>();

        rectTransform.localScale = Vector3.one;
    }
    void Update()
    {
        //ターゲットの位置（ワールド座標→スクリーン座標）
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        //カメラ裏での映り込み防止
        if (screenPos.z < 0f)
        {
            screenPos *= -1f;
        }

        //ターゲット-カメラ間の距離
        Vector3 toTarget = target.position - Camera.main.transform.position;
        float distance = toTarget.magnitude;

        //オフセット
        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY, offsetMinY, offsetMaxY);

        // 配置（減衰補正）/スケーリング
        if (!arrow) PlayerName(screenPos, distance); //ネームUIのみ
        else
        {
            //PlayerNameAndArrow(screenPos, distance); //ネームUI ⇄ 矢印UI
            CharaIcon(screenPos, distance); //ネームUI ⇄ キャラアイコン
        }

        if (arrow3d) ArrowIcon3D(screenPos); //3D矢印オブジェクト
    }
    void PlayerName(Vector3 screenPos, float distance) //ネームUI用
    {
        //スクリーン座標＋オフセット、画面内クランプ
        Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
        float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
        float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);
        Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);

        rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

        //スケーリング補正
        if (distance < distanceMax && distance > distanceMin)
        {
            Vector3 targetScale = Vector3.one / distance * rate_scale;
            targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
            targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
            targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
        }
        else if (distance <= distanceMin)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
        }
        else if (distance >= distanceMax)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
        }
    }
    [Header("矢印UI")]
    public bool arrow = false;
    public GameObject arrowUI;
    public GameObject nameUI;
    RectTransform rect_arrow = null;
    RectTransform rect_name = null;
    void PlayerNameAndArrow(Vector3 screenPos, float distance) // ネームUI＆矢印UI用
    {
        //カメラ裏の判定
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        //画面内/外の判定
        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                   screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        if (isOffScreen) //画面外
        {
            //矢印UI表示＆ネームUI非表示
            arrowUI.SetActive(true);
            nameUI.SetActive(false);

            //スケーリング補正
            if (distance < 10.0f && distance > 5.0f)
            {
                Vector3 targetScale = Vector3.one * distance;
                targetScale.x = Mathf.Clamp(targetScale.x, 0.1f, 1.0f);
                targetScale.y = Mathf.Clamp(targetScale.y, 0.1f, 1.0f);
                targetScale.z = Mathf.Clamp(targetScale.z, 0.1f, 1.0f);
                rect_arrow.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
            }
            else if (distance <= 5.0f)
            {
                rect_arrow.localScale = Vector3.Lerp(rectTransform.localScale, 0.1f * Vector3.one, Time.deltaTime * 10.0f);
            }
            else if (distance >= 10.0f)
            {
                rect_arrow.localScale = Vector3.Lerp(rectTransform.localScale, 1.0f * Vector3.one, Time.deltaTime * 10.0f);
            }

            //画面中心からターゲット（画面上）への向き
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) * 0.5f;
            Vector3 dir = (screenPos - screenCenter).normalized;

            //スクリーン座標＋オフセット、画面内クランプ
            Vector3 arrowScreenPos = screenCenter + dir * ((Mathf.Min(Screen.width, Screen.height) * 0.5f) - edgeBuffer.magnitude);
            arrowScreenPos.x = Mathf.Clamp(arrowScreenPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            arrowScreenPos.y = Mathf.Clamp(arrowScreenPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            rectTransform.position = arrowScreenPos;

            //ターゲットの方向に回転させる
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
        else //画面内
        {
            //矢印UI非表示＆ネームUI表示
            arrowUI.SetActive(false);
            nameUI.SetActive(true);

            //スケーリング補正
            if (distance < distanceMax && distance > distanceMin)
            {
                Vector3 targetScale = Vector3.one / distance * rate_scale;
                targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
                targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
                targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
            }
            else if (distance <= distanceMin)
            {
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
            }
            else if (distance >= distanceMax)
            {
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
            }

            //スクリーン座標＋オフセット、画面内クランプ、減衰補正
            Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
            float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

            //向きリセット
            rectTransform.rotation = Quaternion.identity;
        }
    }
    [Header("キャラアイコン")]
    public GameObject charaUI;
    RectTransform rect_chara = null;
    void CharaIcon(Vector3 screenPos, float distance) //ネームUI ⇄キャラアイコン
    {
        //カメラ裏の判定
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        //画面内/外の判定
        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                   screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        //画面内/外でのUI切り替え
        if (isOffScreen)
        {
            //矢印UI表示＆ネームUI非表示
            arrowUI.SetActive(true);
            nameUI.SetActive(false);
        }
        else
        {
            //矢印UI非表示＆ネームUI表示
            arrowUI.SetActive(false);
            nameUI.SetActive(true);
        }

        //スケーリング補正
        if (distance < distanceMax && distance > distanceMin)
        {
            Vector3 targetScale = Vector3.one / distance * rate_scale;
            targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
            targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
            targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
            rect_name.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
        }
        else if (distance <= distanceMin)
        {
            rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
        }
        else if (distance >= distanceMax)
        {
            rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
        }

        //スクリーン座標＋オフセット、画面内クランプ、減衰補正
        Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
        float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
        float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);
        Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);

        rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

        //キャラアイコンの向き
        if (arrowUI.activeSelf)
        {
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) * 0.5f;
            Vector3 dir = (screenPos - screenCenter).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90f);
            rect_chara.localRotation = Quaternion.Euler(0, 0, -(angle - 90f));
        }
        else
        {
            rectTransform.rotation = Quaternion.identity;
        }
    }
    [Header("３D矢印")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject arrow3d;
    [SerializeField] private float dis_arrow = 2.0f;
    void ArrowIcon3D(Vector3 screenPos) //矢印オブジェクト
    {
        //カメラ裏の判定
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        //画面内/外の判定
        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                   screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        if (isOffScreen) //画面外
        {
            if(!arrow3d.activeSelf) arrow3d.SetActive(true);

            //プレイキャラからターゲットへの向き（XZ平面）
            var dir = (target.transform.position - player.transform.position).normalized;
            dir.y = 0;

            //プレイキャラ近くに配置＆ターゲットの方向に回転させる
            arrow3d.transform.position = player.transform.position + dir * dis_arrow;
            arrow3d.transform.rotation = Quaternion.LookRotation(dir);
        }
        else arrow3d.SetActive(false);

        //スケーリング補正（画面上でのサイズ保持）
        var dis = (arrow3d.transform.position - Camera.main.transform.position).magnitude;
        arrow3d.transform.localScale = Vector3.one * 0.2f * dis / 10.0f;
    }
}