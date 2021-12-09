using UnityEngine;
using Bleak.Controller;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;
    public Transform interactionTransform;
    public TaskCounter taskCounter;
    public GameObject gameManager;
    public string message;
    public GameObject textPrefab;
    public GameObject finishedGame;

    bool isFocus = false;
    public Transform player;
    public GameObject playerObject;
    public PlayerControllerIK playerControllerIK;
    public Animator _anim;

    bool hasInteracted = false;
    public bool completed = false;
    public bool destroyWhenPickedUp = true;
    public bool isATask = true;
    public bool interactBoat = false;

    private void Start()
    {

    }

    public virtual void Interact()
    {
        ShowFloatingText();

        if (interactBoat)
        {
            finishedGame.SetActive(true);
        }

        if (isATask)
        {
            if (!completed && !destroyWhenPickedUp)
            {
                taskCounter.CompleteTask();
                completed = true;
            }
            else if (!completed && destroyWhenPickedUp)
            {
                taskCounter.CompleteTask();
                completed = true;
                Destroy(gameObject);
            }
        }
        else if (!isATask)
        {
            if (destroyWhenPickedUp)
                Destroy(gameObject);
        }
    }

    void Update()
    {
        if (player == null)
        {
            gameManager = GameObject.FindWithTag("GameManager");
            taskCounter = gameManager.GetComponent<TaskCounter>();
            playerObject = GameObject.FindWithTag("Player");
            player = playerObject.transform;
            playerControllerIK = playerObject.GetComponent<PlayerControllerIK>();
            _anim = playerObject.GetComponent<Animator>();
            textPrefab = (GameObject)Resources.Load("TextPrefabs/FloatingText", typeof(GameObject));
        }

        if (isFocus && !hasInteracted)
        {
            float distance = Vector3.Distance(player.position, interactionTransform.position);
            if (distance <= radius)
            {
                Interact();
                hasInteracted = true;
            }
        }
    }

    public void ShowFloatingText()
    {
        var go = Instantiate(textPrefab, player.transform.position + new Vector3(0, 2.594f, 0), Quaternion.identity, player.transform);
        go.GetComponent<TextMesh>().text = message;
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
