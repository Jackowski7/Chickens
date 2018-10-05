using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{

	GameManager gameManager;
	Info info;

	public int maxSpots = 6; // how many chickens can do this at once
	public float tickLength = 1; // seconds between actions
	public float energyUsed = 1; // how much energy each tick uses

	[HideInInspector]
	public string whatDoing = " is going to go farming. ";  // Rufus cluckleford ....

	private void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		info = GetComponent<Info>();

		info.tickLength = tickLength;
		info.maxSpots = maxSpots;
		info.whatDoing = whatDoing;
	}

	private void Update()
	{
		if (gameManager.season != "Winter")
		{
			info.open = true;
		}
		else
		{
			info.open = false;
		}
	}

	public void Tick(ChickenBehavior chicken)
	{

		if (chicken.carrying < chicken.maxCarry && (chicken.energy - energyUsed) >= 0)
		{
			chicken.energy -= energyUsed;

			chicken.resourceCarried = "Wheat";
			chicken.carrying += (1 + chicken.farmingBonus);
			chicken.carrying = Mathf.Min(chicken.carrying, chicken.maxCarry);
		}
		else
		{
			chicken.onTask = false;
		}

	}

}
