using UnityEngine;

public class PlayerUIControl : MonoBehaviour
{
    //�@
    RectTransform rectTransform = null;

    public Transform target = null;

    // �����p
    [SerializeField] Vector2 offset = Vector2.zero;
    [SerializeField] float rate_pos = 10.0f;
    [SerializeField] float rate_offset = 2000.0f;
    [SerializeField] float rate_scale = 4.5f;

    public Vector2 edgeBuffer = Vector2.zero; // �N�����v�̗]��

    // �����ɉ������␳�̐����͈�
    public float offsetMinY = 50.0f;
    public float offsetMaxY = 200.0f;

    //�X�P�[�����O�͈̔�
    public float scaleMin = 0.5f;
    public float scaleMax = 1.5f;
    void Start()
    {
        // �@�F�v���C���[UI��RectTransform�̎擾
        rectTransform = GetComponent<RectTransform>();
        rect_name = nameUI.GetComponent<RectTransform>();

        // �C�F�A�C�R��UI��RectTransform�̎擾
        if (arrowUI) rect_arrow = arrowUI.GetComponent<RectTransform>();

        // �A�F�J�����|�^�[�Q�b�g�Ԃ̋���
        Vector3 toTarget = target.position - Camera.main.transform.position;
        float distance = toTarget.magnitude;

        // �A�F�����ɂ��I�t�Z�b�g�␳
        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY, offsetMinY, offsetMaxY);

        // �A�FUI�̏����X�P�[��
        rectTransform.localScale = Vector3.one;

        // �@�FUI�̏����ʒu
        Vector3 initialScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
        rectTransform.position = initialScreenPos;
    }

    void Update()
    {
        // �A�F�J�����|�^�[�Q�b�g�Ԃ̋���
        Vector3 toTarget = target.position - Camera.main.transform.position;
        float distance = toTarget.magnitude;

        // �B�F�����ɂ��I�t�Z�b�g�␳
        float dynamicOffsetY = rate_offset / distance;
        offset.y = Mathf.Clamp(dynamicOffsetY, offsetMinY, offsetMaxY);

        // �X�N���[�����W�擾
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

        // �J�����̌��ɂ���ꍇ�F
        if (screenPos.z < 0f)
        {
            screenPos *= -1f;
        }


        if (!arrow) PlayerName(screenPos, distance);
        else PlayerNameAndArrow(screenPos, distance);
    }
    void PlayerName(Vector3 screenPos, float distance) 
    {
        // �A�FUI�̃X�P�[���X�V
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

        // �B�F�N�����v����
        Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
        float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
        float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);
        Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);

        // �@�FUI�̈ʒu�X�V
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

        // ��]�����Z�b�g�i���ʕ\���j
        rectTransform.rotation = Quaternion.identity;
    }

    public bool arrow = false;
    public GameObject arrowUI;
    public GameObject nameUI;
    RectTransform rect_arrow = null;
    RectTransform rect_name = null;
    void PlayerNameAndArrow(Vector3 screenPos, float distance)
    {

        // �J�����̌��ɂ���ꍇ�i���ʂɍĔz�u�j
        bool isBehindCamera = screenPos.z < 0f;
        if (isBehindCamera)
        {
            screenPos *= -1f;
        }

        // ��ʓ��ɂ��邩����
        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||
                           screenPos.y < 0 || screenPos.y > Screen.height || isBehindCamera;

        if (isOffScreen)
        {
            // �� �^�[�Q�b�g����ʊO�F����\���A�ʏ�UI���\��
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

            // ��������^�[�Q�b�g�����ւ̃x�N�g��
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) * 0.5f;
            Vector3 dir = (screenPos - screenCenter).normalized;

            // ��ʒ[�ɖ���\���iedgeBuffer���Ɏ��߂�j
            Vector3 arrowScreenPos = screenCenter + dir * ((Mathf.Min(Screen.width, Screen.height) * 0.5f) - edgeBuffer.magnitude);
            arrowScreenPos.x = Mathf.Clamp(arrowScreenPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            arrowScreenPos.y = Mathf.Clamp(arrowScreenPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            rectTransform.position = arrowScreenPos;

            // ��]�iZ�������j
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f); // ��󂪏�����Ȃ� -90
        }
        else
        {
            // �� �^�[�Q�b�g����ʓ��F�ʏ�UI��\���A�����\��
            arrowUI.SetActive(false);
            nameUI.SetActive(true);

            // �X�P�[���ω�
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

            // offset ���f + clamp�i�O�̃��W�b�N�𗬗p�j
            Vector2 desiredUIPos = new Vector2(screenPos.x + offset.x, screenPos.y + offset.y);
            float clampedX = Mathf.Clamp(desiredUIPos.x, edgeBuffer.x, Screen.width - edgeBuffer.x);
            float clampedY = Mathf.Clamp(desiredUIPos.y, edgeBuffer.y, Screen.height - edgeBuffer.y);

            Vector3 targetUIPos = new Vector3(clampedX, clampedY, 0f);
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetUIPos, Time.deltaTime * rate_pos);

            // ��]�����Z�b�g�i���ʕ\���j
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
