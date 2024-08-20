using UnityEngine;

public class Timer {
    private float duration;
    private float startTime;
    private float pausedElapsed;

    public Timer(){
        pausedElapsed = -1.0f;
        duration = 0.0f;
    }

    public Timer(float duration_){
        pausedElapsed = -1.0f;
        duration = duration_;
        startTime = -duration;
    }

    public void Start(){
        startTime = Time.time;
    }

    public bool IsPaused(){
        return pausedElapsed >= 0.0f;
    }

    public void Pause(){
        if(pausedElapsed < 0.0f){
            pausedElapsed = Elapsed();
        } else {
            Debug.LogError("Timer is already paused");
        }
    }

    public void Unpause(){
        if(IsPaused()){
            startTime = Time.time - pausedElapsed;
            pausedElapsed = -1.0f;
        }
    }

    public float Elapsed(){
        if(IsPaused()){
            return pausedElapsed;
        } else {
            return Time.time - startTime;
        }
    }

    public float Parameterized(){
        return Mathf.Max(Mathf.Min(Elapsed() / duration, 1.0f), 0.0f);
    }

    public float ParameterizedUnclamped(){
        return Elapsed() / duration;
    }

    public float ParameterizedLooping(){
        return ParameterizedUnclamped() % 1.0f;
    }

    public bool Finished(){
        return Elapsed() >= duration && !IsPaused();
    }

    public void SetParameterized(float value){
        startTime = Time.time - (value * duration);
    }

    public void SetDuration(float duration_){
        duration = duration_;
    }

    public float Duration(){
        return duration;
    }
    
    public float Remaining(){
        return Mathf.Max(duration - Elapsed(), 0.0f);
    }
};
