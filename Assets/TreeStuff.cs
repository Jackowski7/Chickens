using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeStuff : MonoBehaviour
{

	GameManager gameManager;
	Info TreeInfo;

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

	// Start is called before the first frame update
	void Start()
    {
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		TreeInfo = gameObject.GetComponent<Info>();

		//color tree
		Renderer rend = transform.Find("GrownTree").GetComponent<Renderer>();
		Color color = rend.materials[1].color;
		color.r = Mathf.Clamp(Random.Range(color.r - .08f, color.r + .08f), 0, 255);
		color.g = Mathf.Clamp(Random.Range(color.g - .08f, color.g + .08f), 0, 255);
		color.b = Mathf.Clamp(Random.Range(color.b - .08f, color.b + .08f), 0, 255);
		rend.materials[1].color = color;
	}

	private void Update()
	{
		if (TreeInfo.targetType == TargetType.Tree && treeGrown && treeHP <= 0) // once this tree (grown) has been chopped
		{
			CutDownTree();
		}

		if (TreeInfo.targetType == TargetType.Wood && TreeInfo.inventory[2].x <= 0) // once this pile of wood has all been picked up
		{
			Destroy(this.gameObject);
		}
	}

	public IEnumerator GrowTree()
	{
		name = "Sapling";
		transform.Find("GrownTree").gameObject.SetActive(true);
		transform.Find("Logs").gameObject.SetActive(false);

		while (treeGrowth < 100)
		{
			treeGrowth++; 
			transform.localScale = Vector3.one * treeGrowth / 100;

			yield return new WaitForSeconds(1 / gameManager.gameSpeed);

		}
		SetTree();
	}

	public void SetTree()
	{
		treeGrown = true;

		transform.localScale = Vector3.one;
		treeHP = 100;

		TreeInfo.actions.Clear(); // remove actions
		TreeInfo.actions.Add(Action.ChopTree); // add chop tree to actions
		TreeInfo.targetType = TargetType.Tree;
		transform.Find("GrownTree").gameObject.SetActive(true);
		transform.Find("Logs").gameObject.SetActive(false);

		for (int x = 0; x < TreeInfo.inventory.Count; x++)
		{
			TreeInfo.inventory[x] = new Vector2(0, 0); // set inventory to 0's
		}
	}

	public void CutDownTree()
	{
		TreeInfo.actions.Clear(); // remove build actions
		TreeInfo.targetType = TargetType.Wood;
		TreeInfo.actions.Add(Action.Get);
		TreeInfo.inventory[2] = new Vector2(25, 0); // set wood amount to 25
		transform.Find("GrownTree").gameObject.SetActive(false);
		transform.Find("Logs").gameObject.SetActive(true);
	}
}
