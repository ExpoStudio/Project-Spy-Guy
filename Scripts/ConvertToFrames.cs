public static class FrameUtils
{
    private const float FixedFrameRateConstant = 60f;

    /// <summary>
    /// Converts the inputted frame number into seconds based on Unity's Time.time. Use for frame-based logic
    /// </summary>
    /// <param name="frames">Input any number in frames</param>
    /// <returns>A float of Time.time seconds</returns>
    public static float FramesToSeconds(float frames)
    {
        return frames / FixedFrameRateConstant;
    }

    /// <summary>
    /// Converts the inputted time number into frames.
    /// </summary>
    /// <param name="time"> Time in time.time seconds</param>
    /// <returns>The equivalent number of frames at a fixed frame rate of 60fps</returns>
    public static float TimeToFrames(float time)
    {
        return time * FixedFrameRateConstant;
    }
}