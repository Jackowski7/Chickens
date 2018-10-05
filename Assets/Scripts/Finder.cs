using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finder : MonoBehaviour
{
	ChickenBehavior chicken;
	public List<Transform> potentials = new List<Transform>();


	private void Start()
	{
		chicken = GetComponent<ChickenBehavior>();
	}

	public string searchTag;
	public void FindTask()
	{


		// poll nearby structures (radius ~10?) - look for grain piles low on grain, fill em up.

		if (chicken.carrying > 0) // if we're carrying a resource
		{

			if (chicken.resourceCarried == "Wheat") // if we've got wheat
			{
				if (chicken.carrying <= (chicken.maxCarry / 2)) // if we're less than half full of our carry limit
				{
					searchTag = "Farm"; // got to a farm to keep getting wheat
				}
				else
				{
					searchTag = "Mill"; // go to a Mill to drop off our wheat
				}
			}
			if (chicken.resourceCarried == "Grain") // if we've got Grain
			{
				searchTag = "GrainPile"; // go drop off our Grain
			}

		}
		else if (chicken.hungry)
		{
			searchTag = "GrainPile";
		}
		else // if we're not carrying a resource
		{
			//check needed resources?
			//check chicken affinity?
			searchTag = "Farm";
		}

		chicken.onTask = true;
		GetComponent<SphereCollider>().radius = 50f;


	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == searchTag)
		{
			Info info = other.GetComponent<Info>();
			if (info.open && info.SpotsUsed < info.maxSpots)
			{
				potentials.Add(other.transform);
			}
		}
	}

	public GameObject FindBest()
	{
		if (potentials.Count > 0)
		{
			Transform bestTarget = null;
			float closestDistanceSqr = Mathf.Infinity;
			Vector3 currentPosition = this.transform.position;

			foreach (Transform potentialTarget in potentials)
			{
				Vector3 directionToTarget = potentialTarget.position - currentPosition;
				float dSqrToTarget = directionToTarget.sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					bestTarget = potentialTarget;
				}
			}

			/* if (searchTag == "Coop")
			{
				bestTarget.gameObject.GetComponent<Coop>().vacancies--; // remove the vacancies as they're selected instead of once they arrive
				// if a chicken doesn't make it? We'll set the vacancies to it's actual amount at some point(s)
			} */

			searchTag = "";
			potentials.Clear();
			GetComponent<SphereCollider>().radius = .75f;
			return bestTarget.gameObject;
		}
		else
		{
			searchTag = "";
			potentials.Clear();
			GetComponent<SphereCollider>().radius = .75f;
			return null;
		}
	}



}
