using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalDecoration : MonoBehaviour
{

	GameManager gameManager;
	Season _season;
	Season seasonSetter;

	public List<GameObject> springDecor = new List<GameObject> { };
	public List<GameObject> summerDecor = new List<GameObject> { };
	public List<GameObject> fallDecor = new List<GameObject> { };
	public List<GameObject> winterDecor = new List<GameObject> { };

	// Start is called before the first frame update
	void Start()
	{
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

	}

	// Update is called once per frame
	void Update()
	{
		_season = gameManager.season;

		if (_season != seasonSetter)
		{
			seasonSetter = _season;
			StartCoroutine(SetSeason(_season));
		}

	}

	IEnumerator SetSeason(Season season)
	{
		float inRate = .04f;
		float outRate = .06f;

		while (season == seasonSetter)
		{
			for (int x = 0; x < springDecor.Count; x++)
			{
				GameObject element = springDecor[x];
				Vector3 currentScale = element.transform.localScale;
				Vector3 targetScale = currentScale;

				Color currentColor = element.GetComponent<Renderer>().material.color;
				Color targetColor = currentColor;

				if (season == Season.Spring)
				{
					targetScale.y = 1;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, inRate);

					targetColor.a = 1;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, inRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
				else
				{
					targetScale.y = 0;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, outRate);

					targetColor.a = 0;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, outRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
			}

			for (int x = 0; x < summerDecor.Count; x++)
			{
				GameObject element = summerDecor[x];
				Vector3 currentScale = element.transform.localScale;
				Vector3 targetScale = currentScale;

				Color currentColor = element.GetComponent<Renderer>().material.color;
				Color targetColor = currentColor;

				if (season == Season.Summer)
				{
					targetScale.y = 1;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, inRate);

					targetColor.a = 1;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, inRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
				else
				{
					targetScale.y = 0;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, outRate);

					targetColor.a = 0;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, outRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
			}

			for (int x = 0; x < fallDecor.Count; x++)
			{
				GameObject element = fallDecor[x];
				Vector3 currentScale = element.transform.localScale;
				Vector3 targetScale = currentScale;

				Color currentColor = element.GetComponent<Renderer>().material.color;
				Color targetColor = currentColor;

				if (season == Season.Fall)
				{
					targetScale.y = 1;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, inRate);

					targetColor.a = 1;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, inRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
				else
				{
					targetScale.y = 0;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, outRate);

					targetColor.a = 0;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, outRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
			}

			for (int x = 0; x < winterDecor.Count; x++)
			{
				GameObject element = winterDecor[x];
				Vector3 currentScale = element.transform.localScale;
				Vector3 targetScale = currentScale;

				Color currentColor = element.GetComponent<Renderer>().material.color;
				Color targetColor = currentColor;

				if (season == Season.Winter)
				{
					targetScale.y = 1;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, inRate);

					targetColor.a = 1;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, inRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
				else
				{
					targetScale.y = 0;
					element.transform.localScale = Vector3.Lerp(currentScale, targetScale, outRate);

					targetColor.a = 0;
					float alpha = Mathf.Lerp(currentColor.a, targetColor.a, outRate);
					targetColor.a = alpha;
					element.GetComponent<Renderer>().material.color = targetColor;
				}
			}

			yield return new WaitForEndOfFrame();
		}
	}
}
