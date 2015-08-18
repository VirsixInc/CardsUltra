using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public abstract class BRTemplate : MonoBehaviour {

	public Slider masteryMeter;
	public Slider loadSlider;
	public float loadDelay = 0.5f;
	public float timeSinceLoad;
	public int requiredMastery = 4;
	protected int totalMastery = 0;
	protected int currMastery = 0;
	protected int priorMastery;
	protected string directoryForAssignment;
	protected bool useImages;
	protected bool readyToConfigure = false;
	protected int currIndex=0; //iterator for the content array of a template
	protected int assignIndex; //the identification index of the current assignment we are playing
	protected int currentImageIterator;

	protected string[] contentForAssign;

	protected float timeAtStart;
	protected float timeAtEnd;
	
	//UI
	public GameObject winningSlide;
	public GameObject introSlide;




}
