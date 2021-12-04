using UnityEngine;
using Bleak.Controller;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;
    public Transform interactionTransform;
    public InteractableType interactableType;

    bool isFocus = false;
    public Transform player;
    public GameObject playerObject;
    public PlayerControllerIK playerControllerIK;
    public Animator _anim;

    bool hasInteracted = false;

    public virtual void Interact()
    {
        // This method is meant to be overwritten
    }

    void Update()
    {
        if (isFocus && !hasInteracted)
        {
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            if (distance <= radius)
            {
                Interact();
                hasInteracted = true;

                playerObject = player.gameObject;
                playerControllerIK = playerObject.GetComponent<PlayerControllerIK>();
                _anim = playerObject.GetComponent<Animator>();
                bool sleeping = _anim.GetBool("Sleeping");

                if (interactableType == InteractableType.Chest)
                {
                    _anim.SetTrigger("InteractChest");
                }
                if (interactableType == InteractableType.Door)
                {
                    _anim.SetTrigger("InteractDoor");
                }
                if (interactableType == InteractableType.Item)
                {
                    _anim.SetTrigger("InteractItem");
                }
                if (interactableType == InteractableType.Light)
                {
                    _anim.SetTrigger("InteractLight");
                }
                if (interactableType == InteractableType.Switch)
                {
                    _anim.SetTrigger("InteractSwitch");
                }
                if (interactableType == InteractableType.Bed)
                {
                    if (!sleeping)
                    {
                        _anim.SetBool("Sleeping", true);
                        playerControllerIK.movementEnabled = false;
                        playerControllerIK.combatEnabled = false;
                        playerControllerIK.ikWeight = 0;
                    }
                    else if (sleeping)
                    {
                        _anim.SetBool("Sleeping", false);
                        playerControllerIK.movementEnabled = true;
                        playerControllerIK.combatEnabled = true;
                        playerControllerIK.ikWeight = 1;
                    }
                }
                if (interactableType == InteractableType.Chair)
                {
                    _anim.SetTrigger("InteractChair");
                }
            }
        }
    }

    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
    }

    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
    }

    void OnDrawGizmosSelected()
    {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionTransform.position, radius);
    }
}

public enum LetterNumber { one, two, three, four, five, six, seven, eight, nine, ten }
public enum InteractableType { Chest, Light, Item, Door, Switch, Bed, Chair }

