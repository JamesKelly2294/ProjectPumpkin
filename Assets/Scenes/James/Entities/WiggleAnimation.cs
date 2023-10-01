using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiggleAnimation : MonoBehaviour
{
    public AnimationCurve walkVertical;
    public AnimationCurve walkWobble;
    public float walkVerticalTime, walkVerticalTotalTime;
    public GameObject WiggleTarget;
    private Vector3 _oldPosition;
    private Vector3 _originalOffset;

    // Start is called before the first frame update
    void Start()
    {
        _oldPosition = transform.position;
        _originalOffset = WiggleTarget.transform.localPosition;

        walkVerticalTime = Random.Range(0, walkVerticalTotalTime);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Wiggle();
    }

    private void Wiggle()
    {

        Vector3 movement = transform.position - _oldPosition;
        movement = new Vector3(movement.x, movement.y, movement.z);
        _oldPosition = transform.position;

        // Animate walk
        if (movement != Vector3.zero)
        {
            walkVerticalTime += Time.deltaTime;
            if (walkVerticalTime > walkVerticalTotalTime)
            {
                walkVerticalTime -= walkVerticalTotalTime;
            }
        }
        else
        {
            // If we are beyond half way, just continue
            if (walkVerticalTime > walkVerticalTotalTime / 2 && walkVerticalTime < walkVerticalTotalTime)
            {
                walkVerticalTime += Time.deltaTime;
                walkVerticalTime = Mathf.Min(walkVerticalTotalTime, walkVerticalTime);
            }
            else if (walkVerticalTime < walkVerticalTotalTime / 2 && walkVerticalTime > 0)
            {
                walkVerticalTime -= Time.deltaTime;
                walkVerticalTime = Mathf.Max(0, walkVerticalTime);
            }
        }
        float innerVerticalOffet = walkVertical.Evaluate(walkVerticalTime / walkVerticalTotalTime);
        WiggleTarget.transform.localPosition = new Vector3(_originalOffset.x, _originalOffset.y + innerVerticalOffet, _originalOffset.z);

        Vector3 eulerRotation = WiggleTarget.transform.rotation.eulerAngles;
        WiggleTarget.transform.rotation = Quaternion.Euler(new Vector3(eulerRotation.x, eulerRotation.y, walkWobble.Evaluate(walkVerticalTime / walkVerticalTotalTime) * 90));

    }
}