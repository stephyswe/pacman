using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour {

	public AudioClip chomp1;
	public AudioClip chomp2;

	public RuntimeAnimatorController chompAnimation;
	public RuntimeAnimatorController deathAnimation;

	public Vector2 orientation;
	public float speed = 6.0f;
	public Sprite idleSprite;

	public bool canMove = true;

	private bool playedChomp1 = false;
	private AudioSource audio;


	private Vector2 direction = Vector2.zero;
	private Vector2 nextDirection;

	// Pac-Man current position 
	private Node currentNode, previousNode, targetNode;

	private Node startingPosition;

	// Use this for initialization
	void Start () {

		audio = transform.GetComponent<AudioSource> ();

		Node node = GetNodeAtPosition (transform.localPosition);

		startingPosition = node;

		// Find which Pellet PacMan is at 
		if (node != null) {
			currentNode = node;
		}

		direction = Vector2.left;
		orientation = Vector2.left;
		ChangePosition (direction);
	}

	public void MoveToStartingPosition () {
		transform.position = startingPosition.transform.position;
		transform.GetComponent<SpriteRenderer> ().sprite = idleSprite;
		direction = Vector2.left;
		orientation = Vector2.left;
		UpdateOrientation ();
	}

	// Reset Pac-Mans position
	public void Restart () {
		canMove = true;
		
		currentNode = startingPosition;
		nextDirection = Vector2.left;

		transform.GetComponent<Animator> ().runtimeAnimatorController = chompAnimation;
		transform.GetComponent<Animator> ().enabled = true;	

		ChangePosition (direction);
		
	}
	
	// Update is called once per frame
	void Update () {

		if (canMove) {
			checkInput ();
			Move ();
			UpdateOrientation ();
			UpdateAnimationState ();
			ConsumePellet ();
		}
	}

	void PlayChompSound () {
		if (playedChomp1) {
			//- Play chomp 2, set playedChomp1 to false;
			audio.PlayOneShot (chomp2);
			playedChomp1 = false;
		} else {
			//- Play chomp 1, set playedChomp1 to true;
			audio.PlayOneShot (chomp1);
			playedChomp1 = true;
		}
	}

    void checkInput () {
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			ChangePosition (Vector2.left);
		} else if ( Input.GetKeyDown (KeyCode.RightArrow)) {
			ChangePosition (Vector2.right);
		} else if ( Input.GetKeyDown (KeyCode.UpArrow)) {
			ChangePosition (Vector2.up);
		} else if ( Input.GetKeyDown (KeyCode.DownArrow)) {
			ChangePosition (Vector2.down);
		}
    }

	void ChangePosition (Vector2 d) {
		if (d != direction)
			nextDirection = d;
		if (currentNode != null) {
			Node moveToNode = CanMove (d);
			if (moveToNode != null) {
				direction = d;
				targetNode = moveToNode;
				previousNode = currentNode;
				currentNode = null;
			}
		}
	}

	void Move () {
		// Check Boundaries where PacMan can Move 
		if (targetNode != currentNode && targetNode != null) {

			// Move opposite way between Nodes 
			if (nextDirection == direction * -1) {
				direction *= -1;
				Node tempNode = targetNode;
				targetNode = previousNode;
				previousNode = tempNode;
			}

			if (OverShotTarget ()) {
				currentNode = targetNode;

				transform.localPosition = currentNode.transform.position;

				GameObject otherPortal = GetPortal (currentNode.transform.position);
				// If the Object is a Portal, and has value. 
				// Set Pacmans position to the other Portal. 
				if (otherPortal != null) {
					transform.localPosition = otherPortal.transform.position;

					// Moves PacMan from Portal to associated Node. 
					currentNode = otherPortal.GetComponent<Node> ();
				}

				Node moveToNode = CanMove (nextDirection);
				if (moveToNode != null)
					direction = nextDirection;

				if (moveToNode == null)
					moveToNode = CanMove (direction);

				if (moveToNode != null) {
					targetNode = moveToNode;
					previousNode = currentNode;
					currentNode = null;
				} else {
					direction = Vector2.zero;
				}
			} else {
				transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;
			}
		}
	}

	void MoveToNode (Vector2 d) {
		Node moveToNode = CanMove (d);
		if (moveToNode != null) {
			transform.localPosition = moveToNode.transform.position;
			currentNode = moveToNode;
		}
	}

	void UpdateOrientation () {
		if (direction == Vector2.left) {
			orientation = Vector2.left;
			transform.localScale = new Vector3 (-1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,0);

		} else if (direction == Vector2.right) {
			orientation = Vector2.right;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,0);

		} else if (direction == Vector2.up ) {
			orientation = Vector2.up;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,90);

		} else if (direction == Vector2.down ) {
			orientation = Vector2.down;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,270);
		}
	}

	void UpdateAnimationState() {
		if (direction == Vector2.zero) {
			GetComponent<Animator> ().enabled = false;
			GetComponent<SpriteRenderer> ().sprite = idleSprite;
		} else {
			GetComponent<Animator> ().enabled = true;
		}
	}

	void ConsumePellet () {
		GameObject o = GetTileAtPosition (transform.position);

		if (o != null) {
			Tile tile = o.GetComponent<Tile> ();

			// If tile hasn't been consumed
			if (tile != null) {

				// If super pellet or pellet, increment score by 1 and set consumed pellets to true. 
				if (!tile.didConsume && (tile.isPellet || tile.isSupperPellet)) {
					o.GetComponent<SpriteRenderer> ().enabled = false;
					tile.didConsume = true;

					if (GameMenu.isOnePlayerGame) {
						GameBoard.playerOneScore += 10;
						GameObject.Find("Game").transform.GetComponent<GameBoard> ().playerOnePelletsConsumed++;
					} else {
						if (GameObject.Find("Game").transform.GetComponent<GameBoard> ().isPlayerOneUp) {
							GameBoard.playerOneScore += 10;
							GameObject.Find("Game").transform.GetComponent<GameBoard> ().playerOnePelletsConsumed++;
						} else {
							GameBoard.playerTwoScore += 10;
							GameObject.Find("Game").transform.GetComponent<GameBoard> ().playerTwoPelletsConsumed++;
						}

					}

					PlayChompSound ();

					if (tile.isSupperPellet) {
						GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
						foreach (GameObject go in ghosts)
						{
							go.GetComponent<Ghost> ().StartFrightenedMode ();
						}
					}
				}
			} 
		}
	}

	// Checks if Pacman can CanMove, and set his position to neighbor node
	Node CanMove (Vector2 d) {
		Node moveToNode = null;
		for (int i = 0; i < currentNode.neighbors.Length; i++)
		{
			if (currentNode.validDirections [i] == d) {
				moveToNode = currentNode.neighbors [i];
				break;
			}
		}
		return moveToNode;
	}

	// Get tile of pacMan current position
	GameObject GetTileAtPosition (Vector2 pos) {
		int tileX = Mathf.RoundToInt (pos.x);
		int tileY = Mathf.RoundToInt (pos.y);

		GameObject tile = GameObject.Find ("Game").GetComponent<GameBoard> ().board [tileX, tileY];
		if (tile != null) {
			return tile;
		}
		return null;
	}

	Node GetNodeAtPosition (Vector2 pos) {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board [(int)pos.x, (int)pos.y];
		if (tile != null) {
			return tile.GetComponent<Node> ();
		}
		return null;
	}
	// checks if pacman moves away from nodes allowed directions.
	bool OverShotTarget () {
		float nodeToTarget = LengthFromNode (targetNode.transform.position);
		float nodetoSelf = LengthFromNode (transform.localPosition);

		return nodetoSelf > nodeToTarget;
	}

	float LengthFromNode (Vector2 targetPosition) {
		Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
		return vec.sqrMagnitude;
	}


	GameObject GetPortal (Vector2 pos) {
		GameObject tile = GameObject.Find ("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
		if (tile != null) {

			// GameObject has Tile Component and is portal
			if (tile.GetComponent<Tile> () != null && tile.GetComponent<Tile> ().isPortal) {

				// use tile.portalreceiver and return other portal
				GameObject otherPortal = tile.GetComponent<Tile> ().portalReceiver;
				return otherPortal;

			}
		}
		return null;
	}
}
