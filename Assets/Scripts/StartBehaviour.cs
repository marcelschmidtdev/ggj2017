﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartBehaviour : MonoBehaviour
{
	public TextAsset GameStatePrefab;

	private void Start ()
	{
		Debug.Log ("Create new game state...");
		gameObject.AddComponent<GameState> ();

		Debug.Log ("Loading UI...");
		SceneManager.LoadScene ("UI", LoadSceneMode.Additive);
	}
}
