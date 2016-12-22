﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;

public class OrderAndChaos : GameMaster {
	
	public Text role;
	public Text colour;
	public Text turn;

	void Start () {
		if (GameData.playerIndex == 1) {
			role.text = "You are playing as Order";
			turn.text = "Your turn";
			aiPlayerIndx = 2;
			playersTurn = true;
		} else {
			role.text = "You are playing as Chaos";
			turn.text = "AIs turn";
			aiPlayerIndx = 1;
			playersTurn = false;
		}

		tiles = new List<GameObject>();
		for (int i = 0; i < x; i++) {
			for (int j = 0; j < y; j++) {
				//create the game object 
				GameObject tile = (GameObject)Instantiate(preFabTile, new Vector3 ((i+(0.1f*i)), 0, j+(0.1f*j)), Quaternion.identity);
				tile.GetComponent<Tile>().setMaster(this);
				tile.GetComponent<Tile> ().setXY (i, j);
				tiles.Add( tile);
				tile.GetComponent<Tile>().preFabCounter = preFabCounter;
			}
		} 
		gameState = new State ();
		selectedColour = 1;
		colour.text = "Selected playing piece: White";
	}

	// Update is called once per frame
	void Update () {
		if (!playersTurn && gamePlaying) {
			turn.text = "AIs turn";
			if (!brain.started) {
				OCAIState preState = new OCAIState (gameState, aiPlayerIndx, null, 0);
				aiThread = new Thread (new ThreadStart (() => brain.runAI (preState)));
				aiThread.Start ();
			}
			if (brain.done) {
				OCAIState postState = (OCAIState)brain.next;
				gameState = postState.state;
				visualiseMove ();
				playersTurn = true;
				aiThread.Abort ();
			}

		} else {
			turn.text = "Your turn";
		}
		if (Input.GetKeyDown ("space")) {
			if (selectedColour == 1) {
				colour.text = "Selected playing piece: Black";
				selectedColour = 2;
				selectedColour = 2;
			} else {
				colour.text = "Selected playing piece: White";
				selectedColour = 1;
				selectedColour = 1;
			}
		}
	}

	public override void spawn(int x, int y)
	{
		playersTurn = false;
		GameObject counter = (GameObject)Instantiate(preFabCounter, new Vector3 ((x+(0.1f*x)), 0.1f, (y+(0.1f*y))), Quaternion.identity);
		counter.GetComponent<Collider> ().enabled = false;
		if (selectedColour == 1) {
			counter.GetComponent<Renderer> ().material.color = Color.white;
		} else {
			counter.GetComponent<Renderer> ().material.color = Color.black;
		}
		int[] playedPiece = new int[3]{ x, y, selectedColour};

		gameState.playPiece (playedPiece);
		checkState (playedPiece);
	}

	public void checkState (int[] playedPiece)
	{
		if (gameState.checkGameEnd (playedPiece)) {
			gamePlaying = false;
			return;
		}
		if (gameState.numbPiecesPlayed == 36) {
			gamePlaying = false;
			return;
		}
	}

	private void visualiseMove()
	{
		foreach(GameObject tile in tiles)
		{
			int pX = gameState.lastPiecePlayed [0];
			int pY = gameState.lastPiecePlayed [1];

			int tX = tile.GetComponent<Tile> ().x;
			int tY = tile.GetComponent<Tile> ().y;
			if (pX == tX && pY == tY) {
				tile.GetComponent<Tile>().aiPlayHere(gameState.lastPiecePlayed [2]);
			}
		}
	}
}