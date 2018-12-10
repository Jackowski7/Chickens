using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Chicken : MonoBehaviour
{

	GameManager gameManager;
	ChickenManager chickenManager;
	TaskCreator taskCreator;
	Resources resources;
	Chicken chicken;
	AIPath ai;


	[Space(5)]
	[Header("Chicken Information")]
	[Tooltip("Name of this Chicken")]
	public string chickenName;
	public ChickenType chickenType = ChickenType.Cockerel;
	[HideInInspector]
	public int birthMoment; // 'time' chicken was born
	[HideInInspector]
	public int age; // chicken's age in 'time' units


	[Space(5)]
	[Header("Job / Eductation")]
	[Tooltip("Level of Education")]
	public EducationLevel educationLevel = EducationLevel.Moron;
	[Tooltip("Current Job")]
	public Job job;


	[Space(5)]
	[Header("Inventory")]
	[Tooltip("What Resource we're carrying")]
	public Resource resourceCarried;
	[Tooltip("How many we're carrying")]
	public int totalCarried;
	[Tooltip("How big is our inventory?")]
	public int maxCarry;


	[Space(5)]
	[Header("Current Task")]
	[Tooltip("Target we're navigating to")]
	public GameObject target;
	[Tooltip("Action we're going to perform")]
	public Action action;
	[Tooltip("The Resource we're going to make/get")]
	public Resource resourceNeeded;
	[Tooltip("How many we're going to make/get")]
	public int amountNeeded;

	[HideInInspector]
	public List<GameObject> targetQueue = new List<GameObject> { };
	[HideInInspector]
	public List<Action> actionQueue = new List<Action> { };
	[HideInInspector]
	public List<Resource> resourceNeededQueue = new List<Resource> { };
	[HideInInspector]
	public List<int> amountNeededQueue = new List<int> { };

	//some bools about status
	public bool jobQueueStarted = false;

	void OnValidate()
	{
		maxCarry = Mathf.Max(maxCarry, 1);
	}

	// Use this for initialization
	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		chickenManager = gameManager.gameObject.GetComponent<ChickenManager>();
		taskCreator = gameManager.gameObject.GetComponent<TaskCreator>();
		resources = gameManager.gameObject.GetComponent<Resources>();
		chicken = GetComponent<Chicken>();
		ai = GetComponent<AIPath>();

		chickenManager.NewChicken(chicken); // log us in the chickenmanager database and assign a job/status/home etc.

		if (age == 0)
		{
			birthMoment = gameManager.time;
		}

	}

	private void Update()
	{
		chicken.age = gameManager.time - chicken.birthMoment;
		gameObject.name = chickenName + " " + job.ToString();


		if (targetQueue.Count > 0 && !jobQueueStarted)
		{
			jobQueueStarted = true;
			StartCoroutine(chicken.DoJob());
		}

		if (targetQueue.Count > 0 && target == null)
		{
			taskCreator.GetJobQueue(chicken); // your target exploded.. 
		}

	}

	// this is false until the jobs task is complete, when true, the chicken advances to the next job in the queue;
	public bool jobTaskComplete;

	// do the next (or only) job in the job queue
	public IEnumerator DoJob()
	{
		//Debug.Log("Starting Job Queue");
		while (targetQueue.Count > 0) // while we have a job in the queue
		{
			target = targetQueue[0];
			action = actionQueue[0];
			resourceNeeded = resourceNeededQueue[0];
			amountNeeded = amountNeededQueue[0];

			if (target != null)
			{
				jobTaskComplete = false;
				if (target != this.gameObject)
				{
					Info TargetInfo = target.GetComponent<Info>();
					StartCoroutine(GoToTarget(TargetInfo)); //start navigation to the target
				}
				else
				{
					DoAction(null);
				}
			}

			while (!jobTaskComplete)
			{
				yield return new WaitForSeconds(1 / gameManager.gameSpeed);
			}

			if (targetQueue.Count > 0)
			{
				targetQueue.RemoveAt(0);
				actionQueue.RemoveAt(0);
				resourceNeededQueue.RemoveAt(0);
				amountNeededQueue.RemoveAt(0);
			}

			Debug.Log(chicken.chickenName + " has finished his task");
			yield return new WaitForSeconds(1 / gameManager.gameSpeed);
		}

		jobQueueStarted = false;
		taskCreator.GetJobQueue(chicken); // done! get a new list of tasks
	}


	// navigate to the target, if you take too long, you're probably stuck, teleport out. once you get there, do the task we're here to do.
	IEnumerator GoToTarget(Info TargetInfo)
	{
		if (target != null)
		{
			int timeout = 10;
			while (TargetInfo.chickens.Count >= TargetInfo.maxSpots && target != null && timeout >= 0) // while this place is full, wait until it's not full
			{
				timeout--;
				yield return new WaitForSeconds(1 / gameManager.gameSpeed);
			}
			if (timeout > 0 && target != null)
			{

				if (TargetInfo != null)
				{
					TargetInfo.chickens.Add(chicken); // reserve a space 
				}
				if (TargetInfo.parentTarget != null)
				{
					Info parentTargetInfo = TargetInfo.parentTarget.GetComponent<Info>();
					parentTargetInfo.chickens.Add(chicken);
				}

				ai.destination = target.transform.position;
				ai.SearchPath();


				float timeStarted = gameManager.time;
				while (target != null && ((transform.position - target.transform.position).sqrMagnitude) > 1f) // while we're not at the target yet, adn the target does still exist
				{
					if (gameManager.time - timeStarted > 500) // if it's been 500 'time' (minutes), then lets' teleport there // this number will change later idk what it should be
					{
						if (target != null)
						{
							transform.position = target.transform.position;
							Debug.Log("I was stuck and teleported");
						}
						else // if the target doesn't exist, it blew up or something, start from scratch and get a whole new job
						{
							jobTaskComplete = true;
							RemoveReservation();
							Debug.Log("ay the target exploded while i was on the way?");
						}
					}
					yield return new WaitForSeconds(1 / gameManager.gameSpeed);
				}

				if (resourceNeeded != Resource.None) // if we're trying to get
				{
					//Debug.Log(chicken.name + " is going to " + action.ToString() + " " + resourceNeeded.ToString() + " at " + target.name);
				}
				else
				{
					//Debug.Log(chicken.name + " is going to " + action.ToString());
				}

				if (target != null)
				{
					DoAction(TargetInfo);
				}
				else
				{
					Debug.Log("ay the target exploded while i was on the way?");
					jobTaskComplete = true;
					RemoveReservation();
				}
			}
			else
			{
				Debug.Log("Target had no empty slots");
				jobTaskComplete = true;
				RemoveReservation();
			}

		}
		else
		{
			Debug.Log("There was no Target to navigate to");
			jobTaskComplete = true;
			RemoveReservation();
		}
	}

	// [CHICKEN] start doing what you're here to do
	void DoAction(Info TargetInfo)
	{
		if (target != null)
		{
			if (action == Action.Sleep)
			{
				StartCoroutine(Sleep(TargetInfo));
			}

			if (action == Action.Put)
			{
				StartCoroutine(Put(TargetInfo));
			}

			if (action == Action.Get)
			{
				StartCoroutine(Get(TargetInfo));
			}

			if (action == Action.Make)
			{
				StartCoroutine(Make(TargetInfo));
			}

			if (action == Action.Build)
			{
				StartCoroutine(Build(TargetInfo));
			}

			if (action == Action.Forester)
			{
				ChopWood(TargetInfo);
			}

			if (action == Action.ChopTree)
			{
				StartCoroutine(ChopTree(TargetInfo));
			}

			if (action == Action.PlantTree)
			{
				PlantTree(TargetInfo);
			}

			if (action == Action.Wander)
			{
				StartCoroutine(Wander());
			}

			/*
			if (action == Action.Learn)
			{
				StartCoroutine(Learn(TargetInfo));
			}*/

		}
		else // if the target doesn't exist, it blew up or something, start from scratch and get a whole new job
		{
			taskCreator.GetJobQueue(chicken);
			Debug.Log("target blew up before I could do the job");
		}
	}

	//go to sleep until energy is full
	IEnumerator Sleep(Info TargetInfo)
	{
		bool atTarget = true;
		while (gameManager.night && TargetInfo.open && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			yield return new WaitForSeconds(1 / gameManager.gameSpeed);
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	public IEnumerator Wander()
	{
		int timeout = 3;
		float _speed = ai.maxSpeed;
		ai.maxSpeed = 1;

		while (timeout > 0)
		{
			bool spotFound = false;
			int radius = 2;
			Vector3 orgin = transform.position;
			Vector3 spot = Vector3.zero;
			for (int x = 0; x < 10 && !spotFound; x++)
			{
				bool badSpot = true;
				Vector3 _spot = new Vector3(Random.Range(orgin.x - radius, orgin.x + radius), orgin.y, Random.Range(orgin.z - radius, orgin.z + radius));

				LayerMask layer2 = LayerMask.GetMask("Ground");
				Collider[] hitColliders2 = Physics.OverlapSphere(_spot, 1f, layer2);
				for (int y = 0; y < hitColliders2.Length && badSpot; y++)
				{
					badSpot = false;
				}

				LayerMask layer = LayerMask.GetMask("Buildings", "Trees");
				Collider[] hitColliders = Physics.OverlapSphere(_spot, .5f, layer);
				for (int y = 0; y < hitColliders.Length && !badSpot; y++)
				{
					badSpot = true;
				}

				if (!badSpot)
				{
					spotFound = true;
					ai.destination = _spot;
					ai.SearchPath();
				}
			}

			timeout--;
			yield return new WaitForSeconds(3f / gameManager.gameSpeed);
		}

		ai.maxSpeed = _speed;
		jobTaskComplete = true; // set job task complete, so we get next job in queue

	}

	IEnumerator Play(Info TargetInfo)
	{
		//start playing 
		bool atTarget = true;
		while (gameManager.night && TargetInfo.open && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			//continue playing
			yield return new WaitForSeconds(1 / gameManager.gameSpeed);
		}
		//stop playing

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	IEnumerator Learn(Info TargetInfo)
	{
		//start learning 
		bool atTarget = true;
		while (gameManager.night && TargetInfo.open && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			//continue learning
			//chicken.hoursStudied += 1;

			yield return new WaitForSeconds(60 / gameManager.gameSpeed);
		}
		//stop learning

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	// transfer resource from chicken to place
	IEnumerator Put(Info TargetInfo)
	{

		bool chickenHasResource = true;
		bool targetHasRoom = true;
		bool atTarget = true;


		while (chickenHasResource && targetHasRoom && !gameManager.night && TargetInfo.open && atTarget)
		{

			if (totalCarried <= 0)
			{
				chickenHasResource = false;
			}
			if (chickenHasResource && TargetInfo.inventory[(int)(Resource)resourceCarried].x >= TargetInfo.inventory[(int)(Resource)resourceCarried].y)
			{
				targetHasRoom = false;
			}
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}


			if (chickenHasResource && targetHasRoom)
			{
				TargetInfo.AddRemove(resourceCarried, 1); // add one to target
				totalCarried--; // remoce one from chicken
			}

			yield return new WaitForSeconds(.25f / gameManager.gameSpeed);
		}

		if (totalCarried < 1)
		{
			resourceCarried = Resource.None;
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	//transfer resource from place to chicken
	IEnumerator Get(Info TargetInfo)
	{
		bool chickenHasRoom = true;
		bool targetHasResource = true;
		bool stillNeedSome = true;
		bool atTarget = true;


		while (TargetInfo != null && chickenHasRoom && targetHasResource && stillNeedSome && !gameManager.night && TargetInfo.open && atTarget)
		{

			if (totalCarried >= maxCarry)
			{
				chickenHasRoom = false;
			}
			if (TargetInfo.inventory[(int)(Resource)resourceNeeded].x < 1)
			{
				targetHasResource = false;
			}
			if (chicken.totalCarried >= amountNeeded)
			{
				stillNeedSome = false;
			}
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			if (chickenHasRoom && targetHasResource && stillNeedSome)
			{
				resourceCarried = resourceNeeded; // set what we're carrying to what we're picking up
				totalCarried++; // give on to chicken
				TargetInfo.AddRemove(resourceNeeded, -1); // remove one from target
			}
			yield return new WaitForSeconds(.25f / gameManager.gameSpeed);
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	// check for materials, convert resource to building buildPercent or integrity at place
	IEnumerator Make(Info TargetInfo)
	{
		bool hasIngredients = true;
		bool hasSpace = true;
		bool atTarget = true;

		while (hasIngredients && hasSpace && !gameManager.night && TargetInfo.open && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}
			if (totalCarried + TargetInfo.quantityProduced <= maxCarry) // if we have room
			{
				for (int y = 0; y < TargetInfo.actionIngredients.Count; y++)
				{
					if (TargetInfo.inventory[y].x < TargetInfo.actionIngredients[y] && hasIngredients)
					{
						hasIngredients = false;
						int amountNeeded = TargetInfo.actionIngredients[y] - (int)TargetInfo.inventory[y].x;

						if (totalCarried > 0)
						{
							if (resourceCarried == (Resource)y)
							{
								taskCreator.DropOffResources(chicken, TargetInfo.gameObject); // put the materials here
								amountNeeded -= totalCarried;
							}
							else
							{
								taskCreator.DropOffResources(chicken, null); // put the materials somwhere
							}
						}

						if (amountNeeded > 0)
						{
							if (taskCreator.GetResource(chicken, (Resource)y, null)) // get the first material we come across that we dont have, in the amount that we need
							{
								taskCreator.DropOffResources(chicken, TargetInfo.gameObject); // put the materials here
							}
						}
					}
				}

				if (hasIngredients)
				{
					yield return new WaitForSeconds(TargetInfo.makeTime / gameManager.gameSpeed); // wait for as long as one action takes

					for (int y = 0; y < TargetInfo.actionIngredients.Count; y++)
					{
						TargetInfo.AddRemove((Resource)y, -TargetInfo.actionIngredients[y]);
					}

					totalCarried += TargetInfo.quantityProduced; // give one to chicken
					resourceCarried = TargetInfo.resourceProduced;
				}
			}
			else //if we dont have room
			{
				hasSpace = false;
			}
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}


	void ChopWood(Info TargetInfo)
	{
		int numTrees = 0;
		float radius = 5;
		List<GameObject> trees = new List<GameObject> { };
		List<GameObject> logs = new List<GameObject> { };

		LayerMask layer = LayerMask.GetMask("Buildings", "Trees");
		Collider[] hitColliders = Physics.OverlapSphere(TargetInfo.transform.position, radius, layer, QueryTriggerInteraction.Collide);
		for (int x = 0; x < hitColliders.Length; x++)
		{
			if (hitColliders[x].tag == "Target")
			{
				Info _TargetInfo = hitColliders[x].GetComponent<Info>();
				if (_TargetInfo.targetType == TargetType.Tree || _TargetInfo.targetType == TargetType.Sapling || _TargetInfo.targetType == TargetType.Wood)
				{
					TreeStuff tree;
					tree = hitColliders[x].GetComponent<TreeStuff>();

					if (_TargetInfo.targetType == TargetType.Tree)
					{
						numTrees++;

						_TargetInfo.parentTarget = TargetInfo.gameObject;

						if (tree.treeGrowth >= 100)
						{
							trees.Add(hitColliders[x].transform.gameObject);
						}
					}
					if (_TargetInfo.targetType == TargetType.Sapling || _TargetInfo.targetType == TargetType.SaplingPlaceHolder)
					{
						numTrees++;
					}
					if (_TargetInfo.targetType == TargetType.Wood)
					{
						logs.Add(hitColliders[x].transform.gameObject);
					}
				}
			}
		}

		if (logs.Count > 0)
		{
			taskCreator.GetResource(chicken, Resource.Wood, logs[0]);
		}
		else if (numTrees < 25)
		{
			bool placeHolderFound = false;
			LayerMask layer3 = LayerMask.GetMask("Trees");
			Collider[] _hitColliders = Physics.OverlapSphere(TargetInfo.transform.position, radius, layer3, QueryTriggerInteraction.Collide);
			for (int x = 0; x < _hitColliders.Length && !placeHolderFound; x++)
			{
				if (_hitColliders[x].tag == "Target")
				{
					Info TreeInfo = _hitColliders[x].gameObject.GetComponent<Info>();
					if (TreeInfo.targetType == TargetType.SaplingPlaceHolder)
					{
						placeHolderFound = true;
						taskCreator.PlantTree(chicken);
					}
				}
			}
			if (!placeHolderFound)
			{
				if (!SetTreePlaceHolder(TargetInfo, radius))
				{
					taskCreator.ChopTree(chicken);
				}
			}
		}
		else
		{
			taskCreator.ChopTree(chicken);
		}

		//TODO: right now, this wont work right becaue chicken that work at trees are no longer working at the forster, rip.
		//maybe, if you're a tree that's within the radius of a forester, you are working at the forester?
		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	bool SetTreePlaceHolder(Info TargetInfo, float radius)
	{
		bool spotFound = false;
		Vector3 orgin = TargetInfo.transform.position;
		Vector3 spot = Vector3.zero;
		for (int x = 0; x < 30 && !spotFound; x++)
		{
			bool badSpot = false;
			Vector3 _spot = new Vector3(Random.Range(orgin.x - radius, orgin.x + radius), orgin.y, Random.Range(orgin.z - radius, orgin.z + radius));

			LayerMask layer = LayerMask.GetMask("Buildings");
			Collider[] hitColliders = Physics.OverlapSphere(_spot, 1.2f, layer);
			for (int y = 0; y < hitColliders.Length && !badSpot; y++)
			{
				badSpot = true;
				Debug.Log("Bad spot for a tree!");
			}

			Collider[] hitColliders2 = Physics.OverlapSphere(_spot, .75f);
			for (int y = 0; y < hitColliders2.Length && !badSpot; y++)
			{
				if (hitColliders2[y].tag != "Ground")
				{
					badSpot = true;
					Debug.Log("Bad spot for a tree!");
				}
			}

			if (!badSpot)
			{
				spotFound = true;
				spot = _spot;
			}
		}

		if (spot != Vector3.zero)
		{
			GameObject placeHolder;
			placeHolder = Instantiate(resources.treePlaceHolder, spot, Quaternion.Euler(Vector3.zero));
			taskCreator.PlantTree(chicken);
			return true;
		}
		else
		{
			return false;
		}
	}

	void PlantTree(Info TargetInfo)
	{
		Destroy(TargetInfo.gameObject);
		GameObject sapling;
		sapling = Instantiate(resources.sapling, TargetInfo.transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	IEnumerator ChopTree(Info TargetInfo)
	{
		bool atTarget = true;
		TreeStuff tree = TargetInfo.GetComponent<TreeStuff>();

		while (tree.treeHP > 0 && !gameManager.night && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			tree.treeHP--;
			yield return new WaitForSeconds(tree.chopTime / 100 / gameManager.gameSpeed); // wait for as long as one action takes
		}

		if (tree.treeHP <= 0 && !gameManager.night)
		{
			taskCreator.GetResource(chicken, Resource.Wood, TargetInfo.gameObject);
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}


	IEnumerator Build(Info TargetInfo)
	{
		bool hasMaterials = true;
		bool constructionComplete = false;
		bool atTarget = true;

		while (hasMaterials && !constructionComplete && !gameManager.night && TargetInfo.open && atTarget)
		{
			if ((chicken.transform.position - TargetInfo.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			for (int y = 0; y < TargetInfo.constructionMaterials.Count; y++)
			{
				if (TargetInfo.inventory[y].x < TargetInfo.constructionMaterials[y] && hasMaterials)
				{
					hasMaterials = false;
					int amountNeeded = TargetInfo.constructionMaterials[y] - (int)TargetInfo.inventory[y].x;

					if (totalCarried > 0)
					{
						if (resourceCarried == (Resource)y)
						{
							taskCreator.DropOffResources(chicken, TargetInfo.gameObject); // put the materials here
							amountNeeded -= totalCarried;
						}
						else
						{
							taskCreator.DropOffResources(chicken, null); // put the materials somwhere
						}
					}

					if (amountNeeded > 0)
					{
						if (taskCreator.GetResource(chicken, (Resource)y, null, amountNeeded)) // get the first material we come across that we dont have, in the amount that we need
						{
							taskCreator.DropOffResources(chicken, TargetInfo.gameObject); // put the materials here
						}
					}
				}
			}

			if (hasMaterials && !constructionComplete)
			{
				yield return new WaitForSeconds(TargetInfo.constructionTime / 100 / gameManager.gameSpeed); // wait for as long as one action takes

				if (TargetInfo.constructionPercent < 100)
				{
					TargetInfo.constructionPercent++;
				}
				else
				{
					constructionComplete = true;
					TargetInfo.constructionComplete = true;

					// remove all construction materials from completed building
					for (int y = 0; y < TargetInfo.constructionMaterials.Count; y++)
					{
						TargetInfo.AddRemove((Resource)y, -TargetInfo.constructionMaterials[y]);
					}
				}
			}
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	void RemoveReservation()
	{
		for (int x = 0; x < taskCreator.targets.Count; x++)
		{
			Info TargetInfo = taskCreator.targets[x].GetComponent<Info>();
			if (TargetInfo.chickens.Contains(chicken))
			{
				TargetInfo.chickens.Remove(chicken);
			}
		}
	}

}

