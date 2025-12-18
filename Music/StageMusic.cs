using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class StageMusic : MonoBehaviour
{
    public AudioSource baseMainMusicSource;
    public AudioSource musicLayer1Source;
    public AudioSource musicLayer2Source;
    public AudioSource lowHealthLayer1Source;
    public AudioSource lowHealthLayer2Source;
    public AudioSource bossMusicSource;
    public AudioSource bossMusicLayerCriticalSource;
    public AudioSource musicShieldLayerSource;

    //Gather all the audio sources in the scene
    public List<AudioSource> MainMusicSources;
    public List<AudioSource> BossMusicSources;


    public AudioClip baseMainMusic;
    public AudioClip musicLayer1;
    public AudioClip musicLayer2;
    public AudioClip lowHealthLayer1;
    public AudioClip lowHealthLayer2;
    public AudioClip musicShieldLayer;
    public AudioClip bossMusic;
    public AudioClip bossMusicLayerCritical;
    public AudioSource length;

    public bool bossMusicPlaying = false;

    private const float LerpMultiplier = 2f;

    //loop mechanics
    private Coroutine LoopMechanicsCoroutine;

    private float idleTimer = 0f; // Timer for idle state
    private float slowSpeedTimer = 0f; // Timer for transitioning from running to walking
    private const float idleThreshold = 3f; // Time in seconds before fading out layers when idle
    private const float slowSpeedThreshold = 2f; // Time in seconds before fading out layers when slowing down

    //Use this to gather the number of alive players and the prioritized player in the scene for averages
    //All players count as +1 wieght. Prioritized player counts as +2.
    //If for example there are 4 alive players, and the two of them are running (but not the prioritized)

    //Both the weight system and Average player velocity system works in single player. Having player weights would reduce boiler plate code and would work for things like shielding. 
    //Manual overrides will need to be considered, such as low health, cannot be ignored
    private CameraPositionController Camera;
    private GameStateManager gameStateManager;
    private int AlivePlayerCount => gameStateManager.AlivePlayerCount;
    private enum MusicState
    {
        IDLE,
        WALKING,
        RUNNING,
        LOW_HEALTH,
        SHIELDING,
        BOSS_MUSIC,
        BOSS_MUSIC_CRITICAL,
        QUIET
    }
    private MusicState musicState = MusicState.IDLE;
    private MusicState previousMusicState = MusicState.IDLE;
    void Start()
    {
        Camera = CameraPositionController.Instance;
        gameStateManager = GameStateManager.Instance;

        // Assign the audio clips to the corresponding audio sources
        baseMainMusicSource.clip = baseMainMusic;
        musicLayer1Source.clip = musicLayer1;
        musicLayer2Source.clip = musicLayer2;
        lowHealthLayer1Source.clip = lowHealthLayer1;
        lowHealthLayer2Source.clip = lowHealthLayer2;
        musicShieldLayerSource.clip = musicShieldLayer;

        MainMusicSources = new List<AudioSource>();
        BossMusicSources = new List<AudioSource>();
        //Assigns the regular Stage Music and the Boss Music Sources to arrays-
        AudioSource[] sources = GetComponents<AudioSource>();
        double StartTime = AudioSettings.dspTime;

        foreach (AudioSource source in sources)
        {
            AudioClip clip = source.clip;
            if (clip == null)
                continue;
            // Skip if the clip is null

            if (clip == bossMusic || clip == bossMusicLayerCritical)
            {
                BossMusicSources.Add(source);
            }
            else
            {
                MainMusicSources.Add(source);
            }
        }

        double Length = MainMusicSources.Count > 0
            ? MainMusicSources[0].clip.length
            : 0;

        if (MainMusicSources.Count == 0 || Length <= 0)
        {
            Debug.LogWarning("No main music sources found or clip length invalid.");
            return;
        }

        //play music
        foreach (AudioSource source in MainMusicSources)
        {
            source.PlayScheduled(StartTime);
        }
        //Loop Music
        if (LoopMechanicsCoroutine != null)
        {
            StopCoroutine(LoopMechanicsCoroutine);
        }
        LoopMechanicsCoroutine = StartCoroutine(ScheduleNextLoop(StartTime, Length, MainMusicSources));

        // Set the volume of the music layers to 0
        baseMainMusicSource.volume = 1;
        musicLayer1Source.volume = 0;
        musicLayer2Source.volume = 0;
        lowHealthLayer1Source.volume = 0;
        lowHealthLayer2Source.volume = 0;
        musicShieldLayerSource.volume = 0;
    }




    private bool hysteresisActive = false;
    private float hysteresisTimer = 0f;
    private readonly float hysteresisThreshold = 2f;
    private MusicState desiredMusicState = MusicState.IDLE;

    void Update()
    {
        //Control Music State Based on weighed decision
        PlayerWeightSummary weighedSnapshot = GetWeightSnapshot();
        if (musicState is not (MusicState.BOSS_MUSIC or MusicState.BOSS_MUSIC_CRITICAL)) MovementStageMusic(weighedSnapshot);

        /*if (hysteresisActive)
        {
            hysteresisTimer += Time.deltaTime;
            if (hysteresisTimer >= hysteresisThreshold)
            {
                previousMusicState = musicState;
                musicState = desiredMusicState;
                hysteresisActive = false;
                hysteresisTimer = 0;
            }
        }
        */

        // Actual Volume Controls here
        switch (musicState)
        {
            case MusicState.IDLE:
                IdleLayerVolumeControls();
                break;
            case MusicState.WALKING:
                WalkingLayerVolumeControls();
                break;
            case MusicState.RUNNING:
                RunningLayerVolumeControls();
                break;
            case MusicState.SHIELDING:
                ShieldLayerVolumeControls();
                break;
            case MusicState.LOW_HEALTH:
                LowHealthStateMusic(weighedSnapshot);
                break;
            case MusicState.QUIET:
                LowerLowHealthMusic();
                LowerMusic();
                break;
        }
    }

    void MusicStateChange(MusicState toState)
    {
        //if (musicState == toState) return;

        musicState = toState;
        //hysteresisActive = true;
        //hysteresisTimer = 0f;
    }

    // Bool Auto Fields
    private int ActionableWeight => gameStateManager.AlivePlayers.Count;

    public record PlayerWeightSummary(int WalkingCount, int RunningCount, int ShieldingCount, int StrongCount, int LowHealthCount, Movement2 PrioritizedMovement, bool zoomHit = false);
    private PlayerWeightSummary GetWeightSnapshot()
    {
        if (Camera == null) return new PlayerWeightSummary(0, 0, 0, 0, 0, null);

        var prioritized = Camera.PrioritizedPlayerMovement;
        int walking = 0, running = 0, shielding = 0, lowHealth = 0, strong = 0;
        
        foreach (var p in Camera.players)
        {
            if (p == null) continue;

            if (p.OutOfBounds) continue;
            if (p.playerHealth == null) continue;

            float speed = p.RigBod.linearVelocity.magnitude;
            bool isStrong = p.attacks.strongPhase == StrongPhase.releaseReady;
            float health = p.playerHealth.health;
            bool isBlocking = p.playerHealth.guardUp;
            int weight = p == prioritized ? 2 : 1;

            walking += speed is >= 5f and <= 14f ? weight : 0;
            running += speed is > 14f ? weight : 0;
            shielding += isBlocking ? weight : 0;
            strong += isStrong ? weight : 0;
            lowHealth += health <= 20f ? weight : 0;

            if (p.attacks.zoomBoxHit)
            {
                musicState = MusicState.QUIET;
            }
        }

        return new PlayerWeightSummary(walking, running, shielding, strong, lowHealth, prioritized);
    }

    private void LowerBossMusic()
    {
        bossMusicLayerCriticalSource.volume = Mathf.Lerp(bossMusicLayerCriticalSource.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
        bossMusicSource.volume = Mathf.Lerp(bossMusicSource.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
    }

    private void LowerMusic()
    {
        musicLayer1Source.volume = Mathf.Lerp(musicLayer1Source.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
        musicLayer2Source.volume = Mathf.Lerp(musicLayer2Source.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
        baseMainMusicSource.volume = Mathf.Lerp(baseMainMusicSource.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
    }


    private void LowerLowHealthMusic()
    {
        lowHealthLayer1Source.volume = Mathf.Lerp(lowHealthLayer1Source.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
        lowHealthLayer2Source.volume = Mathf.Lerp(lowHealthLayer2Source.volume, 0, LerpMultiplier * Time.unscaledDeltaTime);
    }

    private void LowHealthStateMusic(PlayerWeightSummary weightSummary)
    {
        LowerMusic();
        if (weightSummary.RunningCount > ActionableWeight || weightSummary.StrongCount > ActionableWeight)
        {
            idleTimer = 0f;
            slowSpeedTimer = 0f;

            lowHealthLayer2Source.volume = Mathf.Lerp(lowHealthLayer2Source.volume, 1, LerpMultiplier * Time.deltaTime);
        }
        else
        {
            slowSpeedTimer += Time.deltaTime;
            if (idleTimer > idleThreshold)
                lowHealthLayer2Source.volume = Mathf.Lerp(lowHealthLayer2Source.volume, 0, LerpMultiplier * Time.deltaTime);
        }
        lowHealthLayer1Source.volume = Mathf.Lerp(lowHealthLayer1Source.volume, 1, LerpMultiplier * Time.deltaTime);
    }

    private void MovementStageMusic(PlayerWeightSummary weightSummary)
    {
        if (weightSummary.LowHealthCount > 0)
        {
            MusicStateChange(MusicState.LOW_HEALTH);
            return;
        }

        // Check if the player is running or performing a strong attack
        if (weightSummary.RunningCount > ActionableWeight || weightSummary.StrongCount > ActionableWeight)
        {
            // Reset timers when the player is running or attacking
            idleTimer = 0f;
            slowSpeedTimer = 0f;

            // Increase the volume of the higher layers
            MusicStateChange(MusicState.RUNNING);
        }
        else if (weightSummary.ShieldingCount > ActionableWeight)
        {
            idleTimer = 0f;
            slowSpeedTimer = 0f;
            MusicStateChange(MusicState.SHIELDING);
        }
        else if (weightSummary.WalkingCount > ActionableWeight) // Mid-speed movement or walking
        {
            // Reset idle timer but increment slow speed timer
            idleTimer = 0f;
            slowSpeedTimer += Time.deltaTime;

            MusicStateChange(MusicState.WALKING);
        }
        else // Player is idle or moving very slowly
        {
            // Increment the idle timer
            idleTimer += Time.deltaTime;

            // Fade out the layers only after the idle timer exceeds the threshold
            if (idleTimer >= idleThreshold)
            {
                MusicStateChange(MusicState.IDLE);
                LowerShieldMusicVolume();
            }
        }
    }

    private void IdleLayerVolumeControls()
    {
        musicLayer2Source.volume = Mathf.Lerp(musicLayer2Source.volume, 0, LerpMultiplier * Time.deltaTime);
        musicLayer1Source.volume = Mathf.Lerp(musicLayer1Source.volume, 0, LerpMultiplier * Time.deltaTime);
        baseMainMusicSource.volume = Mathf.Lerp(baseMainMusicSource.volume, 1f, LerpMultiplier * Time.deltaTime);
    }

    private void ShieldLayerVolumeControls()
    {
        LowerMusic();
        musicShieldLayerSource.volume = Mathf.Lerp(musicShieldLayerSource.volume, 1f, LerpMultiplier * Time.deltaTime);
    }
    private void LowerShieldMusicVolume()
    {
        musicShieldLayerSource.volume = Mathf.Lerp(musicShieldLayerSource.volume, 0f, LerpMultiplier * Time.deltaTime);
    }

    private void WalkingLayerVolumeControls()
    {
        // Only fade out the higher layers after the slow speed timer exceeds the threshold
        if (slowSpeedTimer >= slowSpeedThreshold)
        {
            musicLayer2Source.volume = Mathf.Lerp(musicLayer2Source.volume, 0, LerpMultiplier * Time.deltaTime);
        }

        // Keep the mid-layer active
        musicLayer1Source.volume = Mathf.Lerp(musicLayer1Source.volume, 0.8f, LerpMultiplier * Time.deltaTime);
        baseMainMusicSource.volume = Mathf.Lerp(baseMainMusicSource.volume, 0.85f, LerpMultiplier * Time.deltaTime);
    }

    private void RunningLayerVolumeControls()
    {
        musicLayer2Source.volume = Mathf.Lerp(musicLayer2Source.volume, 1, LerpMultiplier * Time.deltaTime);
        musicLayer1Source.volume = Mathf.Lerp(musicLayer1Source.volume, 0.8f, LerpMultiplier * Time.deltaTime);
        baseMainMusicSource.volume = Mathf.Lerp(baseMainMusicSource.volume, 0.85f, LerpMultiplier * Time.deltaTime);
    }

    private IEnumerator ScheduleNextLoop(double StartTime, double Length, List<AudioSource> MainMusicSources)
    {
        while(true)
        {
            yield return new WaitForSeconds((float)Length);

            // Schedule the next loop
            StartTime += Length;

            foreach (AudioSource source in MainMusicSources)
            {
                source.PlayScheduled(StartTime);
            }
        }
    }
}
