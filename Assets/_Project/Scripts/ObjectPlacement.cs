using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacement : MonoBehaviour
{

	public Material placementMaterial;
	public Material invalidMat;
	public Material validMat;

	public Camera cam;

	bool placingObject = false;

	bool rotate;
	bool cancel;
	bool place;
	bool readyToPlace = false;
	bool readyToRotate = true;

	// Start is called before the first frame update
	void Start()
	{
	}

	public void PlaceObject(GameObject newObject)
	{
		if (!placingObject)
		{
			StartCoroutine(_PlaceObject(newObject));
		}
	}

	private void Update()
	{
		rotate = false;
		cancel = false;
		place = false;

		if (Input.GetButtonDown("Rotate"))
		{
			rotate = true;
		}


		if (Input.GetButtonDown("Cancel"))
		{
			cancel = true;
		}


		if (Input.GetButtonDown("Submit") && readyToPlace)
		{
			place = true;
		}

	}

	IEnumerator _PlaceObject(GameObject newObject)
	{
		placingObject = true;

		int random = Random.Range(0, 5);
		Vector3 randomRot = new Vector3(0, random * 60, 0);

		GameObject _object = Instantiate(newObject, Vector3.zero, Quaternion.Euler(randomRot));

		List<Collider> cols = new List<Collider>() { };
		//cols.Add(_object.GetComponent<Collider>());

		Collider[] _cols = _object.GetComponentsInChildren<Collider>();

		foreach (Collider col in _cols)
		{
			cols.Add(col);
		}

		foreach (Collider col in cols)
		{
			col.enabled = false;
		}

		Renderer[] rends = _object.GetComponentsInChildren<Renderer>();

		List<List<Material>> ogMatsList = new List<List<Material>>() { };
		List<List<Material>> matsList = new List<List<Material>>() { };

		foreach (Renderer rend in rends)
		{
			List<Material> ogMats = new List<Material> { };
			List<Material> mats = new List<Material> { };

			for (int x = 0; x < rend.materials.Length; x++)
			{
				ogMats.Add(rend.materials[x]);
				mats.Add(placementMaterial);
			}

			ogMatsList.Add(ogMats);
			matsList.Add(mats);

			rend.materials = mats.ToArray();
		}


		yield return new WaitForEndOfFrame();
		bool placed = false;
		bool canceled = false;
		readyToPlace = true;

		while (!placed && !canceled)
		{
			Vector3 point = Vector3.zero;

			LayerMask groundLayer = LayerMask.GetMask("Ground");

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				point = hit.transform.position;
				point.y = 2f;
			}

			Collider[] closeNodes = Physics.OverlapSphere(point, 1f, groundLayer, QueryTriggerInteraction.Collide);

			Transform closest = null;
			float shortestDistance = 1000f;
			for (int x = 0; x < closeNodes.Length; x++)
			{
				float distance = (closeNodes[x].transform.position - point).magnitude;
				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					closest = closeNodes[x].transform;
				}
			}

			if (closest != null)
			{
				_object.transform.position = closest.position;
			}
			else
			{
				_object.transform.position = new Vector3(2000, 2000, 2000); // way away
			}

			bool validPosition = false;

			int numNodes = _object.transform.Find("Nodes").childCount;

			if (readyToRotate)
			{
				validPosition = true;

				for (int x = 0; x < numNodes; x++)
				{
					Transform node = _object.transform.Find("Nodes").GetChild(x);
					node.GetChild(0).gameObject.SetActive(false);

					Vector3 pos = node.transform.position;
					pos.y += 2f;


					if (!Physics.Raycast(pos, Vector3.down, 4f, groundLayer, QueryTriggerInteraction.Collide))
					{
						validPosition = false;
						node.GetChild(0).GetComponent<Renderer>().material = invalidMat;
						node.GetChild(0).gameObject.SetActive(true);
					}
					else if (x == 0)
					{
						node.GetChild(0).GetComponent<Renderer>().material = validMat;
						node.GetChild(0).gameObject.SetActive(true);
					}

					RaycastHit _hit;
					LayerMask nodeLayer = LayerMask.GetMask("Node");
					if (Physics.Raycast(pos, Vector3.down, out _hit, 4f, nodeLayer, QueryTriggerInteraction.Collide))
					{
						if (_hit.transform.tag == "EdgeNode")
						{
							if (node.tag != "EdgeNode")
							{
								validPosition = false;
								node.GetChild(0).GetComponent<Renderer>().material = invalidMat;
								node.GetChild(0).gameObject.SetActive(true);
							}
						}
						if (_hit.transform.tag == "Node")
						{
							validPosition = false;
							node.GetChild(0).GetComponent<Renderer>().material = invalidMat;
							node.GetChild(0).gameObject.SetActive(true);
						}
					}
					else if (x == 0)
					{
						node.GetChild(0).GetComponent<Renderer>().material = validMat;
						node.GetChild(0).gameObject.SetActive(true);
					}
				}
			}
			else
			{
				for (int x = 0; x < numNodes; x++)
				{
					Transform node = _object.transform.Find("Nodes").GetChild(x);
					node.GetChild(0).gameObject.SetActive(false);
				}
			}


			if (rotate && readyToRotate)
			{
				readyToRotate = false;
				StartCoroutine(Rotate(_object));
			}

			if (cancel)
			{
				canceled = true;
			}

			if (place)
			{
				if (validPosition)
					placed = true;
			}

			yield return new WaitForEndOfFrame();
		}

		if (placed)
		{
			foreach (Collider col in cols)
			{
				col.enabled = true;
			}

			//set materials back to original
			for (int x = 0; x < rends.Length; x++)
			{
				rends[x].materials = ogMatsList[x].ToArray();
			}

			if (_object.GetComponent<Target>())
				_object.GetComponent<Target>().Place();

			if (_object.GetComponent<Chicken>())
				_object.GetComponent<Chicken>().Place();

		}
		else
		{
			Destroy(_object);
		}

		placingObject = false;
		readyToPlace = false;

	}

	IEnumerator Rotate(GameObject _object)
	{

		int timeout = 20;
		Vector3 currentRot = _object.transform.rotation.eulerAngles;
		Vector3 targetRot = new Vector3(currentRot.x, currentRot.y + 60, currentRot.z);
		while (timeout > 0)
		{
			_object.transform.rotation = Quaternion.Lerp(_object.transform.rotation, Quaternion.Euler(targetRot), .2f);
			timeout--;
			yield return new WaitForEndOfFrame();
		}

		_object.transform.rotation = Quaternion.Euler(targetRot);
		readyToRotate = true;

		yield return new WaitForEndOfFrame();
		//this isnt working
	}

}
