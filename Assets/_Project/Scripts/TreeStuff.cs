using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeStuff : MonoBehaviour
{

	GameManager gameManager;
	Target target;

	[Header("Tree Stuff")]
	[Tooltip("How grown is this tree?")]
	[Range(0, 100)]
	public int treeGrowth;
	[Tooltip("How chopped down is this tree?")]
	[Range(0, 100)]
	public int treeHP;
	[Tooltip("How long does it take to chop down this tree total?")]
	public int chopTime;
	[Tooltip("How much wood is in this tree?")]
	public int woodAmount;


	private void Awake()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		target = gameObject.GetComponent<Target>();

		//color tree
		//Renderer rend = transform.Find("Tree/Leaves").GetComponent<Renderer>();
		Renderer rend = transform.Find("Tree").GetChild(0).GetComponent<Renderer>();
		Color color = rend.materials[1].color;
		color.r = Mathf.Clamp(Random.Range(color.r - .18f, color.r + .18f), 0, 255);
		color.g = Mathf.Clamp(Random.Range(color.g - .18f, color.g + .18f), 0, 255);
		color.b = Mathf.Clamp(Random.Range(color.b - .08f, color.b + .08f), 0, 255);
		rend.materials[1].color = color;


		//disable by default
		transform.Find("Tree").gameObject.SetActive(false);
		transform.Find("Logs").gameObject.SetActive(false);
		//transform.Find("Sapling").gameObject.SetActive(false);
		transform.Find("Snow").gameObject.SetActive(false);
	}

	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		target = gameObject.GetComponent<Target>();
	}

	private void Update()
	{
		if (target.targetType == TargetType.Tree && treeGrowth >= 100 && treeHP <= 0) // once this tree (grown) has been chopped
		{
			CutDownTree();
		}

		if (target.targetType == TargetType.Wood && target.inventory[2].x <= 0) // once this pile of wood has all been picked up
		{
			Destroy(this.gameObject);
		}
	}

	public void SetSapling()
	{
		StartCoroutine(GrowTree());
		Place();
	}

	public IEnumerator GrowTree()
	{
		name = "Sapling";

		treeGrowth = 0;
		transform.Find("Tree").gameObject.SetActive(true);

		while (treeGrowth < 100)
		{
			treeGrowth++;
			transform.Find("Tree").gameObject.transform.localScale = Vector3.one * treeGrowth / 100;

			yield return new WaitForSeconds(1 / gameManager.gameSpeed);

		}
		SetTree();
	}

	public void SetTree()
	{
		transform.localScale = Vector3.one;
		treeHP = 100;

		target.actions.Clear(); // remove actions
		target.actions.Add(Action.ChopTree); // add chop tree to actions
		target.targetType = TargetType.Tree;
		transform.Find("Tree").gameObject.SetActive(true);
		transform.Find("Snow").gameObject.SetActive(true);

		for (int x = 0; x < target.inventory.Count; x++)
		{
			target.inventory[x] = new Vector2(0, 0); // set inventory to 0's
		}
	}

	public void CutDownTree()
	{
		target.actions.Clear(); // remove build actions
		target.targetType = TargetType.Wood;
		target.actions.Add(Action.Get);
		target.inventory[2] = new Vector2(woodAmount, 0); // set wood amount to woodAmount
		transform.Find("Tree").gameObject.SetActive(false);
		transform.Find("Logs").gameObject.SetActive(true);
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
}
