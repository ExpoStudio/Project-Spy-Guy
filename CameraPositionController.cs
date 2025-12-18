using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public enum CameraMovementState
{
    NONE,
    FOLLOW_PLAYERS_VERTICAL_ONLY,
    FOLLOW_PLAYERS_HORIZONTAL_ONLY,
    FOLLOW_PLAYERS,
    STATIC,
    SLOWLY_ZOOM
}
// 
public enum CameraPlayerPriority
{
    NONE,
    PLAYER_1,
    PLAYER_2,
    PLAYER_3,
    PLAYER_4
}

// This script controls the camera position in a 2D game, allowing it to follow players or remain static.
// It uses a singleton pattern to ensure only one instance exists and can be accessed globally.
public class CameraPositionController : MonoBehaviour
{
    // Make the camera static
    // Control the camera position through the use of colliders/manual movements when in the static state (i.e. when a player moves to a certain location)

    [Header("Camera Settings")]
    public CameraMovementState cameraMovementState = CameraMovementState.FOLLOW_PLAYERS_HORIZONTAL_ONLY;
    public event Action OnPlayerAdded;
    public event Action AverageDistanceTooFar;

    /// <summary>
    /// Dictionary to hold all player inputs. Loop through to refer to all players that are in game
    /// The key is the player index, and the value is the PlayerInput component.
    /// </summary>
    public Dictionary<int, PlayerInput> playerInputs = new();

    /// <summary>
    /// List of players currently in game, alive AND not out of bounds.
    /// This is used to determine the average position of players and to follow them. In some cases, camera effects will be ignored for players out of bounds.
    /// </summary>
    public List<Movement2> players;
    public static CameraPositionController Instance { get; private set; }
    // Create a singleton instance of the CameraPositionController in do not destroy on load to transfer between scenes

    public event Action<Movement2> OnOutOfBounds;


    [SerializeField] private float CameraFollowSpeed = 30f; // Speed at which the camera follows players
    [SerializeField] private float CameraZoomSpeed = 30f; // Rate at which the camera zooms in or out
    [SerializeField] private float cameraZoom = 5f; // Default zoom level of the camera
    [SerializeField] private float cameraZoomMin = 2f; // Minimum zoom level
    public float maxCameraFollowDistance = 15f; // Maximum camera follow distance before becoming fixed
    [SerializeField] private float minCameraFollowDistance = 1f; // Minimum camera follow distance before following average position

    [SerializeField] private float averageDistanceTooFar = 15f; // Distance at which the camera will start following the prioritized player
    [SerializeField] private bool playerOnePrioritySet = false;
    public CameraPlayerPriority PrioritizedPlayer = CameraPlayerPriority.NONE;
    [SerializeField] private bool followUntilInRange = false;
    [SerializeField] private float actionTimer = 0f;
    private float ActionTimer { get => actionTimer; set => actionTimer = Mathf.Clamp(value, 0, 30f); }


    public PlayerInput PrioritizedPlayerInput => playerInputs.TryGetValue((int)PrioritizedPlayer, out var playerInput) ? playerInput : null;
    // Track the Movement2 component for the prioritized player as well
    public Movement2 PrioritizedPlayerMovement;
    void Awake()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += OnPlayerLeft;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        players = new List<Movement2>();
        
        foreach(var input in PlayerInput.all)
        {
            OnPlayerJoined(input);
        }

        AverageDistanceTooFar += () =>
        {
            // Handle the case when the average distance between players is too far
            Debug.Log("Average distance between players is too far.");
            followUntilInRange = true;
            wasOutOfBounds = false; // Set the out of bounds flag to false
            ActionTimer = 5f; // Reset the action timer to 30 seconds
        };

