using UnityEngine;

public class ShowButtons : MonoBehaviour
{
    public GameObject[] buttons;  

    private void Start()
    {
        SetButtonsActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        SetButtonsActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        SetButtonsActive(false);
    }

    private void SetButtonsActive(bool active)
    {
        foreach (GameObject btn in buttons)
        {
            if (btn != null)
                btn.SetActive(active);
        }
    }
}