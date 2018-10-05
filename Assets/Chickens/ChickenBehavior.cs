using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ChickenBehavior : MonoBehaviour
{

	/*TODO
	 *  make finder evaluate the state of grainpiles and see if they need filling, if they do, do we have wheat in our mills? if we do, lets mill, if we dont, lets farm
	 *  make chickens leave the area/wander if they're looking for a job
	 *  incorporate timescale to all actions/speeds of chickens so we can speed up and slow down the game
	 *  throw in some interfaces so we can see whats going on
	 *  coops for sleeps
	 *  paths - a* ground has penalty, path doesn't (maybe a negative penalty on paths instead?)
	 *  
	 *  trees / rocks
	 *  scenery
	 *  Clouds / weather / day night cycles / seasons / ambient birds and crap
	 *  
	 */


	GameManager gameManager;
	AIPath ai;
	Finder finder;
	ChickenBehavior chicken;

	public string name = "Richard Cluckins";
	public float age = 2;
	public string gender = "male";
	public float energy;

	public bool awake;
	public bool hungry;
	public bool alive = true;

	public bool atTarget;
	public bool onTask;
	public GameObject currentTask;

	public string resourceCarried;
	public int carrying;
	public int maxCarry;

	public int carryBonus = 0;
	public int farmingBonus = 0;
	public int millBonus = 0;



	// Use this for initialization
	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		ai = GetComponent<AIPath>();
		finder = GetComponent<Finder>();
		chicken = GetComponent<ChickenBehavior>();

		transform.name = name + " | " + gender + " | " + age;

		StartCoroutine(BeChicken());

	}

	// Update is called once per frame
	void Update()
	{

		maxCarry = 25 + carryBonus;

		if (energy <= 20)
		{
			hungry = true;
		}
		if (energy >= 100)
		{
			hungry = false;
		}

		//Wake up!
		if (gameManager.hour >= 6 && awake == false)
		{
			StartCoroutine(Wake());
		}

		//Go to sleep!
		if (gameManager.hour >= 22 && awake == true)
		{
			awake = false;
		}

	}


	public IEnumerator Wake()
	{
		float randomWait = Random.Range(0, 3.0f);
		yield return new WaitForSeconds(randomWait);
		awake = true;
	}

	public IEnumerator BeChicken() //set navigation target and wait for chicken to arrive
	{
		while (alive)
		{
			while (awake)
			{
				if (!onTask)
				{
					finder.FindTask();
					yield return new WaitForFixedUpdate();

					currentTask = finder.FindBest();

					while (currentTask == null)
					{
						yield return new WaitForSeconds(1);
						Debug.Log(chicken.name + " is trying again to find a task.");
						finder.FindTask();
						yield return new WaitForFixedUpdate();
						currentTask = finder.FindBest();
						yield return new WaitForFixedUpdate();
					}

					Info taskInfo = currentTask.GetComponent<Info>();
					taskInfo.SpotsUsed++;

					ai.destination = currentTask.transform.position;
					ai.SearchPath();
					Debug.Log(name + taskInfo.whatDoing);
				}

				if (onTask)
				{
					Info taskInfo = currentTask.GetComponent<Info>();
					while (taskInfo.open && onTask)
					{
						if (atTarget)
						{
							yield return new WaitForSeconds(taskInfo.tickLength);
							taskInfo.Tick(chicken); // do task through info
							if (carrying == maxCarry)
							{
								onTask = false;
							}
						}

						yield return new WaitForEndOfFrame();
					}
					taskInfo.SpotsUsed--;
				}
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject == currentTask)
		{
			atTarget = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == currentTask)
		{
			atTarget = false;
		}
	}

}
