using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* UNIVERSAL PLAYLER CONTROLLER */
public class PlayerController : MonoBehaviour {

	//Player data that gets set from JSON
	public PlayerData playerData;
	/* Three CardData lists to represet the draw pile, the discard pile, and the players active hand.
	This is only a data representation, and the instantiation/destruction of the cards is handled 
	in the Game Controller */
	public List<CardData> hand;
	public List<CardData> discard;
	public List<CardData> draw;
	//Bool used to prevent card interactability
	public bool canPlay;
	//Canvasonies (the plural of canvas), main game canvas, hand canvas, and the death screen
	public GameObject canvas;
	public GameObject handCanvas;
	public GameObject death;
	//End turn button
	public Button endTurn;
	//Player current row, 0 = back 1 = front
	public int row;

	//Various player UI elements
	private Text playerShield;
	private Text playerHealth;
	private Text playerEnergy;
	private Text playerDamageDisplay;
	//Generic game object used for component lookups
	private GameObject go;
	//I honestly don't remember what this is.  I think it gets set to all objects in the hand canvas?
	private GameObject[] handDisplay;

	void Start ()
	{
		//Set the text displays for the player UI
		go = GameObject.Find("playerShield");
		playerShield = (Text) go.GetComponent(typeof(Text));
		go = GameObject.Find("playerHealth");
		playerHealth = (Text) go.GetComponent(typeof(Text));
		go = GameObject.Find("playerEnergy");
		playerEnergy = (Text) go.GetComponent(typeof(Text));
		go = GameObject.Find("playerDamageDisplay");
		playerDamageDisplay = (Text) go.GetComponent(typeof(Text));
		//Stop the player from playing until they have selected a row
		DisablePlay();
		//Update the playerUI with current values
		UpdateEnergy(playerData.energy);
		UpdateHealth(playerData.health);
		UpdateShields(playerData.shields);
	}

	//Disables card interactability
	public void DisablePlay()
	{
		//Also disables end turn button
		endTurn.interactable = false;
		canPlay = false;
		//Find all cards by tag
		handDisplay = GameObject.FindGameObjectsWithTag("Card");
		foreach (GameObject card in handDisplay)
		{
			//Apply a "disabled" filter to each card (like an instagram filter, not some kinda code thing)
			Image[] img = card.GetComponentsInChildren<Image>();
			foreach (Image image in img)
			{
				image.color = new Color32(111,111,111,111);
			}
		}
	}

	//Enables card interactability
	public void EnablePlay()
	{
		//Find each card
		handDisplay = GameObject.FindGameObjectsWithTag("Card");
		foreach (GameObject card in handDisplay)
		{
			//Disable filter
			Image[] img = card.GetComponentsInChildren<Image>();
			foreach (Image image in img)
			{
				image.color = new Color32(255,255,255,255);
			}
		}
		//Allow player to play and enables end turn button
		canPlay = true;
		endTurn.interactable = true;
	}

	//Updates the current energy level and sends it to the UI
	public void UpdateEnergy(int x)
	{
		playerData.energy = x;
		playerEnergy.text = playerData.energy.ToString();
	}

	//Updates the current shield level and sends it to the UI
	public void UpdateShields(int x)
	{
		playerData.shields = x;
		playerShield.text = playerData.shields.ToString();
	}

	//Updates the current health level and sends it to the UI, also handles death state
	public void UpdateHealth(int x)
	{
		//Check if the player is dead
		if(x <= 0)
		{
			//If ur dead, disable the main canvas and show the death screen
			canvas.SetActive(false);
			death.SetActive(true);
		}
		//Otherwise update health and send it to the UI
		else
		{
			playerData.health = x;
			playerHealth.text = playerData.health.ToString();
		}
	}

	//Updates player health and shields, and sends to UI.  also handles death state
	public void TakeDamage(int x)
	{
		//If player has shields, do the math
		if(playerData.shields != 0)
		{
			//If damage is more than shields, destroy them all and apply remaining damage
			if(x > playerData.shields)
			{
				x = x - playerData.shields;
				UpdateShields(0);
				playerData.health -= x;
			}
			//Otherwise reduce shields and update the UI
			else
			{
				x = playerData.shields - x;
				UpdateShields(x);
			}
		}
		//Otherwise update the health
		else
		{
			playerData.health -= x;
		}
		//Make sure we aren't dead
		if(playerData.health >= 0)
		{
			//If we're allive, update the health and the UI
			UpdateHealth(playerData.health);
			PlayerDamageDisplay("- " + x.ToString());
		}
		else
		{
			//If we're dead, disable main canvas and show death screen
			canvas.SetActive(false);
			death.SetActive(true);
		}
	}

	//Called whenever a row swap button is pressed, assigns new row
	public void AssignRow(int x)
	{
		row = x;
	}

	//Reduces the players energy by x and udpates the UI
	public void SpendEnergy(int x)
	{
		playerData.energy -= x;
		playerEnergy.text = playerData.energy.ToString();
	}

	//Adds to the players shields and updates the UI
	public void GainShields(int x)
	{
		playerData.shields += x;
		playerShield.text = playerData.shields.ToString();
	}

	//Displays damage numbers on the UI and calls a coroutine 2 seconds later to clear them
	public void PlayerDamageDisplay(string x)
	{
		playerDamageDisplay.text = x;
		StartCoroutine(ClearInfo(2, x, playerDamageDisplay));
	}

	//Clear indicated text field after time, unless the text field has already been updated in that time
	IEnumerator ClearInfo(int amt, string x, Text t)
    {
        yield return new WaitForSeconds(amt);
		if(t.text == x)
		{
			t.text = "";
		}
	}
}
