using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
	GameManager gameManager;

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
	[Tooltip("This place's current Inventory (current, max)\n" + "0=wheat\n" + "1=grain\n" + "2=wood\n" + "3=planks\n" + "4=stone\n" + "5=bricks")]
	public List<Vector2> inventory = new List<Vector2> { new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250), new Vector2(0, 250) };
	[Tooltip("Actions that can be done at this place")]
	public List<Action> actions = new List<Action> { };

	[Space(5)]
	[Header("Tree Stuff")]
	[Tooltip("Is this treefully grown?")]
	public bool treeGrown;
	[Tooltip("How grown is this tree?")]
	[Range(0, 100)]
	public int treeGrowth;
	[Tooltip("How chopped down is this tree?")]
	[Range(0, 100)]
	public int treeHP;
	[Tooltip("How long does it take to chop down this tree total?")]
	public int chopTime;

	[Space(5)]
	[Tooltip("If this target belongs to a parent target, this is that")]
	public GameObject parentTarget;

	// Start is called before the first frame update
	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();


		if (targetType == TargetType.Sapling)
		{
			StartCoroutine(GrowTree());
		}
		if (targetType == TargetType.Tree)
		{
			SetTree();
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (targetType == TargetType.Tree && treeGrown && treeHP <= 0) // once this tree (grown) has been chopped
		{
			CutDownTree();
		}

		if (targetType == TargetType.Wood && inventory[2].x <= 0) // once this pile of wood has all been picked up
		{
			Destroy(this.gameObject);
		}
	}

	IEnumerator GrowTree()
	{
		name = "Sapling";
		transform.Find("Model/TreeModel").gameObject.SetActive(false);
		transform.Find("Model/LogsModel").gameObject.SetActive(false);
		transform.Find("Model/SaplingModel").gameObject.SetActive(true);

		while (treeGrowth < 100)
		{
			treeGrowth++;
			transform.Find("Model/SaplingModel").transform.localScale = Vector3.one * treeGrowth / 100;

			yield return new WaitForSeconds(1 / gameManager.gameSpeed);

		}
		SetTree();
	}

	void SetTree()
	{
		treeGrown = true;
		name = "Tree";
		actions.Clear(); // remove actions
		actions.Add(Action.ChopTree); // add chop tree to actions
		targetType = TargetType.Tree;
		transform.Find("Model/TreeModel").gameObject.SetActive(true);
		transform.Find("Model/LogsModel").gameObject.SetActive(false);
		transform.Find("Model/SaplingModel").gameObject.SetActive(false);

		Renderer rend = transform.Find("Model/TreeModel").GetComponent<Renderer>();
		Color color = rend.material.color;
		color.r = Mathf.Clamp(Random.Range(color.r - .08f, color.r + .08f), 0, 255);
		color.g = Mathf.Clamp(Random.Range(color.g - .08f, color.g + .08f), 0, 255);
		color.b = Mathf.Clamp(Random.Range(color.b - .08f, color.b + .08f), 0, 255);
		rend.material.color = color;

		for (int x = 0; x < inventory.Count; x++)
		{
			inventory[x] = new Vector2(0, 0); // set inventory to 0's
		}
	}

	void CutDownTree()
	{
		actions.Clear(); // remove build actions
		targetType = TargetType.Wood;
		actions.Add(Action.Get);
		inventory[2] = new Vector2(25, 0); // set wood amount to 25
		transform.Find("Model/TreeModel").gameObject.SetActive(false);
		transform.Find("Model/LogsModel").gameObject.SetActive(true);
		transform.Find("Model/SaplingModel").gameObject.SetActive(false);
	}

}
