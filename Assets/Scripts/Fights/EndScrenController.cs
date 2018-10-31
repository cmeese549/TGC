using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScrenController : MonoBehaviour {

	//The name of the menu scene in unity
	public string menuScene;

	//Load the menu scene
	public void startGame()
	{
		SceneManager.LoadScene(menuScene);
	}
}
