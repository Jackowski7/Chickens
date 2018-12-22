using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalDecoration : MonoBehaviour
{

	GameManager gameManager;
	public Season season;
	Season currentSeason;

	bool transitioning;

	// Start is called before the first frame update
	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	// Update is called once per frame
	void Update()
	{
		if (!transitioning)
		{
			currentSeason = gameManager.season;

			if (currentSeason == season)
			{
				StartCoroutine(BeginSeason());
			}
			if (currentSeason != season)
			{
				StartCoroutine(EndSeason());
			}
		}
	}

	IEnumerator BeginSeason()
	{
		transitioning = true;

		float rate = .04f;
		yield return new WaitForSeconds(Random.Range(0, 2f));

		Vector3 targetScale = Vector3.one;

		while (transform.localScale.y < .995f)
		{
			Vector3 currentScale = transform.localScale;

			transform.localScale = Vector3.Lerp(currentScale, targetScale, rate);
			yield return new WaitForEndOfFrame();
		}

		transitioning = false;
	}

	IEnumerator EndSeason()
	{
		transitioning = true;

		float rate = .4f;
		yield return new WaitForSeconds(Random.Range(0, 2f));

		Vector3 targetScale = new Vector3(.8f, 0, .8f);

		while (transform.localScale.y > .005f)
		{
			Vector3 currentScale = transform.localScale;

			transform.localScale = Vector3.Lerp(currentScale, targetScale, rate);
			yield return new WaitForEndOfFrame();
		}

		transitioning = false;
	}


}
