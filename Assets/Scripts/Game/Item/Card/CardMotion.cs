using UnityEngine;

[RequireComponent(typeof(Card), typeof(BoxCollider))]
public class CardMotion : MonoBehaviour
{
    public enum CStatus
    {
        Idle,
        Hovered,
        Selected,
        Preview,
        Returning,
    }

    public CStatus Status = CStatus.Idle;
    // public Vector3 BaseScale;
    public BoxCollider Collider;

    public Vector3 TargetPos = new Vector3();
    public Quaternion TargetRot = new Quaternion();
    public Vector3 TargetScale = new Vector3();

    public bool MotionEnabled = false;

    void Start()
    {
        // BaseScale = transform.localScale;
        Collider = GetComponent<BoxCollider>();
    }

    void LateUpdate()
    {
        if (MotionEnabled) HandleCardMotion();
    }

    void HandleCardMotion()
    {
        float k = 1f - Mathf.Exp(-12f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, TargetPos, k);
        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRot, k);
        transform.localScale = Vector3.Lerp(transform.localScale, TargetScale, k);
    }

    void SetStatus(CStatus status)
    {
        // switch (Status)
        // {
        //     case CStatus.Idle:
        //         Collider.enabled = true;
        //         break;
        //     case CStatus.Hovered:
        //         Collider.enabled = true;
        //         break;
        //     case CStatus.Selected:
        //         Collider.enabled = false;
        //         break;
        //     case CStatus.Preview:
        //         Collider.enabled = false;
        //         break;
        //     case CStatus.Returning:
        //         Collider.enabled = false;
        //         break;
        // }
        Status = status;
    }
}
