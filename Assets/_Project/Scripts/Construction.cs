using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour
{

	GameManager gameManager;
	Target target;

	[Space(5)]
	[Header("Construction Settings")]

	[Tooltip("What is the current construction percent?")]
	[Range(0, 100)]
	public int constructionPercent = 0;
	bool constructionComplete = true;

	[Tooltip("How long does this place's construction take total?")]
	public float constructionTime;

	[Tooltip("What Resources are needed to build this place?\n" + "0=wheat\n" + "1=grain\n" + "2=wood\n" + "3=planks\n" + "4=stone\n" + "5=bricks")]
	public List<int> constructionMaterials = new List<int> { 0, 0, 0, 0, 0, 0 }; // #resources req'd to construct (wheat grain wood planks sonte bricks)


	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		target = gameObject.GetComponent<Target>();

		for (int x = 0; x < target.actions.Count; x++)
		{
			target._actions.Add(target.actions[x]); // log actions to reset later
		}
		for (int x = 0; x < target.inventory.Count; x++)
		{
			target._inventory[x] = target.inventory[x]; // log inventory to reset later
		}

		if (constructionPercent >= 100)
		{
			BuildComplete();
		}
		else
		{
			SetBuildable();
		}

	}

	void Update()
	{

		if (constructionPercent >= 100 && !constructionComplete)
		{
			BuildComplete();
		}

	}

	void SetBuildable()
	{
		constructionComplete = false;

		target.actions.Clear(); // remove actions
		target.actions.Add(Action.Build); // add build to actions
		for (int x = 0; x < target.inventory.Count; x++)
		{
			target.inventory[x] = new Vector2(target.inventory[x].x, constructionMaterials[x]); // set inventory capacity to building materials needed
		}
	}

	void BuildComplete()
	{
		constructionComplete = true;
		target.actions.Clear(); // remove build actions
		target.actions = target._actions; // reset original actions
		for (int x = 0; x < target.inventory.Count; x++)
		{
			target.inventory[x] = target._inventory[x]; // reset inventory to original state
		}
	}

}
