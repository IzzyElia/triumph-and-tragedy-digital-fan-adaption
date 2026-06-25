using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.AnimationSystem;

public class AnimationController
{
    // Animations System
    // Basically, we can queue up animations, which are called one at a time if queued with QueueAnimation(),
    // or simultaneously if added with InstantlyTriggerAnimation
    
    // For more info on how this world, see WorldAnimation.cs
    
    private Queue<AnimationBase> _queuedAnimations = new Queue<AnimationBase>();
    private List<AnimationBase> _ongoingAnimations = new List<AnimationBase>();
    private List<AnimationBase> _animationDestructionQueue = new List<AnimationBase>();
    private Queue<AnimationBase> _finalizeQueue = new Queue<AnimationBase>();

    // Called on _Process()
    /// <summary>
    /// Runs animations by looping through _ongoingAnimations and calling ExecuteStep on each.
    /// If all animations are completed, the next animation is pulled from the queue
    /// </summary>
    public void ProcessAnimations(double delta, int maxAnimationsBeforeForcingNew = -1)
    {
        bool allowNewAnimationToStart = true;
        foreach (var animation in _ongoingAnimations)
        {
            AnimationState animationState;
            
            try
            {
                animation.Run(delta);
                animationState = animation.AnimationState;
                if (animationState == AnimationState.Unset) throw new InvalidOperationException("Animation failed to set its AnimationState");
            }
            catch (Exception e)
            {
                DynamicLogger.LogError($"Error running animation {animation.GetType().Name}\n{e}");
                animationState = AnimationState.Done;
            }
            
            if (animationState == AnimationState.Done)
            {
                _animationDestructionQueue.Add(animation);
            }
            else if (animationState == AnimationState.ContinueExclusive)
            {
                allowNewAnimationToStart = false;
            }
        }

        // Remove any animations flagged for removal above
        foreach (var animation in _animationDestructionQueue)
        {
            _ongoingAnimations.Remove(animation);
        }
        
        _animationDestructionQueue.Clear();
        
        // If all ongoing animations are finished, pull the next one from the queue and start it
        if (maxAnimationsBeforeForcingNew > 0 && _queuedAnimations.Count > maxAnimationsBeforeForcingNew) allowNewAnimationToStart = true;
        if ((allowNewAnimationToStart) && _queuedAnimations.TryDequeue(out AnimationBase nextAnimation))
        {
            _ongoingAnimations.Add(nextAnimation);
            nextAnimation.Run(delta);
        }

        while (_finalizeQueue.TryPeek(out AnimationBase animation) && animation.AnimationState == AnimationState.Done)
        {
            _finalizeQueue.Dequeue();
            animation.Conclude();
        }
    }
    public void QueueAnimation(AnimationBase animation)
    {
        _queuedAnimations.Enqueue(animation);
        _finalizeQueue.Enqueue(animation);
    }

    public void ClearAllAnimations()
    {
        _queuedAnimations.Clear();
        _ongoingAnimations.Clear();
    }

    /// <summary>
    /// Start playing an animation immediately, without waiting for it to come up in the queue
    /// </summary>
    public void InstantlyTriggerAnimation(AnimationBase animation)
    {
        _ongoingAnimations.Add(animation);
        _finalizeQueue.Enqueue(animation);
    }
}