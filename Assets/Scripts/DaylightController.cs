using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaylightController : MonoBehaviour {

	public GameObject sun;
	public GameObject moon;

	GameManager gameManager;

	// Use this for initialization
	void Start () {

		gameManager = GetComponent<GameManager>();

	}
	
	// Update is called once per frame
	void Update () {

		sun.GetComponent<Light>().intensity = Mathf.Min(1.5f, 1.5f * gameManager.dayLight );
		moon.GetComponent<Light>().intensity = Mathf.Min(1, -1 + (1 - gameManager.dayLight) * 2 );

	}
}
