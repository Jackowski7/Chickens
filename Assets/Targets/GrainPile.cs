using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrainPile : MonoBehaviour
{
	GameManager gameManager;
	Info info;

	public int maxSpots = 6; // how many chickens can do this at once
	public float tickLength = 1; // seconds between actions

	[HideInInspector]
	public string whatDoing = " is going to eat. ";  // Rufus cluckleford ....

	public int grainAmount;

	public Text grainAmountUI;

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
		if (grainAmount > 0)
		{
			info.open = true;
		}
		else
		{
			info.open = false;
		}

		grainAmountUI.text = "Grain: " + grainAmount;

	}

	public void Tick(ChickenBehavior chicken)
	{

		if (chicken.resourceCarried == "Grain")
		{
			grainAmount += chicken.carrying;
			chicken.carrying = 0;
		}

		if (chicken.hungry && grainAmount > 0)
		{
			chicken.energy += 10;
			chicken.energy = Mathf.Min(chicken.energy, 100f);
			grainAmount -= 1;
		}
		else
		{ 
			chicken.onTask = false;
		}

	}

}
