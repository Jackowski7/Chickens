using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

	public float year = 0;
	public float month = 0;
	public float day = 0;
	public float hour = 0;
	public float minute = 0;

	public float time = 0;
	public float timeSpeed = 1;

	public float dayLight;
	public string season;

	public bool paused;

	// Use this for initialization
	void Start()
	{
		StartCoroutine(Time());
	}

	// Update is called once per frame
	void Update()
	{
		minute = Mathf.Floor(time % 60);
		hour = Mathf.Floor(time / 60) % 24;
		day = Mathf.Floor(time / 60 / 24) % 30;
		month = Mathf.Floor(time / 60 / 24 / 30) % 12;
		year = Mathf.Floor(time / 60 / 24 / 30 / 12);
		dayLight = 1 - (Mathf.Abs(12 - ((time / 60) % 24)) / 12);

		if (month == 1 || month == 2 || month == 12)
		{
			season = "Winter";
		}
		if (month == 3 || month == 4 || month == 5)
		{
			season = "Spring";
		}
		if (month == 6 || month == 7 || month == 8)
		{
			season = "Summer";
		}
		if (month == 9 || month == 10 || month == 11)
		{
			season = "Autumn";
		}
	}

	IEnumerator Time()
	{
		while ("lol" == "lol")
		{
			if (paused != true)
			{
				time += 1 * timeSpeed;
			}
			yield return new WaitForSecondsRealtime(.1f);
		}
	}

	//make all chickens lose energy over time accrding to timespeed
}
