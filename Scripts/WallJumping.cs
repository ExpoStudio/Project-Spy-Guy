using System.Collections;
using UnityEngine;

public class WallJumping : MonoBehaviour
{
    [SerializeField] private PhysicsMaterial2D FrictionMaterial;
    [SerializeField] private Movement2 PlayerMovement;
    public bool CanWallJump;
    public bool walljumpingState;
    private Coroutine WallJumpingRout;
    private Coroutine JumpMovementDisabled;
    [HideInInspector] public bool wallStick;
    private bool Dir;

    public bool JumpInputted;
    public bool wallJumpedAlready;
    public bool ignoreJumpInput;

    private void Update()
    {
        if (ignoreJumpInput) 
        {
            return;
        } else if (!ignoreJumpInput) 
        {
            JumpInputted = PlayerMovement.Jump();
        }
        Dir = PlayerMovement.FacingRight;
        if (PlayerMovement.IsRightWall() && !PlayerMovement.IsGrounded()) 
        {
            if (((PlayerMovement.IsRightWall() && Dir && PlayerMovement.horiz > 0) || (PlayerMovement.IsRightWall() && !Dir && PlayerMovement.horiz < 0)) && PlayerMovement.RigBod.linearVelocity.y <= 0f && !walljumpingState) 
            {
                PlayerMovement.WallCling();
                wallJumpedAlready = false;
                CanWallJump = true;
                if (!wallStick) 
                {
                    if (WallJumpingRout != null) StopCoroutine(WallJumpingRout);
                    WallJumpingRout = StartCoroutine(WallSlideTimer(0.5f));
                }
            } 
            else 
            {
                StopAllRevert();
                CancelWallSlide();
            }
        }
        else if (PlayerMovement.IsGrounded()) 
        {
            StopAllRevert();
            CancelWallSlide();
        }
    }
    private void FixedUpdate()
    {
        
        if (wallStick && CanWallJump)
        {
            PlayerMovement.RigBod.linearVelocity = new Vector2(0,PlayerMovement.RigBod.linearVelocity.y*0.85f);
        }

        //Actual Wall Jump Code
        if (!wallJumpedAlready && JumpInputted && CanWallJump && !PlayerMovement.IsGrounded() && !walljumpingState && PlayerMovement.jumpPressedTimer < 4f) 
        {
            PlayerMovement.canmove = false;
            Dir = !Dir;
            CanWallJump = false;
            wallJumpedAlready = true;
            walljumpingState = true;
            PlayerMovement.canjump = false;
            TriggerWallJump();
        }
    }

    private void TriggerWallJump()
    {
        float Facing = Dir ? 1f : -1f;
        if (PlayerMovement.HoldRun())
        {
            PlayerMovement.RigBod.linearVelocity = new Vector2(0, 0);
            PlayerMovement.RigBod.AddForce(new Vector2(6f*Facing, 7f), ForceMode2D.Impulse);
        }
        else
        {
            PlayerMovement.RigBod.linearVelocity = new Vector2(0, 0);
            PlayerMovement.RigBod.AddForce(new Vector2(0, 7f), ForceMode2D.Impulse);
        }
        if (JumpMovementDisabled != null) StopCoroutine(JumpMovementDisabled);
        JumpMovementDisabled = StartCoroutine(JumpMovementdelay(0.5f));
    }

    IEnumerator JumpMovementdelay(float delay) 
    {
        PlayerMovement.RigBod.gravityScale = 1.2f;
        yield return new WaitForSeconds(0.2f);
        ignoreJumpInput = true;
        PlayerMovement.jumpTime = 20f;
        yield return new WaitForSeconds(delay);
        PlayerMovement.canmove = true;
        walljumpingState = false;
        ignoreJumpInput = false;
        wallJumpedAlready = false;
        PlayerMovement.canjump = true;
        PlayerMovement.RigBod.gravityScale = 2.5f;
        
    }
    public void JumpMovementdelayCancel() 
    {
        if (JumpMovementDisabled != null) 
        {
            StopCoroutine(JumpMovementDisabled);
            JumpMovementDisabled = null;
        }
        PlayerMovement.canmove = true;
    }

    IEnumerator WallSlideTimer(float delay) 
    {
        PlayerMovement.RigBod.linearVelocity = new Vector2(0, 0);
        yield return new WaitForSeconds(delay);
        wallStick = true;
    }
    private void CancelWallSlide() 
    {
        if (WallJumpingRout != null) 
        {
            StopCoroutine(WallJumpingRout);
            WallJumpingRout = null;
        }   
        wallStick = false;
    }

    private void StopAllRevert() 
    {
        CanWallJump = false;
        if (WallJumpingRout != null) {
            StopCoroutine(WallJumpingRout);
            WallJumpingRout = null;
        }
        PlayerMovement.RigBod.gravityScale = 1.2f;
        FrictionMaterial.friction = 0f;
        wallStick = false;
        walljumpingState = false;
    }
}
