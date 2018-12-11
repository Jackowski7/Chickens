using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

	List<Vector3> nodeArray;
	List<GameObject> nodes;
	public List<Tile> tiles = new List<Tile> { };


	public bool Testing;

	public GameObject node;
	Transform tilesFolder;


	public int _mapSize;
	private int mapSize;
	public int seed;
	public float scale = 1.0F;

	public float iFall = 1f;
	public float iRate = 1f;

	public float fall = 1f;
	public float rate = 1f;

	public int terraces = 5;

	float xCorrection;

	//preview stuff
	private Texture2D noiseTex;
	private Color[] pix;
	private Renderer rend;

	//island things
	float randomXOffset1;
	float randomYOffset1;
	float randomXOffset2;
	float randomYOffset2;
	float randomXOffset3;
	float randomYOffset3;
	float randomXOffset4;
	float randomYOffset4;
	float randomXOffset5;
	float randomYOffset5;

	void Start()
	{

		tilesFolder = GameObject.Find("World/TileFolder").transform;

		if (seed == 0)
			seed = Random.RandomRange(0, 234);


		mapSize = _mapSize * 2;
		xCorrection = _mapSize * 1.156f;

		// NoisePreview Stuff
		rend = GetComponent<Renderer>();
		noiseTex = new Texture2D(mapSize, mapSize);
		pix = new Color[mapSize * mapSize];
		rend.material.mainTexture = noiseTex;
		NoisePreview();

		nodeArray = GenerateNodes();

		randomXOffset1 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomYOffset1 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomXOffset2 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomYOffset2 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomXOffset3 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomYOffset3 = (Random.Range(-_mapSize / 2, _mapSize / 2));
		randomXOffset4 = (Random.Range(-_mapSize / 1, _mapSize / 1));
		randomYOffset4 = (Random.Range(-_mapSize / 1, _mapSize / 1));
		randomXOffset5 = (Random.Range(-_mapSize / 1, _mapSize / 1));
		randomYOffset5 = (Random.Range(-_mapSize / 1, _mapSize / 1));

		if (!Testing)
			StartCoroutine(PlaceTiles());
	}

	void Update()
	{
		if (Testing)
		{
			NoisePreview();
			rend.enabled = true;
		}
		else
		{
			rend.enabled = false;
		}
	}


	void NoisePreview()
	{
		List<Vector3> pixelArray = new List<Vector3> { };

		float y = 0.0F;
		while (y < mapSize)
		{
			float x = 0.0F;
			while (x < mapSize)
			{
				float xCoord = x / mapSize * scale;
				float yCoord = y / mapSize * scale;
				float height = Mathf.PerlinNoise(seed + xCoord, seed + yCoord);

				//islands
				Vector2 center = new Vector2(_mapSize, _mapSize);
				Vector2 island1 = new Vector2(x - randomXOffset1, y - randomYOffset1);
				Vector2 island2 = new Vector2(x - randomXOffset2, y - randomYOffset2);
				Vector2 island3 = new Vector2(x - randomXOffset3, y - randomYOffset3);
				Vector2 island4 = new Vector2(x - randomXOffset4, y - randomYOffset4);
				Vector2 island5 = new Vector2(x - randomXOffset5, y - randomYOffset4);

				float island = (center - island1).magnitude;

				if ((center - island2).magnitude < island)
				{
					island = (center - island2).magnitude;
				}
				if ((center - island3).magnitude < island)
				{
					island = (center - island3).magnitude;
				}
				if ((center - island4).magnitude < island)
				{
					island = (center - island4).magnitude;
				}
				if ((center - island5).magnitude < island)
				{
					island = (center - island5).magnitude;
				}


				//falloff
				Vector2 pos = new Vector2(x, y);
				float distance = (pos - center).magnitude;

				height = height - Mathf.Sqrt(island / iRate);
				float droppOff = Mathf.Abs(Mathf.Sqrt(distance / rate));

				height -= Mathf.Clamp(Mathf.Abs(droppOff), 0, 2);

				//apply texture
				pixelArray.Add(new Vector3(x, height, y));
				x++;
			}
			y++;
		}


		// find highest spot for normalization
		float highestHeight = 0;
		foreach (Vector3 pixel in pixelArray)
		{
			float height = pixel.y;
			if (height > highestHeight)
			{
				highestHeight = height;
				//Debug.Log(highestHeight);
			}
		}
		// find lowest spot for normalization
		float lowestHeight = 0;
		foreach (Vector3 pixel in pixelArray)
		{
			float height = pixel.y;
			if (height < lowestHeight)
			{
				lowestHeight = height;
			}
		}

		foreach (Vector3 pixel in pixelArray)
		{

			int _x = (int)pixel.x;
			int _y = (int)pixel.z;
			float height = pixel.y;

			//normalize to one
			height = (height - lowestHeight) / (highestHeight - lowestHeight);

			//round for terraces
			height = Mathf.Round(height * terraces) / terraces;

			pix[(int)_y * mapSize + (int)_x] = new Color(height, height, height);
		}

		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}

	List<Vector3> GenerateNodes()
	{
		List<Vector3> nodeArray = new List<Vector3> { };

		float y = 0.0F;
		while (y < mapSize)
		{
			float x = 0.0F;
			while (x < mapSize)
			{

				float xCoord = x / mapSize * scale;
				float yCoord = y / mapSize * scale;
				float height = Mathf.PerlinNoise(seed + xCoord, seed + yCoord);

				//islands
				Vector2 center = new Vector2(_mapSize, _mapSize);
				Vector2 island1 = new Vector2(x - randomXOffset1, y - randomYOffset1);
				Vector2 island2 = new Vector2(x - randomXOffset2, y - randomYOffset2);
				Vector2 island3 = new Vector2(x - randomXOffset3, y - randomYOffset3);
				Vector2 island4 = new Vector2(x - randomXOffset4, y - randomYOffset4);
				Vector2 island5 = new Vector2(x - randomXOffset5, y - randomYOffset4);

				float island = (center - island1).magnitude;

				if ((center - island2).magnitude < island)
				{
					island = (center - island2).magnitude;
				}
				if ((center - island3).magnitude < island)
				{
					island = (center - island3).magnitude;
				}
				if ((center - island4).magnitude < island)
				{
					island = (center - island4).magnitude;
				}
				if ((center - island5).magnitude < island)
				{
					island = (center - island5).magnitude;
				}

				//falloff
				Vector2 pos = new Vector2(x, y);
				float distance = (pos - center).magnitude;

				height = height - Mathf.Sqrt(island / iRate);
				float droppOff = Mathf.Abs(Mathf.Sqrt(distance / rate));

				height -= Mathf.Clamp(Mathf.Abs(droppOff), 0, 2);

				//set x and y pos
				float posX = x * 1.156f;
				float posY = y;
				if (y % 2 == 1)
				{
					posX += .58f;
				}

				nodeArray.Add(new Vector3(posX - xCorrection, height, posY - _mapSize));

				x++;
			}
			y++;
		}

		// find highest spot for normalization
		float highestHeight = 0;
		foreach (Vector3 node in nodeArray)
		{
			float height = node.y;
			if (height > highestHeight)
			{
				highestHeight = height;
			}
		}
		// find lowest spot for normalization
		float lowestHeight = 0;
		foreach (Vector3 node in nodeArray)
		{
			float height = node.y;
			if (height < lowestHeight)
			{
				lowestHeight = height;
			}
		}

		//modify
		for (int q = 0; q < nodeArray.Count; q++)
		{
			float posX = nodeArray[q].x;
			float posY = nodeArray[q].z;
			float height = nodeArray[q].y;

			//normalize to one
			height = (height - lowestHeight) / (highestHeight - lowestHeight);

			//round for levels
			height = Mathf.Round(height * terraces);

			nodeArray[q] = (new Vector3(posX, height, posY));

		}

		return nodeArray;

	}


	IEnumerator PlaceTiles()
	{
		for (int q = 0; q < nodeArray.Count; q++)
		{
			float level = nodeArray[q].y;
			level = Mathf.Round(level);

			GameObject _tile = Instantiate(node);
			_tile.transform.parent = tilesFolder;

			Tile tile = _tile.GetComponent<Tile>();
			tiles.Add(tile);
			tile.level = (int)level;
			tile.pos = nodeArray[q];

		}
		yield return new WaitForEndOfFrame();

		for (int x = 0; x < tiles.Count; x++)
		{
			if (tiles[x].SetEdges())
			{
				GameObject tile = tiles[x].gameObject;
				Destroy(tile);
			}
		}

		yield return new WaitForEndOfFrame();

		for (int x = 0; x < tiles.Count; x++)
		{
			if (tiles[x] != null)
			StartCoroutine(tiles[x].PlaceObjects());
		}

	}


}
