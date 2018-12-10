using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coop : MonoBehaviour {

	// Use this for initialization
	void Start () {

		//chickens sleep here at night and consume 1 food per chicken
		//coops hold 10 food max, and house 5 chickens max
		//in winter months, coops consume 5 wood per night to keep warm
		//coops hold 10 wood max

		// on sleep
		// remove one grain per chicken who slept here
		// if winter, remove 5 wood
		// if not enough grain, chickens who didnt eat are hungry for next day (perform 1/2 speed)
		// hungry chickens are given priority eating the next night
		// hungry chickens who dont eat again will be starving
		// starving chickens are given priority eating the next night
		// starving chickens who dont eat again will die

		// on wakeup
		// if there is less than 5 grain (another day's worth) pick a chicken who slept here, assign him to get grain
		// if less than 5 wood here (enough for another day) pick a chicken who slept here, assign him to get wood

		// markets are jobs - they deliver food/wood to houses more efficiently than a chicken supplying a coop itself (in a truck or something)


	}

	// Update is called once per frame
	void Update () {
		
	}
}
