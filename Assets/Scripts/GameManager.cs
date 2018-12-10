using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Action { Eat, Sleep, Get, Put, Build, Make, Labor, Forester, ChopTree, PlantTree, None, Wander };
public enum Resource { Wheat, Grain, Wood, Planks, Stone, Bricks, None };
public enum TargetType { Farm, Mill, Forestry, SawMill, Quary, Mason, StoreHouse, Coop, Granary, Sapling, SaplingPlaceHolder, Tree, Wood, Planks, Stone, Bricks, Wheat, Grain, None };
public enum Job { Farmer, Millworker, Lumberjack, SawMill, Miner, Mason, Laborer, Builder, Student, Kid, None };
public enum ChickenType { Chick, Cockerel, Pullet, Rooster, Hen };
public enum EducationLevel { Moron, Diploma, Degree, Phd }
public enum Season { Spring, Summer, Fall, Winter };
public enum Month { March, April, May, June, July, Aug, Sep, Oct, Nov, Dec, Jan, Feb }
//inventories / materials / ingredients etc. are always in order for array - wheat grain wood planks stone bricks ..

public class GameManager : MonoBehaviour
{

	ChickenManager chickenManager;
	TaskCreator taskCreator;

	//[HideInInspector]
	public int time;
	public int day;
	public Month month;
	public Season season;
	public int year;

	public bool night;

	public bool paused;
	bool running;
	public float gameSpeed;

	// Use this for initialization
	void Start()
	{
		chickenManager = GameObject.Find("GameManager").GetComponent<ChickenManager>();
		taskCreator = GameObject.Find("GameManager").GetComponent<TaskCreator>();
		StartCoroutine(PassTime());
	}

	// Update is called once per frame
	void Update()
	{
		day = (int)Mathf.Floor(time / 7) % 30; // 7 seconds is an day
		month = (Month)((int)Mathf.Floor(time / 7 / 30) % 12); // 210 seconds is a month (3.5 minutes)
		season = (Season)((int)Mathf.Floor(time / 7 / 30 / 3) % 4); // 840 seconds is a season // 14 minutes
		year = (int)Mathf.Floor(time / 7 / 30 / 4 / 4); // 3360 seconds is one year // 56 minutes


		if (paused)
		{
			Time.timeScale = 0;
		}
		else
		{
			/* we need a new 'night' thing - make chickens go home just every now and then, no good way to fit into our timescale
			 * random range even. 
			 * bing chicken to house, make them return every so often to rest/eat. if theres no food thewy have to get some first
			 * if its winter and no wood they have to get some first. if they cant find place to get food for their house hungry..
			if ((hour <= 6 || hour >= 22)) // if its night
			{
				night = true;
				Time.timeScale = gameSpeed * 5;
			}
			else
			{*/

			night = false;
			Time.timeScale = gameSpeed;

		}

	}

	public void SetGameSpeed(float speed)
	{
		gameSpeed = speed;
	}

	public void PauseGame()
	{
		if (!paused)
		{
			paused = true;
		}
		else paused = false;
	}

	IEnumerator PassTime()
	{
		int update = 0;
		while (gameObject.activeSelf)
		{
			if (!paused)
			{
				time++;
				update++;

				if (time > 0 && update >= 5) // if it's been 5 minutes since last time, let's update all the chicken's age
				{
					chickenManager.UpdateChickens();
					update = 0; // and reset the timer
				}
			}

			yield return new WaitForSeconds(1);
		}
	}

}
