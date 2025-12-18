using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveCollider : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Collider2D ReviveTrigger;
    [SerializeField] private Health DownedPlayerHealth;
    [SerializeField] private Movement2 DownedPlayerMovement;
    [SerializeField] private float _actionTimer = 0f;
    public float ActionTimer { get => _actionTimer; set => _actionTimer = Mathf.Clamp(value, 0f, 10f); }
    void OnTriggerStay2D(Collider2D inRange)
    {
        if (inRange.CompareTag("Player"))
        {
            GameObject inRangeObject = inRange.gameObject;
            Movement2 inRangeObjectMovement = inRangeObject.GetComponent<Movement2>();
            Debug.Log($"Inrange: {inRange}, InRangeObject {inRangeObject}");
            if (inRangeObjectMovement.TargetFirstRevivePlayer == null) inRangeObjectMovement.TargetFirstRevivePlayer = DownedPlayerMovement;
            else return;
            if (inRangeObjectMovement == null)
                return;
            if (inRangeObjectMovement.Parry())
            {
                Debug.Log("Revive Trying and things this is working!!!");
            }
        }
    }
    void OnTriggerExit2D(Collider2D outOfRange)
    {
        GameObject outOfRangeObject = outOfRange.gameObject;
        Movement2 outOfRangeObjectMovement = outOfRangeObject.GetComponent<Movement2>();
        outOfRangeObjectMovement.TargetFirstRevivePlayer = null;
    }
}
