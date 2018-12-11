using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Action { Eat, Sleep, Get, Put, Build, Make, Labor, Forester, ChopTree, PlantTree, None, Wander };
public enum Resource { Wheat, Grain, Wood, Planks, Stone, Bricks, None };
public enum TargetType { Farm, Mill, Forestry, SawMill, Quary, Mason, StoreHouse, Coop, Granary, Sapling, SaplingPlaceHolder, Tree, Wood, Planks, Stone, Bricks, Wheat, Grain, None, Rock };
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
		if (paused)
		{
			Time.timeScale = 0;
		}
		else
		{
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

			season = (Season)((int)Mathf.Floor(time / 30) % 4); // 30 seconds is a season
			year = (int)Mathf.Floor(time / 120) + 1; // 2 minutes is one year

			yield return new WaitForSeconds(1f);
		}
	}

}
