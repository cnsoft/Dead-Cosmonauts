using UnityEngine;
using System.Collections;

public class StatsScreen : uLink.MonoBehaviour {

	public GameObject statsParent;
	private bool statsActive;

	public tk2dTextMesh[] killsText;
	public tk2dTextMesh[] deathsText;
	public tk2dTextMesh[] nameText;
	public tk2dTextMesh[] rankingText;
	public tk2dTextMesh[] rankingTextSuffix;
	public UITexture[] avatarTexture;

	public Color firstColor;

	public void ToggleStats()
	{
		statsActive = !statsActive;
		statsParent.SetActive(statsActive);
	}

	void SuffixCalculations()
	{
		for (int i = 0; i < rankingText.Length; i++)
		{
			if (rankingText[i].text == "1")
			{
				rankingTextSuffix[i].text = "st";
			}else if (rankingText[i].text == "2")
			{
				rankingTextSuffix[i].text = "nd";
			}else if (rankingText[i].text == "3")
			{
				rankingTextSuffix[i].text = "rd";
			}else
			{
				rankingTextSuffix[i].text = "th";
			}
		}
	}
}

