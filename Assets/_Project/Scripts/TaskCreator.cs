using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskCreator : MonoBehaviour
{

	GameManager gameManager;
	ChickenManager chickenManager;

	public List<GameObject> targets = new List<GameObject> { };

	public int totalChickens;
	public int freeRangeChickens;

	private void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		chickenManager = GameObject.Find("GameManager").GetComponent<ChickenManager>();
	}

	// determine chicken job and create potential jobs, test and see if they are available, and do them (or their backup jobs)
	public void GetJobQueue(Chicken chicken)
	{
		chickenManager.UpdateChicken(chicken); //update this chicken

		//clear out old queues
		chicken.targetQueue = new List<GameObject> { };
		chicken.actionQueue = new List<Action> { };
		chicken.resourceNeededQueue = new List<Resource> { };

		chicken.currentTarget = null;
		chicken.action = Action.None;
		chicken.resourceNeeded = Resource.None;

		if (gameManager.night == true)
		{
			Sleep(chicken);
		}
		else
		{
			if (chicken.job == Job.Farmer)
			{
				if (!MakeResource(chicken, Resource.Wheat, null))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.Millworker)
			{
				if (!MakeResource(chicken, Resource.Grain, null))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.Miner)
			{
				if (!MakeResource(chicken, Resource.Stone, null))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.Mason)
			{
				if (!MakeResource(chicken, Resource.Bricks, null))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.Lumberjack)
			{
				if (!Forester(chicken))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.SawMill)
			{
				if (!MakeResource(chicken, Resource.Planks, null))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}

			if (chicken.job == Job.Builder)
			{
				if (!Build(chicken))
				{
					AddTask(chicken, chicken.gameObject, Action.Wander);
				}
			}
		}

	}

	// go build the nearest building
	public bool Sleep(Chicken chicken)
	{
		GameObject target = FindTarget(chicken, Action.Sleep, Resource.None, null);
		if (target != null)
		{
			AddTask(chicken, target, Action.Sleep, Resource.None); // do the thing we intended to			
			return true;
		}
		else
		{
			return false;
		}
	}

	// go build the nearest building
	public bool Build(Chicken chicken)
	{
		GameObject target = FindTarget(chicken, Action.Build, Resource.None, null);
		if (target != null)
		{
			AddTask(chicken, target, Action.Build, Resource.None); // do the thing we intended to			
			return true;
		}
		else
		{
			return false;
		}
	}

	// go make a resource ( the chicken to do so, the resource we want to make, and the specific place we want to make it at (like from a assigned joblocation)
	public bool MakeResource(Chicken chicken, Resource resource, GameObject target)
	{
		if (target == null) // if we didn't set a target, get one
		{
			target = FindTarget(chicken, Action.Make, resource, null);
		}

		if (target != null)
		{
			if (chicken.totalCarried > 0)
			{
				if (chicken.resourceCarried != resource) // if we're carrying something other than what we're going to make
				{
					DropOffResources(chicken, null);
				}
				else if (chicken.totalCarried > (chicken.maxCarry / 2)) // if we're more than half full
				{
					DropOffResources(chicken, null);
				}
			}

			AddTask(chicken, target, Action.Make, resource); // go make the resource
			return true;
		}
		else
		{
			return false;
		}
	}

	//get a specific resource of an optional quantity
	public bool GetResource(Chicken chicken, Resource resource, GameObject target, int amountNeeded = 100)
	{

		if (chicken.totalCarried > 0 && chicken.resourceCarried != resource)
		{
			if (!DropOffResources(chicken, null))
			{
				Debug.Log("We don't have any more room to store " + resource.ToString());
			}
		}
		else if (chicken.totalCarried > (chicken.maxCarry / 2))
		{
			if (!DropOffResources(chicken, null))
			{
				Debug.Log("We don't have any more room to store " + resource.ToString());
			}
		}


		if (target == null)
		{
			target = FindTarget(chicken, Action.Get, resource, null);
		}
		if (target != null)
		{
			AddTask(chicken, target, Action.Get, resource, amountNeeded);
			return true;
		}
		else
		{
			return false;
		}
	}

	public bool DropOffResources(Chicken chicken, GameObject target)
	{
		if (target == null)
		{
			target = FindTarget(chicken, Action.Put, chicken.resourceCarried, null);
		}

		if (target != null)
		{
			AddTask(chicken, target, Action.Put, chicken.resourceCarried);
			return true;
		}
		else
		{
			return false;
		}
	}

	// go make a resource ( the chicken to do so, the resource we want to make, and the specific place we want to make it at (like from a assigned joblocation)
	public bool Forester(Chicken chicken)
	{

		GameObject target = FindTarget(chicken, Action.Forester, Resource.Wood, null);

		if (target != null)
		{
			if (chicken.totalCarried > 0 && chicken.resourceCarried != Resource.Wood)
			{
				if (!DropOffResources(chicken, null))
				{
					Debug.Log("We don't have any more room to store " + Resource.Wood.ToString());
				}
			}
			else if (chicken.totalCarried > (chicken.maxCarry / 2))
			{
				if (!DropOffResources(chicken, null))
				{
					Debug.Log("We don't have any more room to store " + Resource.Wood.ToString());
				}
			}
			else
			{
				AddTask(chicken, target, Action.Forester); // go make the resource
			}

			return true;
		}
		else
		{
			Debug.Log(chicken.name + " couldnt find a place to " + Action.Make.ToString() + " " + Resource.Wood.ToString());
			return false;
		}
	}

	public bool PlantTree(Chicken chicken)
	{
		GameObject target = FindTarget(chicken, Action.PlantTree, Resource.None, null);
		if (target != null)
		{
			AddTask(chicken, target, Action.PlantTree, Resource.None); // do the thing we intended to			
			return true;
		}
		else
		{
			Debug.Log(chicken.name + " couldnt find a place to " + Action.PlantTree.ToString());
			return false;
		}
	}


	public bool ChopTree(Chicken chicken)
	{
		GameObject target = FindTarget(chicken, Action.ChopTree, Resource.None, null);
		if (target != null)
		{
			AddTask(chicken, target, Action.ChopTree, Resource.None); // do the thing we intended to			
			return true;
		}
		else
		{
			Debug.Log(chicken.name + " couldnt find a place to " + Action.ChopTree.ToString());
			return false;
		}
	}


	GameObject FindTarget(Chicken chicken, Action action, Resource resource, GameObject destination)
	{
		GameObject closestTarget = null;
		List<GameObject> potentialTargets = EvaluateTargets(chicken, action, resource);

		if (potentialTargets.Count > 0)
		{
			float closestDistanceSqr = Mathf.Infinity;
			float totalDistance = 0;
			Vector3 orgin = chicken.transform.position;

			foreach (GameObject potentialTarget in potentialTargets)
			{
				Vector3 directionToTarget = potentialTarget.transform.position - orgin;

				if (destination != null)
				{
					totalDistance = (((orgin - potentialTarget.transform.position).sqrMagnitude) + ((potentialTarget.transform.position - destination.transform.position).sqrMagnitude));
				}
				else
				{
					totalDistance = directionToTarget.sqrMagnitude;
				}

				if (totalDistance < closestDistanceSqr)
				{
					closestDistanceSqr = totalDistance;
					closestTarget = potentialTarget;
				}
			}
		}
		return closestTarget;
	}

	List<GameObject> EvaluateTargets(Chicken chicken, Action action, Resource resource)
	{
		List<GameObject> potentialTargets = new List<GameObject>();

		for (int x = 0; x < targets.Count; x++)
		{
			Target target = targets[x].GetComponent<Target>();


			if (action == Action.Sleep) // if we're sleeping
			{
				if (target.actions.Contains(Action.Sleep)) // if this place lets us sleep
				{
					if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						potentialTargets.Add(targets[x]);  // add it to the list to consider
				}
			}

			if (action == Action.Get) // if we're getting something
			{
				if (target.actions.Contains(Action.Get)) // if this place let's us get
				{
					if (target.inventory[(int)resource].x > 0) // and it has some of this resource
					{
						if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
							potentialTargets.Add(targets[x]);  // add it to the list to consider
					}
				}
			}

			if (action == Action.Put) // if we're putting something
			{
				// if this place has room to put what we're putting
				if (target.inventory[(int)resource].x < target.inventory[(int)resource].y)
				{
					if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						potentialTargets.Add(targets[x]);  // add it to the list to consider
				}
			}

			if (action == Action.Make)
			{
				if (target.actions.Contains(Action.Make) && target.resourceProduced == resource) // if this place lets us make what we're making
				{
					bool hasIngredients = true;
					Resource resourceNeeded = Resource.None;

					for (int y = 0; y < target.actionIngredients.Count; y++)
					{
						if (target.inventory[y].x < target.actionIngredients[y] && hasIngredients)
						{
							hasIngredients = false;
							resourceNeeded = (Resource)y;
						}
					}

					if (hasIngredients)
					{
						if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
							potentialTargets.Add(targets[x]);  // add it to the list to consider
					}
					else
					{
						if (GetResource(chicken, resourceNeeded, null)) // get the first material we come across that we dont have, in the amount that we need
						{
							DropOffResources(chicken, target.gameObject); // put the materials here
						}
					}
				}
			}

			if (action == Action.Forester)
			{
				if (target.actions.Contains(Action.Forester) && target.resourceProduced == resource) // if this place lets us make what we're making
				{
					if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						potentialTargets.Add(targets[x]);  // add it to the list to consider
				}
			}

			if (action == Action.PlantTree)
			{
				if (target.actions.Contains(Action.PlantTree))
				{
					if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						potentialTargets.Add(targets[x]);  // add it to the list to 
				}
			}

			if (action == Action.ChopTree)
			{
				if (target.actions.Contains(Action.ChopTree))
				{
					if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						potentialTargets.Add(targets[x]);  // add it to the list to 
				}
			}

			if (action == Action.Build) // if we're building or laboring
			{
				if (target.actions.Contains(Action.Build)) // if this place lets us build
				{
					Construction construction = target.GetComponent<Construction>();

					bool hasMaterials = true;
					int amountNeeded = 0;
					Resource resourceNeeded = Resource.None;

					for (int y = 0; y < construction.constructionMaterials.Count; y++)
					{
						if (target.inventory[y].x < construction.constructionMaterials[y] && hasMaterials)
						{
							hasMaterials = false;
							amountNeeded = construction.constructionMaterials[y] - (int)target.inventory[y].x;
							resourceNeeded = (Resource)y;
						}
					}

					if (hasMaterials)
					{
						if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
							potentialTargets.Add(targets[x]);  // add it to the list to consider
					}
					else
					{
						if (GetResource(chicken, resourceNeeded, null, amountNeeded)) // get the first material we come across that we dont have, in the amount that we need
						{
							DropOffResources(chicken, target.gameObject); // put the materials here
						}
					}
				}
			}

			if (action == Action.Labor) // if we're making something or laboring
			{
				if (target.actions.Contains(Action.Make)) // if this place lets us make what we're making
				{
					bool hasIngredients = true;
					for (int y = 0; y < target.actionIngredients.Count; y++)
					{
						if (target.inventory[y].x < target.actionIngredients[y])
						{
							hasIngredients = false;
						}
					}
					if (!hasIngredients) // if it doesn't have the ingredients // TODO we need to make this a priority over other tasks, and laborers need to keep putting resources at any open place that doesnt have max cap.
					{
						if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						{
							potentialTargets.Add(targets[x]);  // add it to the list to consider
						}
					}
				}
				if (target.actions.Contains(Action.Build))
				{
					Construction construction = target.GetComponent<Construction>();

					bool hasMaterials = true;
					for (int y = 0; y < construction.constructionMaterials.Count; y++)
					{
						if (target.inventory[y].x < construction.constructionMaterials[y])
						{
							hasMaterials = false;
						}
					}
					if (!hasMaterials) // if it doesn't have the ingredients // TODO we need to make this a priority over other tasks, and laborers need to keep putting resources at any open place that doesnt have max cap.
					{
						if (target.open && target.chickens.Count < target.maxSpots)// if it's open and has space
						{
							potentialTargets.Add(targets[x]);  // add it to the list to consider
						}
					}
				}
			}

		}

		return potentialTargets;
	}



	//add task functions
	public void AddTask(Chicken chicken, GameObject target, Action action)
	{
		chicken.targetQueue.Add(target);
		chicken.actionQueue.Add(action);
		chicken.resourceNeededQueue.Add(Resource.None);
		chicken.amountNeededQueue.Add(0);
	}

	public void AddTask(Chicken chicken, GameObject target, Action action, Resource resource)
	{
		chicken.targetQueue.Add(target);
		chicken.actionQueue.Add(action);
		chicken.resourceNeededQueue.Add(resource);
		chicken.amountNeededQueue.Add(100);
	}

	public void AddTask(Chicken chicken, GameObject target, Action action, Resource resource, int amount)
	{
		chicken.targetQueue.Add(target);
		chicken.actionQueue.Add(action);
		chicken.resourceNeededQueue.Add(resource);
		chicken.amountNeededQueue.Add(amount);
	}

}