        OnOutOfBounds += (movement) =>
        {
            // Remove the player from the list of players in bounds
            if (players.Contains(movement))
            {
                _ = players.Remove(movement);
            }

            //FactorInWeights
        };
    }



    void OnPlayerJoined(PlayerInput playerInput)
    {
        // Add the new player input to the dictionary
        if (!playerInputs.ContainsKey(playerInput.playerIndex + 1))
        {
            playerInputs.Add(playerInput.playerIndex + 1, playerInput);
            OnPlayerAdded?.Invoke();
        }
        // Set the player priority if it's the first player
        if (!playerOnePrioritySet)
        {
            playerInput.SetPlayerPriority();
            playerOnePrioritySet = true;
        }
        else
        {
            // Ensure prioritized movement reference stays up to date if this join affects it
            if (PrioritizedPlayerInput != null)
            {
                PrioritizedPlayerMovement = PrioritizedPlayerInput.GetComponentInChildren<Movement2>();
            }
        }
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        int key = playerInput.playerIndex + 1;
        // remove from dictionary
        if (playerInputs.ContainsKey(key)) _ = playerInputs.Remove(key);

        // remove movement entry if present
        var movement = playerInput.GetComponentInChildren<Movement2>();
        if (movement != null && players.Contains(movement)) _ = players.Remove(movement);

        // reset first-player flag if no players remain
        if (playerInputs.Count == 0) playerOnePrioritySet = false;

        // if the left player was the prioritized one, choose a new prioritized player or clear
        if ((int)PrioritizedPlayer == key)
        {
            var first = FindFirstInBoundsPlayerInput;
            if (first != null)
                first.SetPlayerPriority();
            else
            {
                PrioritizedPlayer = CameraPlayerPriority.NONE;
                PrioritizedPlayerMovement = null;
            }
        }

        UpdatePlayerPriorities();
    }

    // Update is called once per frame
    void Update()
    {
        if (ActionTimer > 0) ActionTimer -= Time.deltaTime;
        if (ActionTimer <= 0 && followUntilInRange)
        {
            followUntilInRange = false;
        }
        // Update the player inputs list
        UpdatePlayerPriorities();
        UpdatePlayerInputs();
    }

    private bool wasOutOfBounds = false;
    void LateUpdate()
    {
        // If dictionary is empty, return
        if (playerInputs.Count == 0) return;

        UpdateCameraPosition();
    }

    /// <summary>
    /// Updates player priorities by prioritizing the first player in bounds (in order).
    /// The prioritized player will only be followed if the distance between players is too far.
    /// </summary>
    private void UpdatePlayerPriorities()
    {
        // If there are no players in bounds OR if the game state is not in game or lobby, reset the priority
        if (players.Count == 0 || !GameStateManager.Instance.InGame)
        {
            PrioritizedPlayer = CameraPlayerPriority.NONE;
            PrioritizedPlayerMovement = null;
            followUntilInRange = false;
            return;
        }

        // If the prioritized player is out of bounds, find the first in-bounds player
        if (PrioritizedPlayer == CameraPlayerPriority.NONE || (PrioritizedPlayerMovement != null && PrioritizedPlayerMovement.OutOfBounds))
        {
            var firstInBoundsPlayer = FindFirstInBoundsPlayerInput;
            if (firstInBoundsPlayer != null)
            {
                firstInBoundsPlayer.SetPlayerPriority();

            }
            else
            {
                PrioritizedPlayer = CameraPlayerPriority.NONE;
                PrioritizedPlayerMovement = null;
            }
        }
        else
        {
            // Keep movement reference in sync if the prioritized input is present but movement ref is null
            if (PrioritizedPlayerMovement == null && PrioritizedPlayerInput != null)
            {
                PrioritizedPlayerMovement = PrioritizedPlayerInput.GetComponentInChildren<Movement2>();
            }
        }
    }
    private void UpdateCameraPosition()
    {
        switch (cameraMovementState)
        {
            case CameraMovementState.FOLLOW_PLAYERS_HORIZONTAL_ONLY:
                {
                    float averagePosition = 0f;
                    int count = 0;
                    float maxTrackedPosition = float.MinValue;
                    float minTrackedPosition = float.MaxValue;
                    foreach (var player in players)
                    {
                        var PTransform = player.transform;
                        if (!player.OutOfBounds)
                        {
                            minTrackedPosition = Mathf.Min(minTrackedPosition, PTransform.position.x);
                            maxTrackedPosition = Mathf.Max(maxTrackedPosition, PTransform.position.x);
                            count++;
                        }
                    }
                    if (count == 0) return; // No players in bounds, exit early
                    float distance = Mathf.Abs(maxTrackedPosition - minTrackedPosition);
                    if (!wasOutOfBounds) averagePosition = players.Average(player => player.transform.position.x);
                    else if (wasOutOfBounds) averagePosition = transform.position.x;

                    if (followUntilInRange)
                    {
                        // Prefer the Movement2 position if available, otherwise fall back to input child lookup
                        float targetX = transform.position.x;
                        if (PrioritizedPlayerMovement != null)
                            targetX = PrioritizedPlayerMovement.transform.position.x;
                        else if (PrioritizedPlayerInput != null)
                            targetX = PrioritizedPlayerInput.transform.GetChild(0).position.x;

                        transform.position = transform.position.FrameRateIndependentLerp(new Vector3(targetX, transform.position.y, transform.position.z), CameraFollowSpeed, Time.deltaTime);
                    }
                    else
                    {
                        if (ActionTimer > 0f) return;
                        if (players.Count == 1)
                        {
                            // If there's only one player, set the camera to follow that player
                            if (wasOutOfBounds) wasOutOfBounds = false; // Reset the out of bounds flag
                            transform.position = transform.position.FrameRateIndependentLerp(new Vector3(players[0].transform.position.x, transform.position.y, transform.position.z), 30f, Time.deltaTime);
                            PrioritizedPlayer = CameraPlayerPriority.NONE; // Reset priority since we are following a single player
                            PrioritizedPlayerMovement = null;
                        }
                        else if (players.Count > 1)
                        {
                            if (distance > minCameraFollowDistance && distance < maxCameraFollowDistance && !followUntilInRange)
                            {
                                // If the distance between players is within the acceptable range, follow the average position
                                if (wasOutOfBounds)
                                {
                                    wasOutOfBounds = false;
                                    return;
                                }
                                transform.position = transform.position.FrameRateIndependentLerp(new Vector3(averagePosition, transform.position.y, transform.position.z), 10f, Time.deltaTime);
                            }
                            if (distance >= maxCameraFollowDistance && distance <= averageDistanceTooFar && !followUntilInRange)
                            {
                                wasOutOfBounds = true; // Set the out of bounds flag
                            }
                            if (distance > averageDistanceTooFar && !followUntilInRange)
                            {
                                // Set the camera to move towards the prioritized player
                                AverageDistanceTooFar?.Invoke();
                            }
                        }
                    }
                    break;
                }

            case CameraMovementState.FOLLOW_PLAYERS_VERTICAL_ONLY:
                break;
            case CameraMovementState.STATIC:
                break;
            case CameraMovementState.SLOWLY_ZOOM:
                break;
        }
    }
    private void UpdatePlayerInputs()
    {
        players.Clear();
        foreach (var input in playerInputs.Values)
        {
            var movement = input.GetComponentInChildren<Movement2>();
            var alive = input.GetComponentInChildren<Health>();
            if (movement != null && !movement.OutOfBounds && alive != null && alive.playerDownedState == PlayerDownedState.ALIVE)
                players.Add(movement);
        }

        // Ensure prioritized movement is up to date with current inputs
        if (PrioritizedPlayerInput != null)
            PrioritizedPlayerMovement = PrioritizedPlayerInput.GetComponentInChildren<Movement2>();
        else
            PrioritizedPlayerMovement = null;
    }


    public static void NotifyOutOfBounds(Movement2 movement)
    {
        // Notify subscribers that a player is out of bounds
        Instance.OnOutOfBounds?.Invoke(movement);
    }
