using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Entity;

public class BasicAttackBehavior : ActionBehavior
{
    public bool IsMeleeAttack = true;

    [Range(0, 20)]
    public int DamageAmount = 2;

    [Range(0.1f, 20.0f)]
    public float Speed = 11.5f;

    public override bool CanExecute(Action.ExecutionContext context)
    {
        return context.source != null && context.target != null && context.target.Value.Entity != null;
    }

    private Action.ExecutionContext _executionContext;
    private Entity _targetEntity;

    void PlayAttackAudio()
    {
        AudioManager.Instance.Play("SFX/GenericAttack",
            pitchMin: 0.9f, pitchMax: 1.1f,
            volumeMin: 0.7f, volumeMax: 0.7f,
            position: transform.position,
            minDistance: 10, maxDistance: 20);
    }

    public override void Execute(Action.ExecutionContext context)
    {
        Debug.Log("Executing " + context);

        _executionContext = context;

        context.source.SetBusy(true);
        var startPosition = context.source.Position;
        Vector2Int targetPosition;
        var tileData = context.target;

        if (tileData != null)
        {
            targetPosition = tileData.Value.Position;
            _targetEntity = tileData.Value.Entity;
        }
        else
        {
            context.source.SetBusy(false);
            Debug.LogError("Invalid configuration for MoveBehavior - missing target");
            return;
        }

        PlayAttackAudio();
        context.source.PlayMeleeAttackAnimation(targetPosition, speedMPS: Speed, zenithHandler: AttackAnimationZenithCompleted, completionHandler: AttackAnimationCompleted);
    }

    private void AttackAnimationZenithCompleted(Entity e)
    {
        Debug.Log("Zenith!");

        _targetEntity.ApplyDamage(DamageAmount);
    }

    private void AttackAnimationCompleted(Entity e)
    {
        Debug.Log("Complete!");
        e.SetBusy(false);
    }
}
