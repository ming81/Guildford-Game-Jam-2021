using UnityEngine;

public class InteractableLight : Interactable
{
    public GameObject lightFixture;
    public GameObject lightEmitter;
    public GameObject particle;
    private bool isOn = true;

    void Start()
    {
        interactableType = InteractableType.Light;
    }

    public override void Interact()
    {
        base.Interact();
        EnableLight();
    }

    void EnableLight()
    {
        if (!isOn)
        {
            lightEmitter.SetActive(true);
            particle.SetActive(true);
            isOn = true;
            Debug.Log("Turning on " + lightFixture.name);
        }
        else if (isOn)
        {
            lightEmitter.SetActive(false);
            particle.SetActive(false);
            isOn = false;
            Debug.Log("Turning off " + lightFixture.name);
        }

    }
}

