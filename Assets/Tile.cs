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
		height = Random.Range(1.0f, 1.03f);
		//height = 2f;

		//scale collider to standard size
		transform.GetChild(1).localScale = new Vector3(1, 1 / height, 1);

		if (level < 2)
		{
			height = 0.2f;
			transform.GetChild(1).localScale = new Vector3(1, 1 * height, 1);
		}

		transform.position = new Vector3(pos.x, height, pos.z);

		Vector3 scale = transform.localScale;
		transform.localScale = new Vector3(scale.x, height, scale.z);



	}

	public bool SetEdges()
	{
		LayerMask layer = LayerMask.GetMask("Ground");
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.25f, layer, QueryTriggerInteraction.Collide);
		for (int x = 0; x < hitColliders.Length; x++)
		{
			if (hitColliders[x].tag == "Ground")
			{
				Tile tile = hitColliders[x].transform.parent.gameObject.GetComponent<Tile>();

				if (tile.level > level)
				{
					float newHeight = (tile.height - height) + height * 1f;
					transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);

					Vector3 scale = transform.localScale;
					transform.localScale = new Vector3(scale.x, (tile.height - height) + height * .85f, scale.z);

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

	public void PlaceObjects()
	{
		if (!edge)
		{
			int random = Random.Range(0, 5);
			Vector3 randomRot = new Vector3(0, random * 60, 0);

			//spawn trees
			for (int x = 0; x < levelTrees.Count; x++)
			{
				if (x == level)
				{
					if (Random.Range(0, 100) < levelTrees[x])
					{
						Transform treeFolder = transform.parent.parent.Find("TreeFolder").transform;
						GameObject o = Instantiate(tree, transform.position, Quaternion.Euler(randomRot), treeFolder);
						o.GetComponent<Info>().Place();
					}
				}
			}

			//spawn rocks
			for (int x = 0; x < levelRocks.Count; x++)
			{
				if (x == level)
				{
					if (Random.Range(0, 100) < levelRocks[x])
					{
						Transform rockFolder = transform.parent.parent.Find("RockFolder").transform;
						GameObject o = Instantiate(rock, transform.position, Quaternion.Euler(randomRot), rockFolder);
						//o.GetComponent<Info>().Place();
					}
				}
			}

		}
	}


}
