using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibeAnimation : MonoBehaviour
{
    [Range(0.01f, 2.0f)]
    public float AnimationTime;

    [Range(0.01f, 1.0f)]
    public float XCurveMultiplier = 0.1f;

    public AnimationCurve XCurve;

    [Range(0.01f, 1.0f)]
    public float YCurveMultiplier = 0.1f;

    public AnimationCurve YCurve;

    private float _t;
    private Vector3 _startScale;

    // Start is called before the first frame update
    void Start()
    {
        _startScale = transform.localScale;
        _t = Random.Range(0, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        _t += Time.deltaTime;

        float pct = _t / AnimationTime;

        float x = (XCurve.Evaluate(pct + 1.0f) * 2.0f) * XCurveMultiplier * _startScale.x + _startScale.x;
        float y = (YCurve.Evaluate(pct) * 2.0f) * YCurveMultiplier * _startScale.x + _startScale.y;
                                               
        transform.localScale = new Vector3(x, y, _startScale.z);
    }

    private void OnDestroy()
    {
        transform.localScale = _startScale;
    }
}
