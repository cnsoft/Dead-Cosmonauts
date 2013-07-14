using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public void UpdateStats (List<Player.Stat> stats)
    {
        int[] sortOrder = new int[] {2,1,0,3,4};

        stats.Sort ((a,b) => b.Kills - a.Kills);

        for (int j = 0; j < stats.Count; j++) {
            int i = sortOrder [j];
            nameText [i].text = stats [j].name;
            nameText [i].Commit ();
            killsText[i].text = string.Format("KILLS\n{0}",stats [j].Kills);
            killsText [i].Commit ();
            deathsText[i].text = string.Format("DEATHS\n{0}",stats [j].Deaths);
            deathsText [i].Commit ();
        }

    }
}

