using UnityEngine;

class CustomMath {
    public static float StepToTarget(float current, float target, float stepChange){
        float currentToTarget = target - current;
        float sign = currentToTarget > 0.0f ? 1.0f : -1.0f;

        if(Mathf.Abs(currentToTarget) <= stepChange){
            return target;
        } else {
            return current + (sign * stepChange);
        }
    }

    public static int StepToTarget(int current, int target, int stepChange){
        int currentToTarget = target - current;
        int sign = currentToTarget > 0 ? 1 : -1;

        if(Mathf.Abs(currentToTarget) <= stepChange){
            return target;
        } else {
            return current + (sign * stepChange);
        }
    }
    
    public static float EaseInOut(float t){
        return EaseInOut(t, 0.5f);
    }

    public static float EaseInOut(float t, float cutoff){
        if(cutoff > 1.0f){ cutoff = 1.0f; }
        if(cutoff < 0.0f){ cutoff = 0.0f; }

        if(t <= cutoff){
            return t * t / cutoff;
        } else {
            t = (1.0f - t);
            t *= t;
            t /= (1.0f - cutoff);
            return 1.0f - t;
        }
    }
}
