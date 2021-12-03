using UnityEngine;

public class InteractableDoor : Interactable
{
    private enum DoorType { LeftDoor, RightDoor, Locked }
    [SerializeField] private DoorType doorType;

    public Animator doorAnim;
    public GameObject door;

    void Start()
    {
        interactableType = InteractableType.Door;
    }

    public override void Interact()
    {
        base.Interact();

        OpenDoor();
    }

    void OpenDoor()
    {
        bool isOpenLeft = doorAnim.GetBool("isOpenLeft");
        bool isOpenRight = doorAnim.GetBool("isOpenRight");

        if (doorType == DoorType.RightDoor)
            doorAnim.SetBool("isRightDoor", true);
        else if (doorType == DoorType.LeftDoor)
            doorAnim.SetBool("isRightDoor", false);

        if (doorType == DoorType.LeftDoor)
        {
            if (!isOpenLeft)
            {
                doorAnim.SetBool("isOpenLeft", true);
                Debug.Log("Opening " + door.name);
            }
            else if (isOpenLeft)
            {
                doorAnim.SetBool("isOpenLeft", false);
                Debug.Log("Closing " + door.name);
            }
        }
        else if (doorType == DoorType.RightDoor)
        {
            if (!isOpenRight)
            {
                doorAnim.SetBool("isOpenRight", true);
                Debug.Log("Opening " + door.name);
            }
            else if (isOpenRight)
            {
                doorAnim.SetBool("isOpenRight", false);
                Debug.Log("Closing " + door.name);
            }
        }
        else if (doorType == DoorType.Locked)
        {
            // Do nothing.
            if (isOpenRight || isOpenLeft)
            {
                doorAnim.SetBool("isOpenRight", false);
                doorAnim.SetBool("isOpenLeft", false);
            }

            Debug.Log(door.name + " is locked.");
        }

    }
}

