using UnityEngine;
public class InteractableChest : Interactable
{
    public Animator chestAnim;
    public GameObject chest;

    void Start()
    {
        interactableType = InteractableType.Chest;
    }

    public override void Interact()
    {
        base.Interact();

        OpenChest();
    }

    void OpenChest()
    {
        bool isOpen = chestAnim.GetBool("isOpen");

        if (!isOpen)
        {
            chestAnim.SetBool("isOpen", true);
            Debug.Log("Opening " + chest.name);
        }
        else if (isOpen)
        {
            chestAnim.SetBool("isOpen", false);
            Debug.Log("Closing " + chest.name);
        }
    }
}

