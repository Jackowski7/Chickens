using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// chicken manager is responsible for managing the chickens' and their jobs as a whole
// this will assign jobs to chickens, and probably some other things like living situations, schooling, and whatever else

public class ChickenManager : MonoBehaviour
{

	GameManager gameManager;
	TaskCreator taskCreator;

	public List<GameObject> chickens = new List<GameObject> { };

	private void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		taskCreator = GameObject.Find("GameManager").GetComponent<TaskCreator>();
	}

	public void NewChicken(Chicken chicken)
	{
		chickens.Add(chicken.gameObject); // add chicken to the chickenmanager's list so we can use it
		UpdateChicken(chicken);
	}

	public void UpdateChicken(Chicken chicken)
	{
		//AgeChicken(chicken);
		//EducateChicken(chicken);
	}

	//check all chicken ages
	public void UpdateChickens()
	{
		foreach (GameObject _chicken in chickens)
		{
			Chicken chicken = _chicken.GetComponent<Chicken>();


			if (chicken.targetQueue.Count < 1)
			{
				taskCreator.GetJobQueue(chicken); // done! get a new list of tasks
			}
		}
	}



}
