using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;


public class DissolveEffect : MonoBehaviour
{
    public Material DissolveMaterial;
    public delegate void DissolveComplete(DissolveEffect dissolveEffect);

    [Range(0, 5.0f)]
    public float Duration = 0.5f;
    public float DissolveAmount;

    public bool IsDissolving;

    public DissolveComplete CompletionHandler;

    private SpriteRenderer _sr;

    private float _t;

    // Start is called before the first frame update
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.material = DissolveMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDissolving)
        {
            _t += Time.deltaTime;
            float pct = _t / Duration;
            _sr.material.SetFloat("_DissolveAmount", pct);

            if (pct > 1.0f)
            {
                IsDissolving = false;
                if (CompletionHandler != null)
                {
                    CompletionHandler(this);
                }
                enabled = false;
            }
        }
    }
}
