using UnityEngine;

public class StartWorkout : MonoBehaviour
{
    [Header("Target to control")]
    [SerializeField] private GameObject model;   // model
    [SerializeField] private int workoutType = 1; 
    [SerializeField] private Transform anchor;

    public void Play()   // shows up in Button OnClick()
    {
        if (!model) return;

        // move model
        if (anchor)
        {
            model.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }

        // activate
        if (!model.activeSelf) model.SetActive(true);

        // use animation
        var anim = model.GetComponentInChildren<Animator>(); // Animator 在 Person_Standin_Dummy
        if (anim)
        {
            // Idle first
            anim.SetInteger("WorkoutType", 0);
            // Then start workout
            anim.SetInteger("WorkoutType", workoutType);
        }
    }

    public void Stop()   // optional "stop" button
    {
        if (!model) return;

        var anim = model.GetComponent<Animator>();
        if (anim) anim.SetInteger("WorkoutType", 0); // back to Idle
        model.SetActive(false);
    }
}
