using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Mill : MonoBehaviour
{

	GameManager gameManager;
	Info info;

	public int maxSpots = 6; // how many chickens can do this at once
	public float tickLength = 1; // seconds between actions
	public float energyUsed = 1; // how much energy each tick uses

	[HideInInspector]
	public string whatDoing = " is going to the Mill. ";  // Rufus cluckleford ....

	public float wheatAmount;

	public Text wheatAmountUI;

	private void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		info = GetComponent<Info>();

		info.tickLength = tickLength;
		info.maxSpots = maxSpots;
		info.whatDoing = whatDoing;

		info.open = true;
	}

	private void Update()
	{
		wheatAmountUI.text = "Wheat: " + wheatAmount;
	}

	public void Tick(ChickenBehavior chicken)
	{

		if (chicken.resourceCarried == "Wheat")
		{
			wheatAmount += chicken.carrying;
			chicken.carrying = 0;
			chicken.onTask = false;
		}
		else
		{

			if (chicken.carrying < chicken.maxCarry && wheatAmount > 0 && (chicken.energy - energyUsed) >= 0)
			{
				chicken.energy -= energyUsed;

				chicken.resourceCarried = "Grain";
				chicken.carrying += (1 + chicken.millBonus);
				chicken.carrying = Mathf.Min(chicken.carrying, chicken.maxCarry);

				wheatAmount -= (1 + chicken.millBonus);
			}
			else
			{
				chicken.onTask = false;
			}
		}

	}

}
