using UnityEngine;
using Bleak.Controller;

public class InteractableBed : Interactable
{
    public GameObject bed;
    public Animator playerAnim;

    void Start()
    {
        interactableType = InteractableType.Bed;
    }

    public override void Interact()
    {
        base.Interact();

        SleepInBed();
    }

    void SleepInBed()
    {
        bool sleeping = false;
        Vector3 offSetPosition = transform.position + new Vector3(0.2f, 0.53f, 0.2f);
        Quaternion offSetRotation = transform.rotation * Quaternion.Euler(new Vector3(0, 200, 0));

        if (!sleeping)
        {
            player.position = offSetPosition;
            player.rotation = offSetRotation;
            sleeping = true;
        }
        else if (sleeping)
        {
            sleeping = false;
        }
    }
}

