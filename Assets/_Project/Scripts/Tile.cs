using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	Renderer rend;

	public int level;
	public Vector3 pos;


	public List<float> levelHeights = new List<float> { };
	public List<Material> levelMaterials = new List<Material> { };
	public List<float> levelTrees = new List<float> { };
	public List<float> levelRocks = new List<float> { };

	float height;
	Material material;

	public GameObject tree;
	public GameObject rock;

	bool edge = false;

	private void Start()
	{
		rend = GetComponentInChildren<Renderer>();

		for (int x = 0; x < levelHeights.Count; x++)
		{
			if (x == level)
			{
				height = levelHeights[x];
				rend.material = levelMaterials[x];
			}
		}

		SetHeight();

	}


	void SetHeight()
	{
		height = Random.Range(0f, .03f);

		transform.transform.position = new Vector3(pos.x, 1f, pos.z);

		if (level < 2)
		{
			transform.GetChild(0).transform.position = new Vector3(pos.x, height - .8f, pos.z);
		}

		transform.GetChild(0).transform.position = new Vector3(pos.x, height, pos.z);


	}

	public bool SetEdges()
	{
		LayerMask layer = LayerMask.GetMask("Ground");
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f, layer, QueryTriggerInteraction.Collide);
		for (int x = 0; x < hitColliders.Length; x++)
		{
			if (hitColliders[x].tag == "Ground")
			{
				Tile tile = hitColliders[x].transform.parent.gameObject.GetComponent<Tile>();

				if (tile.level > level)
				{
					float newHeight = ((tile.height - height) * .5f) + height;
					transform.GetChild(0).transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
					edge = true;
				}
			}
		}

		if ((level < 2 && !edge) || level < 1)
		{
			return true;
		}
		else
		{
			return false;
		}

	}

	public IEnumerator PlaceObjects()
	{
		if (!edge)
		{
			int random = Random.Range(0, 5);
			Vector3 randomRot = new Vector3(0, random * 60, 0);

			LayerMask nodeLayer = LayerMask.GetMask("Node");
			Vector3 raycastOrgin = transform.position;
			raycastOrgin.y += 2f;

			//spawn rocks
			for (int x = 0; x < levelRocks.Count; x++)
			{
				if (x == level)
				{
					if (Random.Range(0, 100) < levelRocks[x])
					{
						if (!Physics.Raycast(raycastOrgin, Vector3.down, 4f, nodeLayer, QueryTriggerInteraction.Collide))
						{
							Transform rockFolder = transform.parent.parent.Find("RockFolder").transform;
							GameObject o = Instantiate(rock, transform.position, Quaternion.Euler(randomRot), rockFolder);
							o.transform.localScale = o.transform.localScale * Random.Range(.8f, 1.2f); 
							o.GetComponent<Target>().Place();
						}
					}
				}
			}

			//spawn trees
			for (int x = 0; x < levelTrees.Count; x++)
			{
				if (x == level)
				{
					if (Random.Range(0, 100) < levelTrees[x])
					{
						if (!Physics.Raycast(raycastOrgin, Vector3.down, 4f, nodeLayer, QueryTriggerInteraction.Collide))
						{
							Transform treeFolder = transform.parent.parent.Find("TreeFolder").transform;
							GameObject Tree = Instantiate(tree, transform.position, Quaternion.Euler(randomRot), treeFolder);
							TreeStuff treeStuff = Tree.GetComponent<TreeStuff>();
							treeStuff.SetTree();
							treeStuff.Place();
						}
					}
				}
			}

			yield return new WaitForEndOfFrame();


		}
	}


}
