
using UnityEngine;

public class MovementEnemy : Movement2
{
    [SerializeField] private BaseEnemyScript EnemyStates;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public override void Update()
    {

    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        if(!IsGrounded())
        {
            RigBod.linearDamping = 0f;
        }
        else if (IsGrounded())
        {
            RigBod.linearDamping = 10f;
        }
    }
}
