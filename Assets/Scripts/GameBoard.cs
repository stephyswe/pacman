using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

	private static int boardWidth = 28;
	private static int boardHeight = 36;

	private bool didStartDeath = false;

	public int totalPellets = 0;
	public int score = 0;
	public int pacManLives = 3;


	public AudioClip bgAudioNormal;
	public AudioClip bgAudioFrightened;
	public AudioClip bgAudioPacManDeath;

	public GameObject[,] board = new GameObject[boardWidth, boardHeight];

	// Use this for initialization
	void Start () {

		// Getting all Objects in the Scene
		Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));

		// Looping over all Objects
		foreach (GameObject o in objects)
		{
			// Get current position of the Object iterating over 
			Vector2 pos = o.transform.position;

			// Checks name of the Object that was Found - Pellets = Empty.
			if (o.name != "PacMan" && o.name != "Nodes" && o.name != "NonNodes" && o.name != "Maze" && o.name != "Pellets" && o.name != "bottom_left_corner_single" && o.name != "Ghost" && o.tag != "ghostHome") {
				
				// If Object is a Tile Component [ Pellet, Node or Portal ]
				if (o.GetComponent<Tile> () != null ) {

					// If Object is (Super) Pellet - Increase totalPellets by 1. 
					if (o.GetComponent<Tile> ().isPellet || o.GetComponent<Tile> ().isSupperPellet) {
						totalPellets++;
					}
				}

				// Adding Object to Board array at pos x and y where it occurs			
				board [(int)pos.x, (int)pos.y] = o;
			} else {
				//Debug.Log ("Found PacMan at: " +pos);
			}
		}
	}

	// If pacMan dies. Stop Ghost, Pacman and All Animation. 
	public void StartDeath () {
		if (!didStartDeath) {
			didStartDeath = true;
			GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
			foreach (GameObject ghost in o)
			{
				ghost.transform.GetComponent<Ghost> ().canMove = false;
			}

			GameObject pacMan = GameObject.Find ("PacMan");

			pacMan.transform.GetComponent<PacMan> ().canMove = false;
			pacMan.transform.GetComponent<Animator> ().enabled = false;
			transform.GetComponent<AudioSource> ().Stop ();
			StartCoroutine (ProcessDeathAfter (2));
		}
	}

	// Find and Make the Ghosts Invisible. 
	// Loop to IEnumerator ProcessDeathAnimation
	IEnumerator ProcessDeathAfter (float delay) {
		yield return new WaitForSeconds (delay);

		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<SpriteRenderer> ().enabled = false;
		}

		StartCoroutine (ProcessDeathAnimation (1.9f));
	}

	// Find PacMan and change Scale and Rotation to start Death Animation 
	// Play AudioClip and continue with IEnumerator ProcessRestart
	IEnumerator ProcessDeathAnimation (float delay) {
		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.localScale = new Vector3 (1, 1, 1);
		pacMan.transform.localRotation = Quaternion.Euler (0, 0, 0);

		pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan> ().deathAnimation;
		pacMan.transform.GetComponent<Animator> ().enabled = true;

		transform.GetComponent<AudioSource>().clip = bgAudioPacManDeath;
		transform.GetComponent<AudioSource>().Play ();

		yield return new WaitForSeconds (delay);

		StartCoroutine (ProcessRestart (2));			
		
	}

	// Find pacMan and Make him Invisible
	// Stop Audio and finish with a call to Restart method.  
	IEnumerator ProcessRestart (float delay) {
		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<SpriteRenderer> ().enabled = false;

		transform.GetComponent<AudioSource> ().Stop ();

		yield return new WaitForSeconds (delay);

		Restart ();
	}

	public void Restart () {
		pacManLives -= 1;
		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<PacMan> ().Restart ();

		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<Ghost> ().Restart ();
		}
		transform.GetComponent<AudioSource> ().clip = bgAudioNormal;
		transform.GetComponent<AudioSource> ().Play ();

		didStartDeath = false;
	}
}	
