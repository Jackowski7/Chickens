using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info : MonoBehaviour {

	[HideInInspector]
	public string whatDoing;

	public bool open;

	[HideInInspector]
	public int SpotsUsed;
	[HideInInspector]
	public int maxSpots;

	[HideInInspector]
	public float tickLength;


	public void Tick(ChickenBehavior chicken)
	{
		if (transform.tag == "GrainPile")
		{
			GetComponent<GrainPile>().Tick(chicken);
		}
		if (transform.tag == "Farm")
		{
			GetComponent<Farm>().Tick(chicken);
		}
		if (transform.tag == "Mill")
		{
			GetComponent<Mill>().Tick(chicken);
		}

	}

}
