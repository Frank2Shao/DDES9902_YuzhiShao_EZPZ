using UnityEngine;
public class ResetWorkoutOnEnable : MonoBehaviour
{
    void OnEnable()
    {
        var anim = GetComponent<Animator>();
        if (anim) anim.SetInteger("WorkoutType", 0); // idle at spawn
    }
    void OnDisable()
    {
        var anim = GetComponent<Animator>();
        if (anim) anim.SetInteger("WorkoutType", 0);
    }
}