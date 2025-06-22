using UnityEngine;

public class PlayerUIControl : MonoBehaviour
{
    //①
    RectTransform rectTransform = null;

    public Transform target = null;

    // 調整用
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] float rate_pos = 10.0f;
    [SerializeField] float rate_offset = 2000.0f;
    [SerializeField] float rate_scale = 4.5f;

    public Vector2 edgeBuffer = Vector2.zero; // クランプの余白

    // 距離に応じた補正の制限範囲
    public float offsetMinY = 50.0f;
    public float offsetMaxY = 200.0f;

    //スケーリングの範囲
    public float scaleMin = 0.5f;
    public float scaleMax = 1.5f;
    void Start()
    {
        // ①：プレイヤーUIのRectTransformの取得
        rectTransform = GetComponent<RectTransform>();
        rect_name = nameUI.GetComponent<RectTransform>();

        // ④：アイコンUIのRectTransformの取得
        if (arrowUI) rect_arrow = arrowUI.GetComponent<RectTransform>();

        // ②：カメラ－ターゲット間の距離
        Vector3 toTarget = target.position - Camera.main.transform.position;
        float distance = toTarget.magnitude;

        // ②：距離によるオフセット補正
        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY, offsetMinY, offsetMaxY);

        // ②：UIの初期スケール
        rectTransform.localScale = Vector3.one;

        // ①：UIの初期位置
        Vector3 initialScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
        rectTransform.position = initialScreenPos;
    }

    void Update()
    {
        // ②：カメラ－ターゲット間の距離
        Vector3 toTarget = target.position - Camera.main.transform.position;
        float distance = toTarget.magnitude;

        // ③：距離によるオフセット補正
        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY, offsetMinY, offsetMaxY);

        // スクリーン座標取得
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        // カメラの後ろにいる場合：
        if (screenPos.z < 0f)
        {
            screenPos *= -1f;
        }


        if (!arrow) PlayerName(screenPos, distance);
        else PlayerNameAndArrow(screenPos, distance);
    }
    void PlayerName(Vector3 screenPos, float distance) 
    {
        // ②：UIのスケール更新
        if (distance < 10.0f && distance > 5.0f)
        {
            Vector3 targetScale = Vector3.one / distance * rate_scale;
            targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
            targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
            targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
        }
        else if (distance <= 5.0f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
        }
        else if (distance >= 10.0f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
        }

        // ③：クランプ処理
        Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
        float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
        float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);
        Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);

        // ①：UIの位置更新
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

        // 回転をリセット（正面表示）
        rectTransform.rotation = Quaternion.identity;
    }

    public bool arrow = false;
    public GameObject arrowUI;
    public GameObject nameUI;
    RectTransform rect_arrow = null;
    RectTransform rect_name = null;
    void PlayerNameAndArrow(Vector3 screenPos, float distance)
    {

        // カメラの後ろにいる場合（正面に再配置）
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        // 画面内にいるか判定
        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                           screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        if (isOffScreen)
        {
            // ▼ ターゲットが画面外：矢印を表示、通常UIを非表示
            arrowUI.SetActive(true);
            nameUI.SetActive(false);

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

            // 中央からターゲット方向へのベクトル
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) * 0.5f;
            Vector3 dir = (screenPos - screenCenter).normalized;

            // 画面端に矢印を表示（edgeBuffer内に収める）
            Vector3 arrowScreenPos = screenCenter + dir * ((Mathf.Min(Screen.width, Screen.height) * 0.5f) - edgeBuffer.magnitude);
            arrowScreenPos.x = Mathf.Clamp(arrowScreenPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            arrowScreenPos.y = Mathf.Clamp(arrowScreenPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            rectTransform.position = arrowScreenPos;

            // 回転（Z軸方向）
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f); // 矢印が上向きなら -90
        }
        else
        {
            // ▼ ターゲットが画面内：通常UIを表示、矢印を非表示
            arrowUI.SetActive(false);
            nameUI.SetActive(true);

            // スケール変化
            if (distance < 10.0f && distance > 5.0f)
            {
                Vector3 targetScale = Vector3.one / distance * rate_scale;
                targetScale.x = Mathf.Clamp(targetScale.x, scaleMin, scaleMax);
                targetScale.y = Mathf.Clamp(targetScale.y, scaleMin, scaleMax);
                targetScale.z = Mathf.Clamp(targetScale.z, scaleMin, scaleMax);
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10.0f);
            }
            else if (distance <= 5.0f)
            {
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMax * Vector3.one, Time.deltaTime * 10.0f);
            }
            else if (distance >= 10.0f)
            {
                rect_name.localScale = Vector3.Lerp(rectTransform.localScale, scaleMin * Vector3.one, Time.deltaTime * 10.0f);
            }

            // offset 反映 + clamp（前のロジックを流用）
            Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
            float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

            // 回転をリセット（正面表示）
            rectTransform.rotation = Quaternion.identity;
        }
    }
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject arrow3d;
    [SerializeField] private float dis_arrow = 2.0f;
    void ArrowIcon3D(Vector3 screenPos)
    {
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                   screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        if (isOffScreen)
        {
            if (!arrow3d.activeSelf) arrow3d.SetActive(true);
            var dir = (target.transform.position - player.transform.position).normalized;
            dir.y = 0;
            arrow3d.transform.position = player.transform.position + dir * dis_arrow;
            arrow3d.transform.rotation = Quaternion.LookRotation(dir);
        }
        else arrow3d.SetActive(false);

        var dis = (arrow3d.transform.position - Camera.main.transform.position).magnitude;

        arrow3d.transform.localScale = Vector3.one * 0.2f * dis / 10.0f;
    }
}
