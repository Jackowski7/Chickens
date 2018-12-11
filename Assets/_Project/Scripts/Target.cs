using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

	GameManager gameManager;
	Resources resources;
	TaskCreator taskCreator;
	TreeStuff tree;

	[Space(5)]
	[Header("Target Information")]
	[Tooltip("What Kind of Target is this?")]
	public TargetType targetType;
	[Tooltip("Is this place open for business?")]
	public bool open;
	[Tooltip("How many chickens can work /use this place at once?")]
	public int maxSpots;
	[Tooltip("Chickens actively using this place")]
	public List<Chicken> chickens = new List<Chicken> { };


	[Space(5)]
	[Header("Inventory")]
	[Tooltip("This place's current Inventory (current, max)\n" + "0=wheat\n" + "1=grain\n" + "2=wood\n" + "3=planks\n" + "4=stone\n" + "5=bricks")]
	public List<Vector2> inventory = new List<Vector2> { new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250) };


	[Space(5)]
	[Header("Action/Production Settings")]
	[Tooltip("Actions that can be done at this place")]
	public List<Action> actions = new List<Action> { };
	[Space(10)]
	[Tooltip("What Resource (if any) can you 'Make' here?")]
	public Resource resourceProduced;
	[Tooltip("How many of this resource is made per 'Make'?")]
	public int quantityProduced;
	[Tooltip("How Long does each 'Make' action take?")]
	public float makeTime;
	[Tooltip("What Resources are needed to 'Make' one thing here?\n" + "0=wheat\n" + "1=grain\n" + "2=wood\n" + "3=planks\n" + "4=stone\n" + "5=bricks")]
	public List<int> actionIngredients = new List<int> { 0, 0, 0, 0, 0, 0 };


	[HideInInspector] // inventory holder for setting build mode
	public List<Vector2> _inventory = new List<Vector2> { new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250) };
	[HideInInspector] // action holder for setting build mode
	public List<Action> _actions = new List<Action> { };


	[Space(5)]
	[Tooltip("If this target belongs to a parent target, this is that")]
	public GameObject parentTarget;


	private void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		resources = gameManager.GetComponent<Resources>();
		taskCreator = gameManager.GetComponent<TaskCreator>();

		taskCreator.targets.Add(gameObject); // add ourselves to the list of targets in TaskCreator
	}

	public void Place()
	{
		int numNodes = transform.Find("Nodes").childCount;
		for (int x = 0; x < numNodes; x++)
		{
			// deactivate renderer on node
			transform.Find("Nodes").GetChild(x).GetChild(0).gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		taskCreator.targets.Remove(gameObject); // remove ourselves to the list of targets in TaskCreator
	}

	public void AddRemove(Resource resource, int amount)
	{
		int oldinv = (int)inventory[(int)(Resource)resource].x;
		int newval = (int)inventory[(int)(Resource)resource].x + amount;
		int maxroom = (int)inventory[(int)(Resource)resource].y;
		inventory[(int)(Resource)resource] = new Vector2(newval, maxroom);
	}

}
