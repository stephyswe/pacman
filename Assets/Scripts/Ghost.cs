using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

	public float moveSpeed = 3.9f;

	public Node startingPosition;

	public int scatterModeTimer1 = 7;
	public int chaseModeTimer1 = 20;
	public int scatterModeTimer2 = 7;
	public int chaseModeTimer2 = 20;
	public int scatterModeTimer3 = 7;
	public int chaseModeTimer3 = 20;
	public int scatterModeTimer4 = 7;
	public int chaseModeTimer4 = 20;

	public int[] scatterModeTimer = new [] {7, 8, 9, 10};
    public int[] chaseModeTimer = new [] {20, 21, 22, 23};



	private int modeChangeIteration = 1;
	private float modeChangeTimer = 0;

	public enum Mode {
		Chase,
		Scatter,
		Frightened
	}

	Mode currentMode = Mode.Scatter;
	Mode previousMode;
	private GameObject pacMan;

	// Location & DIrection Markers
	private Node currentNode, targetNode, previousNode;
	private Vector2 direction, nextDirection;

	// Use this for initialization
	void Start () {

		pacMan = GameObject.FindGameObjectWithTag("PacMan");
		Node node = GetNodeAtPosition (transform.localPosition);
		if (node != null) {
			currentNode = node;
		}

		direction = Vector2.right;

		previousNode = currentNode;

		Vector2 pacmanPosition = pacMan.transform.position;
		Vector2 targetTile = new Vector2 (Mathf.RoundToInt (pacmanPosition.x), Mathf.RoundToInt (pacmanPosition.y));
		targetNode = GetNodeAtPosition (targetTile);

		Debug.Log (targetNode);
		
	}
	
	// Update is called once per frame
	void Update () {

		ModeUpdate2 ();
		Move ();
		
	}

	void Move () {

		// If Ghost's Target is a Node
		if (targetNode != currentNode && targetNode != null) {
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
    }

	// OLD - If statements
	void ModeUpdate () {

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
	}
	
	void ChangeMode (Mode m) {
		currentMode = m;
	}

	Node ChooseNextNode () {
		Vector2 targetTile = Vector2.zero;

		Vector2 pacmanPosition = pacMan.transform.position;
		targetTile = new Vector2 (Mathf.RoundToInt (pacmanPosition.x), Mathf.RoundToInt (pacmanPosition.y));

		Node moveToNode = null;

		Node[] foundNodes = new Node[4];
		Vector2[] foundNodesDirection = new Vector2[4];

		int nodeCounter = 0;

		for (int i = 0; i < currentNode.neighbors.Length; i++)
		{
			if (currentNode.validDirections [i] != direction * -1) {
				foundNodes [nodeCounter] = currentNode.neighbors [i];
				foundNodesDirection [nodeCounter] = currentNode.validDirections [i];
				nodeCounter++;
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

		// If tile exists
		if (tile != null) {
			// If tile is a Node, return it. 
			if (tile.GetComponent<Node> () != null) {
				return tile.GetComponent<Node> ();
			}
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
