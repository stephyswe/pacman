using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

	public float moveSpeed = 5.9f;
	public float normalMoveSpeed = 5.9f;
	public float frightenedSpeed = 2.9f;
	public float consumedMoveSpeed = 15f;
	private float previousSpeed;

	public bool canMove = true;

	private int pinkyReleaseTimer = 5;
	private int inkyReleaseTimer = 14;
	private int clydeReleaseTimer = 21;
	private float ghostReleaseTimer = 0;

	public int frightenedModeDuration = 10;
	public int startBlinkingAt = 7;

	public bool isInGhostHouse = false;

	public Node startingPosition;
	public Node homeNode;
	public Node ghostHouse;

	public int[] scatterModeTimer = new [] {7, 8, 9, 10};
	public int[] chaseModeTimer = new [] {20, 21, 22, 23};

	private int scatterModeTimer1 = 0;
	private int scatterModeTimer2 = 0;
	private int scatterModeTimer3 = 0;
	private int scatterModeTimer4 = 0;

	private int chaseModeTimer1 = 0;
	private int chaseModeTimer2 = 0;
	private int chaseModeTimer3 = 0;
	private int chaseModeTimer4 = 0;

	public Sprite eyesUp;
	public Sprite eyesDown;
	public Sprite eyesLeft;
	public Sprite eyesRight;

	public RuntimeAnimatorController ghostUp;
	public RuntimeAnimatorController ghostDown;
	public RuntimeAnimatorController ghostLeft;
	public RuntimeAnimatorController ghostRight;

	public RuntimeAnimatorController ghostWhite;
	public RuntimeAnimatorController ghostFrightened;

	//public RuntimeAnimatorController[] ghostDirection = new [] {up,down,left, right};

	private int modeChangeIteration = 0;
	private float modeChangeTimer = 0;
	private float frightenedModeTimer = 0;
	private float blinkTimer = 0;

	private bool frightenedModeIsWhite = false;

	private AudioSource bgAudio;
	

	public enum Mode {
		Chase,
		Scatter,
		Frightened,
		Consumed
	}

	Mode currentMode = Mode.Scatter;
	Mode previousMode;

	public enum GhostType {
		Red,
		Pink,
		Blue,
		Orange
	}

	public GhostType ghostType = GhostType.Red;

	private GameObject pacMan;

	// Location & DIrection Markers
	private Node currentNode, targetNode, previousNode;
	private Vector2 direction, nextDirection;

	// Use this for initialization
	void Start () {

		if (GameBoard.isPlayerOneUp) {
			SetDifficultyForLevel(GameBoard.playerOneLevel);

		} else {
			SetDifficultyForLevel(GameBoard.playerTwoLevel);
		}

		bgAudio = GameObject.Find("Game").transform.GetComponent<AudioSource> ();

		pacMan = GameObject.FindGameObjectWithTag("PacMan");
		Node node = GetNodeAtPosition (transform.localPosition);
		if (node != null) {
			currentNode = node;
		}

		// If Ghosts are in the House
		if (isInGhostHouse) {
			direction = Vector2.up;
			targetNode = currentNode.neighbors [0];

		} else {
			direction = Vector2.left;
			targetNode = ChooseNextNode ();
		}

		previousNode = currentNode;
		UpdateAnimatorController ();	
	}

	void SetDifficultyForLevel (int level) {
		if (level == 2) {
			scatterModeTimer1 = 7;
			scatterModeTimer2 = 7;
			scatterModeTimer3 = 5;
			scatterModeTimer4 = 1;

			chaseModeTimer1 = 20;
			chaseModeTimer2 = 20;
			chaseModeTimer3 = 1033;

			frightenedModeDuration = 9;
			startBlinkingAt = 6;

			pinkyReleaseTimer = 4;
			inkyReleaseTimer = 12;
			clydeReleaseTimer = 18;

			moveSpeed = 6.9f;
			normalMoveSpeed = 6.9f;
			frightenedSpeed = 3.9f;
			consumedMoveSpeed = 18f;


		} else if (level == 3) {

			scatterModeTimer1 = 7;
			scatterModeTimer2 = 7;
			scatterModeTimer3 = 5;
			scatterModeTimer4 = 1;

			chaseModeTimer1 = 20;
			chaseModeTimer2 = 20;
			chaseModeTimer3 = 1033;

			frightenedModeDuration = 8;
			startBlinkingAt = 5;

			pinkyReleaseTimer = 3;
			inkyReleaseTimer = 10;
			clydeReleaseTimer = 15;

			moveSpeed = 7.9f;
			normalMoveSpeed = 7.9f;
			frightenedSpeed = 4.9f;
			consumedMoveSpeed = 20f;

		} else if (level == 4) {

			scatterModeTimer1 = 7;
			scatterModeTimer2 = 7;
			scatterModeTimer3 = 5;
			scatterModeTimer4 = 1;

			chaseModeTimer1 = 20;
			chaseModeTimer2 = 20;
			chaseModeTimer3 = 1033;

			frightenedModeDuration = 7;
			startBlinkingAt = 4;

			pinkyReleaseTimer = 2;
			inkyReleaseTimer = 8;
			clydeReleaseTimer = 13;

			moveSpeed = 8.9f;
			normalMoveSpeed = 8.9f;
			frightenedSpeed = 5.9f;
			consumedMoveSpeed = 22f;
			
		} else if (level == 5) {

			scatterModeTimer1 = 5;
			scatterModeTimer2 = 5;
			scatterModeTimer3 = 5;
			scatterModeTimer4 = 1;

			chaseModeTimer1 = 20;
			chaseModeTimer2 = 20;
			chaseModeTimer3 = 1033;

			frightenedModeDuration = 6;
			startBlinkingAt = 3;

			pinkyReleaseTimer = 2;
			inkyReleaseTimer = 6;
			clydeReleaseTimer = 10;

			moveSpeed = 9.9f;
			normalMoveSpeed = 9.9f;
			frightenedSpeed = 6.9f;
			consumedMoveSpeed = 24f;
		} 
	}

	public void MoveToStartingPosition () {

		if (transform.name != "Ghost_Blinky")
			isInGhostHouse = true;

		transform.position = startingPosition.transform.position;

		if (isInGhostHouse) {
			direction = Vector2.up;
		} else {
			direction = Vector2.left;
		}

		UpdateAnimatorController ();

	}

	public void Restart () {

		canMove = true;
		currentMode = Mode.Scatter;
		moveSpeed = normalMoveSpeed;
		previousSpeed = 0;

		
		ghostReleaseTimer = 0;
		modeChangeIteration = 1;
		modeChangeTimer = 0;

		if (transform.name != "Ghost_Blinky")
			isInGhostHouse = true;

		currentNode = startingPosition;
		if (isInGhostHouse) {
			direction = Vector2.up;
			targetNode = currentNode.neighbors [0];

		} else {
			direction = Vector2.left;
			targetNode = ChooseNextNode ();
		}

		previousNode = currentNode;
		UpdateAnimatorController ();
	}
	
	// Update is called once per frame
	void Update () {

		if (canMove) {
			ModeUpdate2 ();
			Move ();
			ReleaseGhosts ();
			CheckCollision ();
			CheckIsInGhostHouse ();
		}
	}

	void CheckIsInGhostHouse () {
		if (currentMode == Mode.Consumed) {
			GameObject tile = GetTileAtPosition (transform.position);

			if (tile != null) {
				if (tile.transform.GetComponent<Tile> () != null) {
					if (tile.transform.GetComponent<Tile> ().isGhostHouse) {
						moveSpeed = normalMoveSpeed;
						Node node = GetNodeAtPosition (transform.position);
						if (node != null) {
							currentNode = node;
							direction = Vector2.up;
							targetNode = currentNode.neighbors [0];

							previousNode = currentNode;
							currentMode = Mode.Chase;
							UpdateAnimatorController ();
						}
					}
				}
			}
		}
	}

	void CheckCollision () {

		// Collision is set to middle of PacMan and Ghosts
		Rect ghostRect = new Rect (transform.position, transform.GetComponent<SpriteRenderer> ().sprite.bounds.size / 4);
		Rect pacManRect = new Rect (pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer> ().sprite.bounds.size / 4);

		// When Ghosts and PacMan collide
		if (ghostRect.Overlaps (pacManRect)) {

			if (currentMode == Mode.Frightened) {
				//Debug.Log ("COLLIDED!");
				Consumed ();
			} else {
				if (currentMode != Mode.Consumed) {
					//- Pac-Man should die
					GameObject.Find("Game").transform.GetComponent<GameBoard> ().StartDeath ();
				}
			}
		}
	}

	void Consumed () {

		if (GameMenu.isOnePlayerGame) {
			GameBoard.playerOneScore += 200;
		} else {
			if (GameBoard.isPlayerOneUp) {
				GameBoard.playerOneScore += 200;
			} else {
				GameBoard.playerTwoScore += 200;
			}
		}

		currentMode = Mode.Consumed;
		previousSpeed = moveSpeed;
		moveSpeed = consumedMoveSpeed;
		UpdateAnimatorController ();

		GameObject.Find ("Game").transform.GetComponent<GameBoard> ().StartConsumed (this.GetComponent<Ghost> ());
	}

	void UpdateAnimatorController () {
		if (currentMode != Mode.Frightened && currentMode != Mode.Consumed) {
			if (direction == Vector2.up) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostUp;
			} else if (direction == Vector2.down) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostDown;
			} else if (direction == Vector2.left) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostLeft;
			} else if (direction == Vector2.right) {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostRight;
			} else {
				transform.GetComponent<Animator> ().runtimeAnimatorController = ghostLeft;
			}
		} else if (currentMode == Mode.Frightened) {
			transform.GetComponent<Animator> ().runtimeAnimatorController = ghostFrightened;

		} else if (currentMode == Mode.Consumed) {
			transform.GetComponent<Animator> ().runtimeAnimatorController = null;

			if (direction == Vector2.up) {
				transform.GetComponent<SpriteRenderer> ().sprite = eyesUp;
			} else if (direction == Vector2.down) {
				transform.GetComponent<SpriteRenderer> ().sprite = eyesDown;
			} else if (direction == Vector2.left) {
				transform.GetComponent<SpriteRenderer> ().sprite = eyesLeft;
			} else if (direction == Vector2.right) {
				transform.GetComponent<SpriteRenderer> ().sprite = eyesRight;
			}
		} 
	}

	void Move () {

		// If Ghost's Target is a Node and isn't in House
		if (targetNode != currentNode && targetNode != null && !isInGhostHouse) {
			if (OverShotTarget ()) {
				currentNode = targetNode;
				transform.localPosition = currentNode.transform.position;
				GameObject otherPortal = GetPortal (currentNode.transform.position);
				
				if (otherPortal != null) {
					transform.localPosition = otherPortal.transform.position;
					currentNode = otherPortal.GetComponent<Node> ();
				}
				targetNode = ChooseNextNode();
				previousNode = currentNode;
				currentNode = null;
				UpdateAnimatorController ();
			} else {
				//Debug.Log (direction);
				transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
			}
		}
	}

	// New - Switch statement
	void ModeUpdate2 () {
		if (currentMode != Mode.Frightened) {
			modeChangeTimer += Time.deltaTime;
		}
		switch (currentMode) {
			case Mode.Frightened:
				break;
 
			case Mode.Scatter:
				if (modeChangeTimer > scatterModeTimer[modeChangeIteration]) {
					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;
				}
				break;
			case Mode.Chase:
				if(modeChangeTimer > chaseModeTimer[modeChangeIteration]) {
					modeChangeIteration = (modeChangeIteration + 1)%4; // so if it is 3 to go to 0 for next one
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;
				}
				break;
		}

		// If PacMan eat a Super Pellet, count down frightened Mode Timer. 
		if (currentMode == Mode.Frightened) {
			frightenedModeTimer += Time.deltaTime;

			// When Frightened Mode is over, change back to normal mode. 
			if (frightenedModeTimer >= frightenedModeDuration) {

				bgAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard> ().bgAudioNormal;
				bgAudio.Play ();
				
				frightenedModeTimer = 0;
				ChangeMode(previousMode);

			}

			// If 7 seconds has elapsed, Start Blinking Animation
			if (frightenedModeTimer >= startBlinkingAt) {
				blinkTimer += Time.deltaTime;
				if (blinkTimer >= 0.1f) {
					blinkTimer = 0f;

					// Switch between White and Blue animation for Scared Ghosts.
					if (frightenedModeIsWhite) {
						transform.GetComponent<Animator> ().runtimeAnimatorController = ghostFrightened;
						frightenedModeIsWhite = false;
					} else {
						transform.GetComponent<Animator> ().runtimeAnimatorController = ghostWhite;
						frightenedModeIsWhite = true;
					}
				}
			}

		} 
	}

	// OLD - If statements
	/* void ModeUpdate () {

		// If Ghost isn't Frightened
		if (currentMode != Mode.Frightened) {

			// Change Ghost to Chase 
			modeChangeTimer += Time.deltaTime;

			// Check if Modes Iteration is 1 
			if (modeChangeIteration == 1) {

				// if Scatter Timer is more then ChangeTimer change Mode to Chase and vice versa
				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1) {
					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;
				}
				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1) {
					modeChangeIteration = 2;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;
				}
			} else if (modeChangeIteration == 2) {
				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2) {
					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;
				}
				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2) {
					modeChangeIteration = 3;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;
				}

			} else if (modeChangeIteration == 3) {
				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3) {
					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;
				}

				if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3) {
					modeChangeIteration = 4;
					ChangeMode (Mode.Scatter);
					modeChangeTimer = 0;
				}

			} else if (modeChangeIteration == 4) {
				if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4) {
					ChangeMode (Mode.Chase);
					modeChangeTimer = 0;
				}
			}

		} else if (currentMode == Mode.Frightened) {

		}
	} */
	
	void ChangeMode (Mode m) {

		// If CurrentMode is Frightened save Ghost Speed to temp varible.
		// In Frightened Mode - Change Ghosts MoveSpeed and replace with Frightened Speed. 
		if (currentMode == Mode.Frightened) {
			moveSpeed = previousSpeed;
		}

		// Store normal speed in temp variable (previousSpeed)
		if ( m == Mode.Frightened) {
			previousSpeed = moveSpeed;
			moveSpeed = frightenedSpeed;
		}

		// Save Mode in temporary varible. And replace Mode with m.
		if (currentMode != m) {	
			previousMode = currentMode;
			currentMode = m;
		}

		UpdateAnimatorController ();
	}

	public void StartFrightenedMode() {
		if (currentMode != Mode.Consumed) {
			frightenedModeTimer = 0;
			bgAudio.clip = GameObject.Find("Game").transform.GetComponent<GameBoard> ().bgAudioFrightened;
			bgAudio.Play ();
			ChangeMode (Mode.Frightened);
		}
	}

	Vector2 GetRedGhostTargetTile () {
		Vector2 pacManPosition = pacMan.transform.localPosition;
		Vector2 targetTile = new Vector2 (Mathf.RoundToInt (pacManPosition.x), Mathf.RoundToInt (pacManPosition.y));

		return targetTile;
	}

	Vector2 GetPinkGhostTargetTile () {

		//- Four tiles ahead of Pac-Man
		// Taking account Position and Orientation
		Vector2 pacManPosition = pacMan.transform.localPosition;
		Vector2 pacManOrientation = pacMan.GetComponent<PacMan> ().orientation;

		int pacManPositionX = Mathf.RoundToInt (pacManPosition.x);
		int pacManPositionY = Mathf.RoundToInt (pacManPosition.y);

		Vector2 pacManTile = new Vector2 (pacManPositionX, pacManPositionY);
		Vector2 targetTile = pacManTile + (4 * pacManOrientation);

		return targetTile;
	}

	Vector2 GetBlueGhostTargetTile () {

		//- Select the position two tiles in front of Pac-Man
		//- Draw Vector from Blinky to that position
		//- Double the length of the vector
		Vector2 pacManPosition = pacMan.transform.localPosition;
		Vector2 pacManOrientation = pacMan.GetComponent<PacMan> ().orientation;

		int pacManPositionX = Mathf.RoundToInt (pacManPosition.x);
		int pacManPositionY = Mathf.RoundToInt (pacManPosition.y);

		Vector2 pacManTile = new Vector2 (pacManPositionX, pacManPositionY);
		Vector2 targetTile = pacManTile + (2* pacManOrientation);

		//-Temporary Blinky Position
		Vector2 tempBlinkyPosition = GameObject.Find ("Ghost_Blinky").transform.localPosition;

		int blinkyPositionX = Mathf.RoundToInt (tempBlinkyPosition.x);
		int blinkyPositionY = Mathf.RoundToInt (tempBlinkyPosition.y);

		tempBlinkyPosition = new Vector2 (blinkyPositionX, blinkyPositionY);

		float distance = GetDistance (tempBlinkyPosition, targetTile);
		distance *= 2;

		targetTile = new Vector2 (tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);
		return targetTile;
	}

	Vector2 GetOrangeGhostTargetTile () {


		//- Calculate the distance from Pac-Man
		//- If the distance is greater than eight tiles targeting is the same as Blinky
		//- If the distance is less then eight tiles, then target is his home node, so same as scatter mode

		Vector2 pacManPosition = pacMan.transform.localPosition;

		// Clydes Position
		float distance = GetDistance (transform.localPosition, pacManPosition);
		Vector2 targetTile = Vector2.zero;

		if (distance > 8) {
			targetTile = new Vector2 (Mathf.RoundToInt (pacManPosition.x), Mathf.RoundToInt (pacManPosition.y));

		} else if (distance < 8) {
			targetTile = homeNode.transform.position;
		}

		return targetTile;
	}

	Vector2 GetTargetTile () {
		Vector2 targetTile = Vector2.zero;
		if (ghostType == GhostType.Red) {
			targetTile = GetRedGhostTargetTile ();

		} else if (ghostType == GhostType.Pink) {
			targetTile = GetPinkGhostTargetTile ();

		} else if (ghostType == GhostType.Blue) {
			targetTile = GetBlueGhostTargetTile ();

		} else if (ghostType == GhostType.Orange) {
			targetTile = GetOrangeGhostTargetTile ();
		}

		return targetTile;
	}

	Vector2 GetRandomTile () {
		int x = Random.Range (0,28);
		int y = Random.Range (0,36);

		return new Vector2 (x, y);
	}

	void ReleasePinkGhost() {
		if (ghostType == GhostType.Pink && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}

	void ReleaseBlueGhost() {
		if (ghostType == GhostType.Blue && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}

	void ReleaseOrangeGhost() {
		if (ghostType == GhostType.Orange && isInGhostHouse) {
			isInGhostHouse = false;
		}
	}

	void ReleaseGhosts() {
		ghostReleaseTimer += Time.deltaTime;
		if (ghostReleaseTimer > pinkyReleaseTimer)
			ReleasePinkGhost ();

		if (ghostReleaseTimer > inkyReleaseTimer) {
			ReleaseBlueGhost ();
		}

		if (ghostReleaseTimer > clydeReleaseTimer) {
			ReleaseOrangeGhost ();
		}
	}

	Node ChooseNextNode () {
		Vector2 targetTile = Vector2.zero;

		if (currentMode == Mode.Chase) {
			targetTile = GetTargetTile ();
		} else if (currentMode == Mode.Scatter) {
			targetTile = homeNode.transform.position;
		} else if (currentMode == Mode.Frightened) {
			targetTile = GetRandomTile ();
		} else if (currentMode == Mode.Consumed) {
			targetTile = ghostHouse.transform.position;
		}

		Node moveToNode = null;

		Node[] foundNodes = new Node[4];
		Vector2[] foundNodesDirection = new Vector2[4];

		int nodeCounter = 0;

		// Iterate the neighbors of Current Node
		for (int i = 0; i < currentNode.neighbors.Length; i++)
		{
			// Ghosts cannot move in Reverse direction
			if (currentNode.validDirections [i] != direction * -1) {

				if (currentMode != Mode.Consumed) {
					// Get a tile for our current Position
					GameObject tile = GetTileAtPosition (currentNode.transform.position);
					if (tile.transform.GetComponent<Tile> ().isGhostHouseEntrance == true) {
						//- Found a ghost house, don't want to allow movement

						if (currentNode.validDirections[i] != Vector2.down) {
							//Add Node to Found Node Arrays 
							foundNodes [nodeCounter] = currentNode.neighbors [i];
							foundNodesDirection [nodeCounter] = currentNode.validDirections [i];
							nodeCounter++;
						}
					} else {
						//Add Node to Found Node Arrays 
						foundNodes [nodeCounter] = currentNode.neighbors [i];
						foundNodesDirection [nodeCounter] = currentNode.validDirections [i];
						nodeCounter++;
					}
				} else {
					//Add Node to Found Node Arrays 
					foundNodes [nodeCounter] = currentNode.neighbors [i];
					foundNodesDirection [nodeCounter] = currentNode.validDirections [i];
					nodeCounter++;
				}
			}
		}

		// Found 1 node
		if (foundNodes.Length == 1) {
			moveToNode = foundNodes [0];
			direction = foundNodesDirection [0];
		}

		// Found more then 1 node. 
		if (foundNodes.Length > 1) {
			float leastDistance = 100000f;
			for (int i = 0; i < foundNodes.Length; i++)
			{
				if (foundNodesDirection [i] != Vector2.zero) {
					float distance = GetDistance (foundNodes[i].transform.position, targetTile);
					if (distance < leastDistance) {
						leastDistance = distance;
						moveToNode = foundNodes [i];
						direction = foundNodesDirection [i];
					}
				}
			}
		}

		return moveToNode;
	}

	// Get Position of Node
	Node GetNodeAtPosition (Vector2 pos) {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard> ().board [(int)pos.x, (int)pos.y];
		//Debug.Log ("TILE: " + tile + ", FOR GHOST: " + ghostType);

		// If tile exists
		if (tile != null) {
			// If tile is a Node, return it. 
			if (tile.GetComponent<Node> () != null) {
				return tile.GetComponent<Node> ();
			}
		}

		return null;
	}

	GameObject GetTileAtPosition (Vector2 pos) {
		int tileX = Mathf.RoundToInt (pos.x);
		int tileY = Mathf.RoundToInt (pos.y);

		GameObject tile = GameObject.Find("Game").transform.GetComponent<GameBoard> ().board [tileX, tileY];
		if (tile != null) {
			return tile;
		}
		return null;
	}

	GameObject GetPortal (Vector2 pos) {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard> ().board [(int)pos.x, (int)pos.y];

		if (tile != null) {
			if (tile.GetComponent<Tile> ().isPortal) {
				GameObject otherPortal = tile.GetComponent<Tile> ().portalReceiver;
				return otherPortal;
			}
		}

		return null;
	}

	float LengthFromNode (Vector2 targetPosition) {
		Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
		return vec.sqrMagnitude;
	}

	bool OverShotTarget () {
		float nodeToTarget = LengthFromNode (targetNode.transform.position);
		float nodeToSelf = LengthFromNode (transform.localPosition);

		return nodeToSelf > nodeToTarget;
	}

	// Get Distance between two Positons 
	float GetDistance (Vector2 posA, Vector2 posB) {
		float dx = posA.x - posB.x;
		float dy = posA.y - posB.y;

		float distance = Mathf.Sqrt (dx * dx + dy * dy);

		return distance;
	}
}
