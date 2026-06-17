namespace TT2026.libraries.Izzy.AnimationSystem;

public abstract class AnimationBase
{
    protected AnimationBase(double animationTime)
    {
        this.AnimationTime = animationTime;
    }

    /// <summary>
    /// Set to true when the animation starts playing
    /// </summary>
    public bool Started { get; private set; }
    /// <summary>
    /// This is the starting time, in real time, that the animation started
    /// </summary>
    protected double StartTime { get; private set; }
    /// <summary>
    /// This is the total time that the animation should play for (in seconds)
    /// </summary>
    protected double AnimationTime;
    /// <summary>
    /// The animations progress as calculated by Elapsed Time (since start) / (the total) Animation Time
    /// </summary>
    protected float AnimationProgress => Started ? (float)(ElapsedTime / AnimationTime) : 0;

    public AnimationState AnimationState = AnimationState.Unset;
    protected double ElapsedTime;
    protected double DeltaTime;
    protected abstract void Initialize();
    public abstract void Conclude();

    public void Run(double delta)
    {
        if (!Started)
        {
            Initialize();
            Started = true;
        }
        
        ElapsedTime += delta;
        DeltaTime = delta;
        Continue();
    }
    protected abstract void Continue();
    
    /// <summary>
    /// The sigmoid function, modified to accept a 0-1 input range.
    /// </summary>
    protected static float Sigmoid01(float x, float k)
    {
        float normalizedX = x * 2 - 1; // Normalize input from 0-1 to -1-1
        return 1 / (1 + Mathfi.Exp(-k * normalizedX));
    }
}