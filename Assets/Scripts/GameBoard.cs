using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour {

	private static int boardWidth = 28;
	private static int boardHeight = 36;

	private bool didStartDeath = false;
	private bool didStartConsumed = false;

	public int totalPellets = 0;
	public int score = 0;
	public int playerOneScore = 0;
	public int playerTwoScore = 0;
	public int pacManLives = 3;

	public bool isPlayerOneUp = true;

	public AudioClip bgAudioNormal;
	public AudioClip bgAudioFrightened;
	public AudioClip bgAudioPacManDeath;
	public AudioClip consumedGhostAudioClip;

	public Text playerText;
	public Text readyText;

	public Text highScoreText;
	public Text playerOneUp;
	public Text playerTwoUp;
	public Text playerOneScoreText;
	public Text playerTwoScoreText;
	public Image playerLives2;
	public Image playerLives3;

	public Text consumedGhostScoreText;


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
			if (o.name != "PacMan" && o.name != "Nodes" && o.name != "NonNodes" && o.name != "Maze" && o.name != "Pellets" && o.name != "bottom_left_corner_single" && o.name != "Ghost" && o.tag != "ghostHome" && o.name != "Canvas" && o.tag != "UIElements") {
				
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

		StartGame ();
	}

	void Update () {
		UpdateUI ();

	}

	void UpdateUI () {
		playerOneScoreText.text = playerOneScore.ToString ();
		playerTwoScoreText.text = playerTwoScore.ToString ();

		if (pacManLives == 3) {
			playerLives3.enabled = true;
			playerLives2.enabled = true;

		} else if (pacManLives == 2) {
			playerLives3.enabled = false;
			playerLives2.enabled = true;


		} else if (pacManLives == 1) {
			playerLives3.enabled = false;
			playerLives2.enabled = false;
		}
	}

	public void StartGame () {

		if (GameMenu.isOnePlayerGame) {
			playerTwoUp.GetComponent<Text> ().enabled = false;
			playerTwoScoreText.GetComponent<Text> ().enabled = false;
		} else {
			playerTwoUp.GetComponent<Text> ().enabled = true;
			playerTwoScoreText.GetComponent<Text> ().enabled = true;
		}

		if (isPlayerOneUp) {
			StartCoroutine(StartBlinking (playerOneUp));
		} else {
			StartCoroutine(StartBlinking (playerTwoUp));
		}

		//- Hide All Ghosts
		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<SpriteRenderer> ().enabled = false;
			ghost.transform.GetComponent<Ghost> ().canMove = false;
		}

		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<SpriteRenderer> ().enabled = false;
		pacMan.transform.GetComponent<PacMan> ().canMove = false;

		StartCoroutine (ShowObjectAfter (2.25f));

	}

	public void StartConsumed (Ghost consumedGhost) {
		if (!didStartConsumed) {
			didStartConsumed = true;
			//- Pause all the ghosts
			GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
			foreach (GameObject ghost in o)
			{
				ghost.transform.GetComponent<Ghost> ().canMove = false;
			}

			//- Pause Pac-Man
			GameObject pacMan = GameObject.Find ("PacMan");
			pacMan.transform.GetComponent<PacMan> ().canMove = false;

			//- Hide Pac-Man
			pacMan.transform.GetComponent<SpriteRenderer> ().enabled = false;

			//- Hide the consumed ghost
			consumedGhost.transform.GetComponent<SpriteRenderer> ().enabled = false;

			//- Stop background Music
			transform.GetComponent<AudioSource> ().Stop ();

			//- Convert Canvas space to Level cordinate space.
			Vector2 pos = consumedGhost.transform.position;
			Vector2 viewPortPoint = Camera.main.WorldToViewportPoint (pos);
			consumedGhostScoreText.GetComponent<RectTransform> ().anchorMin = viewPortPoint;
			consumedGhostScoreText.GetComponent<RectTransform> ().anchorMax = viewPortPoint;
			consumedGhostScoreText.GetComponent<Text> ().enabled = true;

			//- Play the consumed Sound
			transform.GetComponent<AudioSource> ().PlayOneShot (consumedGhostAudioClip);

			//- Wait for the audio clip to finish
			StartCoroutine (ProcessConsumedAfter (0.75f, consumedGhost));
		}
	}

	// Blink Text each quarter a Second. 
	IEnumerator StartBlinking (Text blinkText) {
		yield return new WaitForSeconds (0.25f);
		blinkText.GetComponent<Text> ().enabled = !blinkText.GetComponent<Text> ().enabled;
		StartCoroutine(StartBlinking (blinkText));
	}

	IEnumerator ProcessConsumedAfter (float delay, Ghost consumedGhost) {
		yield return new WaitForSeconds (delay);
		//- Hide the score
		consumedGhostScoreText.GetComponent<Text> ().enabled = false;

		//- Show Pac-Man
		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<SpriteRenderer> ().enabled = true;

		//- Show Consumed Ghost
		consumedGhost.transform.GetComponent<SpriteRenderer> ().enabled = true;

		//- Resume all ghosts
		GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<Ghost> ().canMove = true;
		}

		//- Resume Pac-Man
		pacMan.transform.GetComponent<PacMan> ().canMove = true;

		//- Start Background Music
		transform.GetComponent<AudioSource> ().Play ();

		didStartConsumed = false;

	}

	// Hide ghosts, disable pacman animation and make him visible. 
	// hide player text and after 2 seconds goto IE - StartGameAfter
	IEnumerator ShowObjectAfter (float delay) {

		yield return new WaitForSeconds (delay);

		//- Hide All Ghosts
		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<SpriteRenderer> ().enabled = true;
		}

		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<Animator> ().enabled = false;
		pacMan.transform.GetComponent<SpriteRenderer> ().enabled = true;

		playerText.transform.GetComponent<Text> ().enabled = false;

		StartCoroutine (StartGameAfter (2));

	}

	// ghosts can move, enable pacman animation and make him moveable.
	// hide ready text and play background music
	IEnumerator StartGameAfter (float delay) {

		yield return new WaitForSeconds (delay);

		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<Ghost> ().canMove = true;
		}

		GameObject pacMan = GameObject.Find ("PacMan");
		pacMan.transform.GetComponent<Animator> ().enabled = true;
		pacMan.transform.GetComponent<PacMan> ().canMove = true;

		readyText.transform.GetComponent<Text> ().enabled = false;

		transform.GetComponent<AudioSource> ().clip = bgAudioNormal;
		transform.GetComponent<AudioSource> ().Play ();
	}

	// When pacMan dies. Stop ghost, pacman and all animation 
	public void StartDeath () {
		if (!didStartDeath) {

			// Stops Blinking the 1UP/2UP Text 
			StopAllCoroutines ();
			if (GameMenu.isOnePlayerGame) {
				playerOneUp.GetComponent<Text> ().enabled = true;
			} else {
				playerOneUp.GetComponent<Text> ().enabled = true;
				playerTwoUp.GetComponent<Text> ().enabled = true;
			}

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

	// make ghosts invisible, loop to IEnumerator ProcessDeathAnimation
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

		StartCoroutine (ProcessRestart (1));			
		
	}

	// (1S): lose one life, player text / ready text is visible
	// pacman is invisible, stops audio and call IE - ProcessRestartShowObjects.  
	IEnumerator ProcessRestart (float delay) {
		pacManLives -= 1;

		if (pacManLives == 0) {
			playerText.transform.GetComponent<Text> ().enabled = true;
			readyText.transform.GetComponent<Text> ().text ="GAME OVER";
			readyText.transform.GetComponent<Text> ().color = Color.red;

			readyText.transform.GetComponent<Text> ().enabled = true;

			GameObject pacMan = GameObject.Find ("PacMan");
			pacMan.transform.GetComponent<SpriteRenderer> ().enabled = false;

			transform.GetComponent<AudioSource> ().Stop ();

			StartCoroutine (ProcessGameOver (2));

		} else {
			playerText.transform.GetComponent<Text> ().enabled = true;
			readyText.transform.GetComponent<Text> ().enabled = true;

			GameObject pacMan = GameObject.Find ("PacMan");
			pacMan.transform.GetComponent<SpriteRenderer> ().enabled = false;

			transform.GetComponent<AudioSource> ().Stop ();

			yield return new WaitForSeconds (delay);

			StartCoroutine (ProcessRestartShowObjects (1));
		}
	}

	IEnumerator ProcessGameOver (float delay) {
		yield return new WaitForSeconds (delay);

		SceneManager.LoadScene ("GameMenu");
	}

	// Hide player text. ghosts are visible and moveable
	// pacman is visible, stop animation and reset his position, then call restart method.
	IEnumerator ProcessRestartShowObjects (float delay) {
		playerText.transform.GetComponent<Text> ().enabled = false;
		GameObject [] o = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject ghost in o)
		{
			ghost.transform.GetComponent<SpriteRenderer> ().enabled = true;
			ghost.transform.GetComponent<Ghost> ().MoveToStartingPosition ();

		}

		GameObject pacMan = GameObject.Find ("PacMan");

		pacMan.transform.GetComponent<Animator> ().enabled = false;
		pacMan.transform.GetComponent<SpriteRenderer> ().enabled = true;
		pacMan.transform.GetComponent<PacMan> ().MoveToStartingPosition ();

		yield return new WaitForSeconds (delay);

		Restart ();
	}

	// hide ready text. decrease one life, use pacMan restart method
	// Use ghost restart method, play background music, disable didstartdeath. 
	public void Restart () {

		readyText.transform.GetComponent<Text> ().enabled = false;

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
