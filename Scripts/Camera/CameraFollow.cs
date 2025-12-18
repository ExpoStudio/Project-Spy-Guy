using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera cam;
    public CameraShake CameraShake;
    public Transform PlayerTransform;
    public Movement2 PlayerMovement;
    [SerializeField] private float OriginalOrthoSize = 5f;


    [HideInInspector] public bool Crit { get; private set; } = false;
    [HideInInspector] public bool isZooming;
    private float targetSpeed;
    private bool returnZoom;
    private float targetOrthoSize;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleShaking();
    }

    private void HandleShaking()
    {
        if (isZooming)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, targetSpeed * Time.unscaledDeltaTime);
            if (Math.Abs(cam.orthographicSize - targetOrthoSize) < 0.01f)
            {
                isZooming = false;
            }
        }


        if (returnZoom)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, OriginalOrthoSize, 2f * Time.deltaTime);
            if (Math.Abs(cam.orthographicSize - OriginalOrthoSize) < 0.01f)
            {
                cam.orthographicSize = OriginalOrthoSize;
                returnZoom = false;
            }
        }
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        CameraShaking();
    }
    private void LateUpdate()
    {
        if (PlayerTransform != null && PlayerMovement != null)
        {
            Vector3 targetPosition = new(PlayerTransform.position.x, PlayerTransform.position.y+3f, -10f);
            transform.position = Vector3.Lerp(transform.position, targetPosition, 12f * Time.deltaTime);
        }
    }
    private void CameraShaking()
    {
        if ((PlayerMovement.runningState && PlayerMovement != null && PlayerMovement.IsGrounded() && PlayerMovement.horiz != 0 && PlayerMovement.RigBod.linearVelocity.x > 4f && !PlayerMovement.turning) || Crit)
        {
            if (Crit) { CameraShake.TriggerUnscaledShake(1.2f, 0.01f, 0.2f, 7f); }
            else
            {
                if (PlayerMovement.RigBod.linearVelocity.magnitude >= 13f && PlayerMovement.runningState)
                {
                    CameraShake.TriggerShake(1.2f, 0.0001f, 3f, 2.5f);
                }

                if (cam.orthographicSize < 5)
                {
                    cam.orthographicSize += .01f;
                }
            }
        }
        else if (!PlayerMovement.runningState && PlayerMovement != null)
        {
            if (cam.orthographicSize >= 5)
            {
                cam.orthographicSize -= .01f;
            }
        }
    }

    public void Zoom(float speed, float OrthoSize, float durationSecs)
    {
        isZooming = true;

        targetSpeed = speed;
        targetOrthoSize = OrthoSize;


        OrthoSize = Mathf.Min(OrthoSize, speed * Time.unscaledDeltaTime);
        OriginalOrthoSize = cam.orthographicSize;
        Crit = true;
        Invoke(nameof(ZoomUndo), durationSecs);
    }
    public void ZoomUndo()
    {
        isZooming = false;
        returnZoom = true;
        Crit = false;
    }
}
