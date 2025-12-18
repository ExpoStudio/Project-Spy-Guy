using UnityEngine;
public enum EnemyState 
    {
        Idle,
        Stunned,
        Chasing,
        Attacking,
        Die
    }

public class BaseEnemyScript
{
    
    public bool canmove;
    public bool IsFacingRight;

}


