using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacMan : MonoBehaviour {

	public float speed = 4.0f;
	private Vector2 direction = Vector2.zero;

	// Store Pac-Man current Position 
	private Node currentNode;

	// Use this for initialization
	void Start () {
		Node node = GetNodeAtPosition (transform.localPosition);

		// Find which Pellet PacMan is at 
		if (node != null) {
			currentNode = node;
			Debug.Log (currentNode);
		}
	}
	
	// Update is called once per frame
	void Update () {
        checkInput();
		//Move();
		UpdateOrientation();
		
	}

    void checkInput () {
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			direction = Vector2.left;
			MoveToNode(direction);
		} else if ( Input.GetKeyDown (KeyCode.RightArrow)) {
			direction = Vector2.right;
			MoveToNode(direction);
		} else if ( Input.GetKeyDown (KeyCode.UpArrow)) {
			direction = Vector2.up;
			MoveToNode(direction);
		} else if ( Input.GetKeyDown (KeyCode.DownArrow)) {
			direction = Vector2.down;
			MoveToNode(direction);
		}
    }

	void Move () {
		transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;
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
			transform.localScale = new Vector3 (-1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,0);

		} else if (direction == Vector2.right) {
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,0);

		} else if (direction == Vector2.up ) {
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,90);

		} else if (direction == Vector2.down ) {
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0,0,270);
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

	Node GetNodeAtPosition (Vector2 pos) {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board [(int)pos.x, (int)pos.y];
		if (tile != null) {
			return tile.GetComponent<Node> ();
		}
		return null;
	}
}
