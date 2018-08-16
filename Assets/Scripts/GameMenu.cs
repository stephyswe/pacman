using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour {

	// Static are called with the type name. No instance is required—this makes them slightly faster. Static methods can be public or private.
	public static bool isOnePlayerGame = true;

	public Text playerText1;
	public Text playerText2;
	public Text playerSelector;
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyUp (KeyCode.UpArrow)) {
			if (!isOnePlayerGame) {
				isOnePlayerGame = true;

				// Moves playerSelector to Player 1
				playerSelector.transform.localPosition = new Vector3 (playerSelector.transform.localPosition.x,
											 	playerText1.transform.localPosition.y, playerSelector.transform.localPosition.z);
			}
		} else if (Input.GetKeyUp (KeyCode.DownArrow)) {
			if (isOnePlayerGame) {
				isOnePlayerGame = false;

				// Moves playerSelector to Player 2
				playerSelector.transform.localPosition = new Vector3 (playerSelector.transform.localPosition.x,
				 								playerText2.transform.localPosition.y, playerSelector.transform.localPosition.z);
			}
		} else if (Input.GetKeyUp (KeyCode.Return)) {
			SceneManager.LoadScene("Level1");
		}
		
	}
}