private PlayerInput FindFirstInBoundsPlayerInput =>
    playerInputs.Values.FirstOrDefault(player =>
    {
        var movement = player.GetComponentInChildren<Movement2>();
        return movement != null && !movement.OutOfBounds;
    });}

public static class MethodExtentions
{
    public static void SetPlayerPriority(this PlayerInput playerIndex)
    {
        if (playerIndex == null)
        {
            CameraPositionController.Instance.PrioritizedPlayer = CameraPlayerPriority.NONE;
            CameraPositionController.Instance.PrioritizedPlayerMovement = null;
            return;
        }

        int index = playerIndex.playerIndex + 1; // Adjust for 0-based index
        CameraPositionController.Instance.PrioritizedPlayer = index switch
        {
            1 => CameraPlayerPriority.PLAYER_1,
            2 => CameraPlayerPriority.PLAYER_2,
            3 => CameraPlayerPriority.PLAYER_3,
            4 => CameraPlayerPriority.PLAYER_4,
            _ => CameraPlayerPriority.NONE,
        };

        // Also set the Movement2 reference for the prioritized player
        CameraPositionController.Instance.PrioritizedPlayerMovement = playerIndex.GetComponentInChildren<Movement2>();
        if (CameraPositionController.Instance.PrioritizedPlayer == CameraPlayerPriority.NONE)
            CameraPositionController.Instance.PrioritizedPlayerMovement = null;
    }
    /// <summary>
    /// Frame rate independent Lerp function for Vector3.
    /// This function allows smooth transitions between two Vector3 positions based on a speed parameter.
    /// Allows for true frame rate independent movement by calculating the lerp factor based on the speed and delta time with exponential decay.
    /// </summary>
    /// <param name="current">Current Vector</param>
    /// <param name="target">Target Vector</param>
    /// <param name="speed">Speed of the lerp</param>
    /// <param name="deltaTime">Time elapsed since the last frame</param>
    /// <returns>Vector3 Frame Rate Independent Lerp</returns>
    /// <example>
    /// Example usage:
    /// <code>
    /// Vector3 currentPosition = transform.position;
    /// Vector3 targetPosition = new Vector3(10, 0, 0);
    /// float speed = 5f;
    /// float deltaTime = Time.deltaTime;
    /// transform.position = currentPosition.FrameRateIndependentLerp(targetPosition, speed, deltaTime);
    /// </code>
    /// </example>
    public static Vector3 FrameRateIndependentLerp(this Vector3 current, Vector3 target, float speed, float deltaTime)
    {
        // Calculate the step size based on the frame rate and speed;
        float decay = Mathf.Exp(-speed * deltaTime);
        float lerpFactor = 1.0f - decay;
        // Return the new position using Vector3.Lerp
        return Vector3.Lerp(current, target, lerpFactor);
    }

    public static Vector2 FindPowerVectorFromDistanceOverflow(this Vector2 FullLaunchVector)
    {
        Vector2 NormalizedVector = FullLaunchVector.normalized;
        float OldMagnitude = FullLaunchVector.magnitude;
        Vector2 CorrectedVector = NormalizedVector * (OldMagnitude - CameraPositionController.Instance.maxCameraFollowDistance / 2);
        return CorrectedVector;
    }

    public static float FindAngleFromVector2(this Vector2 vector)
    {
        // Calculate the angle in radians and convert to degrees
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }
}
