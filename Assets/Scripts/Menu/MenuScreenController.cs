using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour {

	//The name of the game scene
	public string gameScene;

	//Launch the game scene
	public void startGame()
	{
		SceneManager.LoadScene(gameScene);
	}
}
