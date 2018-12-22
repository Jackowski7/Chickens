using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Chicken : MonoBehaviour
{

	GameManager gameManager;
	ChickenManager chickenManager;
	TaskCreator taskCreator;
	LoadedResources loadedResources;
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
	public GameObject currentTarget;
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
		loadedResources = gameManager.gameObject.GetComponent<LoadedResources>();
		chicken = GetComponent<Chicken>();
		ai = GetComponent<AIPath>();

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

		if (targetQueue.Count > 0 && currentTarget == null)
		{
			taskCreator.GetJobQueue(chicken); // your currentTarget exploded.. 
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
			currentTarget = targetQueue[0];
			action = actionQueue[0];
			resourceNeeded = resourceNeededQueue[0];
			amountNeeded = amountNeededQueue[0];

			if (currentTarget != null)
			{
				jobTaskComplete = false;
				if (currentTarget != this.gameObject)
				{
					Target target = currentTarget.GetComponent<Target>();
					StartCoroutine(GoToTarget(target)); //start navigation to the currentTarget
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


	// navigate to the currentTarget, if you take too long, you're probably stuck, teleport out. once you get there, do the task we're here to do.
	IEnumerator GoToTarget(Target target)
	{
		if (currentTarget != null)
		{
			int timeout = 10;
			while (target.chickens.Count >= target.maxSpots && currentTarget != null && timeout >= 0) // while this place is full, wait until it's not full
			{
				timeout--;
				yield return new WaitForSeconds(1 / gameManager.gameSpeed);
			}
			if (timeout > 0 && currentTarget != null)
			{

				if (target != null)
				{
					target.chickens.Add(chicken); // reserve a space 
				}
				if (target.parentTarget != null)
				{
					Target parentTargetInfo = target.parentTarget.GetComponent<Target>();
					parentTargetInfo.chickens.Add(chicken);
				}

				ai.destination = currentTarget.transform.position;
				ai.SearchPath();


				float timeStarted = gameManager.time;
				while (currentTarget != null && ((transform.position - currentTarget.transform.position).sqrMagnitude) > 1f) // while we're not at the currentTarget yet, adn the currentTarget does still exist
				{
					if (gameManager.time - timeStarted > 500) // if it's been 500 'time' (minutes), then lets' teleport there // this number will change later idk what it should be
					{
						if (currentTarget != null)
						{
							transform.position = currentTarget.transform.position;
							Debug.Log("I was stuck and teleported");
						}
						else // if the currentTarget doesn't exist, it blew up or something, start from scratch and get a whole new job
						{
							jobTaskComplete = true;
							RemoveReservation();
							Debug.Log("ay the currentTarget exploded while i was on the way?");
						}
					}
					yield return new WaitForSeconds(1 / gameManager.gameSpeed);
				}

				if (resourceNeeded != Resource.None) // if we're trying to get
				{
					//Debug.Log(chicken.name + " is going to " + action.ToString() + " " + resourceNeeded.ToString() + " at " + currentTarget.name);
				}
				else
				{
					//Debug.Log(chicken.name + " is going to " + action.ToString());
				}

				if (currentTarget != null)
				{
					DoAction(target);
				}
				else
				{
					Debug.Log("ay the currentTarget exploded while i was on the way?");
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
	void DoAction(Target target)
	{
		if (currentTarget != null)
		{
			if (action == Action.Sleep)
			{
				StartCoroutine(Sleep(target));
			}

			if (action == Action.Put)
			{
				StartCoroutine(Put(target));
			}

			if (action == Action.Get)
			{
				StartCoroutine(Get(target));
			}

			if (action == Action.Make)
			{
				StartCoroutine(Make(target));
			}

			if (action == Action.Build)
			{
				StartCoroutine(Build(target));
			}

			if (action == Action.Forester)
			{
				ChopWood(target);
			}

			if (action == Action.ChopTree)
			{
				StartCoroutine(ChopTree(target));
			}

			if (action == Action.PlantTree)
			{
				PlantTree(target);
			}

			if (action == Action.Wander)
			{
				StartCoroutine(Wander());
			}

			/*
			if (action == Action.Learn)
			{
				StartCoroutine(Learn(target));
			}*/

		}
		else // if the currentTarget doesn't exist, it blew up or something, start from scratch and get a whole new job
		{
			taskCreator.GetJobQueue(chicken);
			Debug.Log("currentTarget blew up before I could do the job");
		}
	}

	//go to sleep until energy is full
	IEnumerator Sleep(Target target)
	{
		bool atTarget = true;
		while (gameManager.night && target.open && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
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

	IEnumerator Play(Target target)
	{
		//start playing 
		bool atTarget = true;
		while (gameManager.night && target.open && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
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

	IEnumerator Learn(Target target)
	{
		//start learning 
		bool atTarget = true;
		while (gameManager.night && target.open && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
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
	IEnumerator Put(Target target)
	{

		bool chickenHasResource = true;
		bool targetHasRoom = true;
		bool atTarget = true;


		while (chickenHasResource && targetHasRoom && !gameManager.night && target.open && atTarget)
		{

			if (totalCarried <= 0)
			{
				chickenHasResource = false;
			}
			if (chickenHasResource && target.inventory[(int)(Resource)resourceCarried].x >= target.inventory[(int)(Resource)resourceCarried].y)
			{
				targetHasRoom = false;
			}
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}


			if (chickenHasResource && targetHasRoom)
			{
				target.AddRemove(resourceCarried, 1); // add one to currentTarget
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
	IEnumerator Get(Target target)
	{
		bool chickenHasRoom = true;
		bool targetHasResource = true;
		bool stillNeedSome = true;
		bool atTarget = true;


		while (target != null && chickenHasRoom && targetHasResource && stillNeedSome && !gameManager.night && target.open && atTarget)
		{

			if (totalCarried >= maxCarry)
			{
				chickenHasRoom = false;
			}
			if (target.inventory[(int)(Resource)resourceNeeded].x < 1)
			{
				targetHasResource = false;
			}
			if (chicken.totalCarried >= amountNeeded)
			{
				stillNeedSome = false;
			}
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			if (chickenHasRoom && targetHasResource && stillNeedSome)
			{
				resourceCarried = resourceNeeded; // set what we're carrying to what we're picking up
				totalCarried++; // give on to chicken
				target.AddRemove(resourceNeeded, -1); // remove one from currentTarget
			}
			yield return new WaitForSeconds(.25f / gameManager.gameSpeed);
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	// check for materials, convert resource to building buildPercent or integrity at place
	IEnumerator Make(Target target)
	{
		bool hasIngredients = true;
		bool hasSpace = true;
		bool atTarget = true;

		while (hasIngredients && hasSpace && !gameManager.night && target.open && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}
			if (totalCarried + target.quantityProduced <= maxCarry) // if we have room
			{
				for (int y = 0; y < target.actionIngredients.Count; y++)
				{
					if (target.inventory[y].x < target.actionIngredients[y] && hasIngredients)
					{
						hasIngredients = false;
						int amountNeeded = target.actionIngredients[y] - (int)target.inventory[y].x;

						if (totalCarried > 0)
						{
							if (resourceCarried == (Resource)y)
							{
								taskCreator.DropOffResources(chicken, target.gameObject); // put the materials here
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
								taskCreator.DropOffResources(chicken, target.gameObject); // put the materials here
							}
						}
					}
				}

				if (hasIngredients)
				{
					yield return new WaitForSeconds(target.makeTime / gameManager.gameSpeed); // wait for as long as one action takes

					for (int y = 0; y < target.actionIngredients.Count; y++)
					{
						target.AddRemove((Resource)y, -target.actionIngredients[y]);
					}

					totalCarried += target.quantityProduced; // give one to chicken
					resourceCarried = target.resourceProduced;
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


	void ChopWood(Target target)
	{
		int numTrees = 0;
		float radius = 5;
		List<GameObject> trees = new List<GameObject> { };
		List<GameObject> logs = new List<GameObject> { };

		LayerMask layer = LayerMask.GetMask("Buildings", "Trees");
		Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, radius, layer, QueryTriggerInteraction.Collide);
		for (int x = 0; x < hitColliders.Length; x++)
		{
			if (hitColliders[x].tag == "Target")
			{
				Target _TargetInfo = hitColliders[x].GetComponent<Target>();
				if (_TargetInfo.targetType == TargetType.Tree || _TargetInfo.targetType == TargetType.Sapling || _TargetInfo.targetType == TargetType.Wood)
				{
					TreeStuff tree;
					tree = hitColliders[x].GetComponent<TreeStuff>();

					if (_TargetInfo.targetType == TargetType.Tree)
					{
						numTrees++;

						_TargetInfo.parentTarget = target.gameObject;

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
			Collider[] _hitColliders = Physics.OverlapSphere(target.transform.position, radius, layer3, QueryTriggerInteraction.Collide);
			for (int x = 0; x < _hitColliders.Length && !placeHolderFound; x++)
			{
				if (_hitColliders[x].tag == "Target")
				{
					Target TreeInfo = _hitColliders[x].gameObject.GetComponent<Target>();
					if (TreeInfo.targetType == TargetType.SaplingPlaceHolder)
					{
						placeHolderFound = true;
						taskCreator.PlantTree(chicken);
					}
				}
			}
			if (!placeHolderFound)
			{
				if (!SetTreePlaceHolder(target, radius))
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

	bool SetTreePlaceHolder(Target target, float radius)
	{
		bool spotFound = false;
		Vector3 orgin = target.transform.position;
		Vector3 spot = Vector3.zero;
		for (int x = 0; x < 30 && !spotFound; x++)
		{
			bool badSpot = false;
			Vector3 _spot = new Vector3(Random.Range(orgin.x - radius, orgin.x + radius), orgin.y, Random.Range(orgin.z - radius, orgin.z + radius));

			LayerMask groundLayer = LayerMask.GetMask("Ground");
			Collider[] closeNodes = Physics.OverlapSphere(_spot, 3f, groundLayer, QueryTriggerInteraction.Collide);

			Transform closest = null;
			float shortestDistance = 1000f;
			for (int y = 0; y < closeNodes.Length; y++)
			{
				float distance = (closeNodes[x].transform.position - _spot).magnitude;
				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					closest = closeNodes[x].transform;
				}
			}

			Vector3 rayCastSpot = Vector3.zero;

			if (closest != null)
			{
				rayCastSpot = closest.position;
				rayCastSpot.y += 1f;
				_spot = closest.position;
			}

			LayerMask nodeLayer = LayerMask.GetMask("Node", "Ground");
			RaycastHit[] nodesHit = Physics.RaycastAll(rayCastSpot, Vector3.down, 4f, nodeLayer, QueryTriggerInteraction.Collide);

			for (int y = 0; y < nodesHit.Length; y++)
			{
				if (nodesHit[y].transform.tag != "Ground")
				{
					badSpot = true;
				}
			}

			if (!badSpot)
			{
				spotFound = true;
				spot = _spot;
			}
		}

		if (spotFound)
		{
			GameObject placeHolder;
			placeHolder = Instantiate(loadedResources.treePlaceHolder, spot, Quaternion.Euler(Vector3.zero));
			taskCreator.PlantTree(chicken);
			return true;
		}
		else
		{
			return false;
		}
	}

	void PlantTree(Target target)
	{
		Destroy(target.gameObject);

		int random = Random.Range(0, 5);
		Vector3 randomRot = new Vector3(0, random * 60, 0);

		GameObject Tree;
		Tree = Instantiate(loadedResources.sapling, target.transform.position, Quaternion.Euler(randomRot));
		TreeStuff treeStuff = Tree.GetComponent<TreeStuff>();
		treeStuff.SetSapling();

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}

	IEnumerator ChopTree(Target target)
	{
		bool atTarget = true;
		TreeStuff tree = target.GetComponent<TreeStuff>();

		while (tree.treeHP > 0 && !gameManager.night && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			tree.treeHP--;
			yield return new WaitForSeconds(tree.chopTime / 100 / gameManager.gameSpeed); // wait for as long as one action takes
		}

		if (tree.treeHP <= 0 && !gameManager.night)
		{
			taskCreator.GetResource(chicken, Resource.Wood, target.gameObject);
		}

		RemoveReservation(); // free up a space 
		jobTaskComplete = true; // set job task complete, so we get next job in queue
	}


	IEnumerator Build(Target target)
	{
		Construction construction = target.GetComponent<Construction>();

		bool hasMaterials = true;
		bool constructionComplete = false;
		bool atTarget = true;

		while (hasMaterials && !constructionComplete && !gameManager.night && target.open && atTarget)
		{
			if ((chicken.transform.position - target.transform.position).sqrMagnitude > 1f)
			{
				atTarget = false;
			}

			for (int y = 0; y < construction.constructionMaterials.Count; y++)
			{
				if (target.inventory[y].x < construction.constructionMaterials[y] && hasMaterials)
				{
					hasMaterials = false;
					int amountNeeded = construction.constructionMaterials[y] - (int)target.inventory[y].x;

					if (totalCarried > 0)
					{
						if (resourceCarried == (Resource)y)
						{
							taskCreator.DropOffResources(chicken, target.gameObject); // put the materials here
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
							taskCreator.DropOffResources(chicken, target.gameObject); // put the materials here
						}
					}
				}
			}

			if (hasMaterials && !constructionComplete)
			{
				yield return new WaitForSeconds(construction.constructionTime / 100 / gameManager.gameSpeed); // wait for as long as one action takes

				if (construction.constructionPercent < 100)
				{
					construction.constructionPercent++;
				}
				else
				{
					constructionComplete = true;

					// remove all construction materials from completed building
					for (int y = 0; y < construction.constructionMaterials.Count; y++)
					{
						target.AddRemove((Resource)y, -construction.constructionMaterials[y]);
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
			Target target = taskCreator.targets[x].GetComponent<Target>();
			if (target.chickens.Contains(chicken))
			{
				target.chickens.Remove(chicken);
			}
		}
	}

	public void Place()
	{
		chickenManager.NewChicken(chicken); // log us in the chickenmanager database and assign a job/status/home etc.
		this.GetComponent<Rigidbody>().useGravity = true;

		int numNodes = transform.Find("Nodes").childCount;
		for (int x = 0; x < numNodes; x++)
		{
			// deactivate renderer on node
			transform.Find("Nodes").GetChild(x).GetChild(0).gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		chickenManager.chickens.Remove(gameObject); // remove ourselves to the list of targets in TaskCreator
	}

}

