using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour {

	//CardData set from JSON
	public CardData cardData;
	//IDK if I even use this honestly
	public string id;
	//Setup access to various controllers
	private PlayerController player;
	private EnemyController enemy;
	private GameController gameController;
	private ClassController classController;
	//Generic object used for component lookups
	private GameObject go;
	//Text field to inform player of next attack (or if they are out of energy in this case)
	private Text nextAttackDisplay;

	void Start ()
	{
		//Find various controllers
		go = GameObject.Find("Player");
		player = (PlayerController) go.GetComponent(typeof(PlayerController));
		go = GameObject.Find("Enemy");
		enemy = (EnemyController) go.GetComponent(typeof(EnemyController));
		go = GameObject.Find("Controller");
		gameController = (GameController) go.GetComponent(typeof(GameController));
		//Find display for not enough energy updates
		go = GameObject.Find("nextAttackDisplay");
		nextAttackDisplay = (Text) go.GetComponent(typeof(Text));
		go = GameObject.Find("Class");
		classController = (ClassController) go.GetComponent(typeof(ClassController));
	}

	//Called whenver you click on a card
	public void PlayCard () 
	{
		//Make sure player has enough energy and is allowed to play
		if(player.playerData.energy >= cardData.energyCost && player.canPlay)
		{
			//Apply card damage
			if(cardData.damage != 0)
			{
				enemy.TakeDamage(cardData.damage);
			}
			//Apply card shields
			if(cardData.gainShield != 0)
			{
				player.GainShields(cardData.gainShield);
			}

			//Check if card has special class effects
			if(cardData.isSpecial)
			{
				//Call related class function to execute card functionality
				string methodName = classController.className;
				Invoke(methodName, 0);
			}
			//Otherwise discard the card and remove it from the game world
			else
			{
				//Discard the card
				player.discard.Add(cardData);
				player.hand.Remove(cardData);
				//Update player energy
				player.SpendEnergy(cardData.energyCost);
				//Remove the card from the game world
				Destroy(gameObject);
			}
		}
		//If player can play but doesn't have enough energy to play the card they clicked, let them know
		else if(player.canPlay)
		{
			UpdateInfo("Not enough energy!");
		}
	}

	//Updates the nextAttackDisplay text 
	public void UpdateInfo(string x)
	{
		nextAttackDisplay.text = x;
		//Clear text in two seconds
		StartCoroutine(ClearInfo(2, x, nextAttackDisplay));
	}

	IEnumerator ClearInfo(int amt, string x, Text t)
    {	
		//Wait however long
        yield return new WaitForSeconds(amt);
		//If the text hasn't been updated while waiting, set it back to the attack information
		if(x == t.text)
		{
			t.text = gameController.attackInfoStr;
		}
	}

	//Gets called whenever an Axassin card with special = true is played
	public void Axassin()
	{
		//Each case contains the special functionality for the current class card
		switch(cardData.cardName)
		{
			case("Right Handed Chop"):
				for(int i = 0; i < player.discard.Count; i++)
				{
					if(player.discard[i].cardName == "Left Handed Chop")
					{
						player.draw.Insert(0, player.discard[i]);
						player.discard.RemoveAt(i);
						gameController.AddCardsToHand(1);
					}
				}
				break;
			case("Left Handed Chop"):
				for(int i = 0; i < player.discard.Count; i++)
				{
					if(player.discard[i].cardName == "Right Handed Chop")
					{
						player.draw.Insert(0, player.discard[i]);
						player.discard.RemoveAt(i);
						gameController.AddCardsToHand(1);
					}
				}
				break;
			default: 
				break;
		}

			//Discard the card
			player.discard.Add(cardData);
			player.hand.Remove(cardData);
			//Update player energy
			player.SpendEnergy(cardData.energyCost);
			//Remove the card from the game world
			Destroy(gameObject);
	}
}
