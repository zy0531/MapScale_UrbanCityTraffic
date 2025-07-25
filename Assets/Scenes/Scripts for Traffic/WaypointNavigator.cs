using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    AnimationStateController controller;
    public Waypoint currentWaypoint;

    int direction;

    private void Awake()
    {
        controller = GetComponent<AnimationStateController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        controller.SetDestination(currentWaypoint.GetPosition());
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.reachDestination)
        {

            bool shouldBranch = false;
            if (currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
            {
                shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchRatio ? true : false;
            }
            if (shouldBranch)
            {
                currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
                controller.SetDestination(currentWaypoint.GetPosition());
            }
            else
            {
                if (direction == 0)
                {
                    if (currentWaypoint.nextWaypoint != null)
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                        direction = 1;
                    }
                }
                else
                {
                    if (currentWaypoint.previousWaypoint != null)
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                        direction = 0;
                    }
                }
                controller.SetDestination(currentWaypoint.GetPosition());
            }
        }

    }

    /*    // Update is called once per frame
        void Update()
        {
            if(controller.reachDestination)
            {
                if(direction == 0)
                {
                    currentWaypoint = currentWaypoint.nextWaypoint;
                }
                else if(direction == 1)
                {
                    currentWaypoint = currentWaypoint.previousWaypoint;
                }
                controller.SetDestination(currentWaypoint.GetPosition());
            }
        }*/
}
