using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BRTemplate : MonoBehaviour {

	public Slider masteryMeter;
	public Slider loadSlider;
	public float loadDelay = 0.5f;
	public float timeSinceLoad;

	protected string directoryForAssignment;
	protected bool useImages;
	protected bool readyToConfigure;
	protected int currIndex, assignIndex;
	protected string[] contentForAssign;
	protected int totalMastery;
	protected int currMastery = 0;
	protected float timeAtStart;
	protected float timeAtEnd;

	public int requiredMastery = 4;



	//UI
	public GameObject winningSlide;


	public void configureGame (int index)
	{
		assignIndex = index;
		Assignment assignToUse = AppManager.s_instance.currentAssignments [assignIndex];
		useImages = assignToUse.hasImages;
		if (useImages) {
			directoryForAssignment = assignToUse.imgDir;
		}
		contentForAssign = assignToUse.content;
		currMastery = AppManager.s_instance.pullAssignMastery (assignToUse);
		readyToConfigure = true;
	}
}
