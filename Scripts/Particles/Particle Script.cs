using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Movement2 PlayerMovement;
    [SerializeField] private WallJumping PlayerWallJumping;
    [SerializeField] ParticleSystem WalkParticle;
    [Range(0,10)]
    [SerializeField] int VelocityMin;
    [Range(0,0.2f)]
    [SerializeField] float rate;

    [SerializeField] ParticleSystem WallSlide;
    
    float counter;
    // Update is called once per frame
    private void Update()
    {
        counter += Time.deltaTime;
        if (Mathf.Abs(PlayerMovement.RigBod.linearVelocity.x) > VelocityMin && PlayerMovement.IsGrounded()) 
        {
            if (counter > rate)
            {
                WalkParticle.Play();
                counter = 0;
            }
        }
    }
}
