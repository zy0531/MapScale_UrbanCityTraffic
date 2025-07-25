﻿using UnityEngine;

public class ReasonsStoppingCars : MonoBehaviour
{
	public static void CarInView(GameObject go, Rigidbody rigbody, float distance, float startSpeed, ref float moveSpeed, ref bool tempStop, float distanceToCar = 15)
    {
        if (go == null) return;

        CarAIController car = go.GetComponent<CarAIController>();

        if (distance >= distanceToCar)
        {
            if (car.TEMP_STOP)
            {
                moveSpeed = startSpeed * 0.5f;
            }
            else
            {
                moveSpeed = startSpeed;
            }

            tempStop = false;
        }
        else if (distance < distanceToCar)
        {
            // Brooke added - if (distance < 8f)
            if (distance < 8f)
            {
                tempStop = true;
            }
            else if (car.GetComponent<Rigidbody>().velocity.magnitude < rigbody.velocity.magnitude)
            {
                tempStop = true;
            }
            // Brooke added - in case the two dead lock each other
            else if (car.TEMP_STOP & rigbody.velocity.magnitude < 0.01f)
            {
                tempStop = false;
                moveSpeed = startSpeed * 0.5f;
            }
            else
            {
                if (!car.TEMP_STOP)
                {
                    tempStop = false;
                }
            }
        }
    }

    public static void SemaphoreInView(SemaphorePeople semaphore, float distance, float startSpeed, bool insideSemaphore, ref float moveSpeed, ref bool tempStop, float distanceToSem = 10)
    {
        if (distance >= distanceToSem)
        {
            if (semaphore.CAR_CAN)
            {
                if (semaphore.HOW_MANY > 0)
                {
                    moveSpeed = startSpeed * 0.5f;
                }
                else
                {
                    if (semaphore.FLICKER)
                    {
                        if (!insideSemaphore)
                        {
                            moveSpeed = startSpeed * 0.5f;
                        }
                        else
                        {
                            moveSpeed = startSpeed;
                            tempStop = false;
                        }
                    }
                    else
                    {
                        moveSpeed = startSpeed;
                        tempStop = false;
                    }
                }
            }
            else
            {
                if (!insideSemaphore)
                {
                    moveSpeed = startSpeed * 0.5f;
                }
                else
                {
                    moveSpeed = startSpeed;
                    tempStop = false;
                }
            }
        }
        else
        {
            if (semaphore.CAR_CAN)
            {
                if (semaphore.HOW_MANY > 0)
                {
                    tempStop = true;
                }
                else
                {
                    moveSpeed = startSpeed;
                    tempStop = false;
                }
            }
            else
            {
                if (!insideSemaphore)
                {
                    tempStop = true;
                }
                else
                {
                    if (semaphore.HOW_MANY > 0)
                    {
                        tempStop = true;
                    }
                    else
                    {
                        tempStop = false;
                        moveSpeed = startSpeed;
                    }
                }
            }
        }
    }

    public static void PlayerInView(Transform player, float distance, float startSpeed, ref float moveSpeed, ref bool tempStop)
    {
        Debug.Log("PlayerInView!!!");
        if (distance >= 8.0f)
        {
            moveSpeed = startSpeed * 0.5f;
        }
        else
        {
            tempStop = true;
            Debug.Log("PlayerInView!!! < 8m");
        }
    }

    public static void MainCameraInView(Transform player, float distance, float startSpeed, ref float moveSpeed, ref bool tempStop)
    {
        Debug.Log("MainCameraInView!!!");
        if (distance >= 8.0f)
        {
            moveSpeed = startSpeed * 0.3f;
        }
        else
        {
            tempStop = true;
            Debug.Log("MainCameraInView!!! < 8m");
        }
    }

    //Brooke added
    public static void PedestrianInView(Transform player, float distance, float startSpeed, ref float moveSpeed, ref bool tempStop)
    {
        Debug.Log("PedestrianInView!!!");
        if (distance >= 2.0f)
        {
            moveSpeed = startSpeed * 0.3f;
        }
        else
        {
            tempStop = true;
            Debug.Log("PedestrianInView!!! < 3m");
        }
    }

    public static void BcycleGyroInView(BcycleGyroController controller, Rigidbody rigbody, float distance, float startSpeed, ref float moveSpeed, ref bool tempStop)
    {
        if (distance >= 9.0f)
        {
            if (controller.tempStop)
            {
                moveSpeed = startSpeed * 0.5f;
            }
            else
            {
                moveSpeed = startSpeed;
            }

            tempStop = false;
        }
        else if (distance < 9.0f)
        {
            if (controller.GetComponent<Rigidbody>().velocity.magnitude < rigbody.velocity.magnitude)
            {
                Debug.Log("1");
                tempStop = true;
            }
            else
            {
                if (!controller.tempStop)
                {
                    tempStop = false;
                }
            }
        }
    }
}