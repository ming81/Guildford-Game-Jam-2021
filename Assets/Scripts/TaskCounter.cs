using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskCounter : MonoBehaviour
{
    public float completedTasks;
    public float numberOfTasks;
    public string supplyCounterText;
    public Text supplyCounter;
    public GameObject boat;

    private void Start()
    {
        completedTasks = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (completedTasks < numberOfTasks)
        {
            supplyCounter.text = supplyCounterText;
            supplyCounterText = "(F to Interact) Supplies Found: " + completedTasks + "/" + numberOfTasks;
        }
        else if (completedTasks == numberOfTasks)
        {
            BuildTheBoat();
            supplyCounter.text = supplyCounterText;
            supplyCounterText = "Time to escape! Go to where you woke up.";
        }
    }

    public void CompleteTask()
    {
        completedTasks += 1;
        Debug.Log("Task completed");
    }

    void BuildTheBoat()
    {
        // Activate the boat GameObject.
        boat.SetActive(true);
        Debug.Log("All tasks completed.");
    }
}
