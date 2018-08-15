using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour {

	public AudioClip chomp1;
	public AudioClip chomp2;

	public Vector2 orientation;
	public float speed = 6.0f;
	public Sprite idleSprite;

	private bool playedChomp1 = false;
	private AudioSource audio;


	private Vector2 direction = Vector2.zero;
	private Vector2 nextDirection;

	private int pelletsConsumed = 0;

	// Store Pac-Man current Position 
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

	// Reset Pac-Mans position
	public void Restart () {
		transform.position = startingPosition.transform.position;

		currentNode = startingPosition;

		direction = Vector2.left;
		orientation = Vector2.left;
		nextDirection = Vector2.left;

		ChangePosition (direction);
	}
	
	// Update is called once per frame
	void Update () {

		//Debug.Log ("SCORE: " + GameObject.Find("Game").GetComponent<GameBoard> ().score);

        checkInput ();
		Move ();
		UpdateOrientation ();
		UpdateAnimationState ();
		ConsumePellet ();


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

		// Position is tile or pellet
		if (o != null) {
			Tile tile = o.GetComponent<Tile> ();

			// If GameObject(pellet) on Tile exists
			if (tile != null) {

				// And Superpellet / Pellet hasn't been consumed
				// Remove pellet sprite and set consumed to true.
				// Increment Score and pelletsConsumed by 1. 
				if (!tile.didConsume && (tile.isPellet || tile.isSupperPellet)) {
					o.GetComponent<SpriteRenderer> ().enabled = false;
					tile.didConsume = true;

					GameObject.Find ("Game").GetComponent<GameBoard> ().score += 1;
					pelletsConsumed++;
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

	// Press a Button - Node checks Valid Positions
	// If Ok, Pac-Man position is set to neighbor Node. 
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

	// Get Tile of PacMan current Position
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
	// Checks if Pacman moves away from Nodes allowed Directions.
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

			// Check if GameObject in that Position has Tile Component
			if (tile.GetComponent<Tile> () != null) {
				
				// Check if it is Portal 
				if (tile.GetComponent<Tile> ().isPortal) {

					// Create GameObject. Store PortalReciever and returns Other portal.
					GameObject otherPortal = tile.GetComponent<Tile> ().portalReceiver;
					return otherPortal;
				}
			}
		}
		return null;
	}
}
