using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class BRTemplate : MonoBehaviour {

	public Slider masteryMeter;
	public Slider loadSlider;
	public float loadDelay = 0.5f;
	public float timeSinceLoad;
	public int requiredMastery = 4;

	protected string directoryForAssignment;
	protected bool useImages;
	protected bool readyToConfigure;
	protected int currIndex, assignIndex;
	protected int currentImageIterator;

	protected string[] contentForAssign;
	protected int totalMastery;
	protected int currMastery = 0;
	protected float timeAtStart;
	protected float timeAtEnd;




	//UI
	public GameObject winningSlide;
	public GameObject introSlide;


}
