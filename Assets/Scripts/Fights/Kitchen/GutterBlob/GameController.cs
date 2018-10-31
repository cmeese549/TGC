/*Game Controller for Gutter Blob*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	//Card Template Prefab
	public GameObject card;
	//Current hand and Enemy canvas
	public Transform handCanvas;
	public Transform enemyCanvas;
	//End Turn button
	public Button turnOver;
	//Row switch buttons
	public Button backRow;
	public Button frontRow;
	//String with information on the next enemy attack
	public string attackInfoStr;

	//Current hand
	private CardData[] hand;
	//JSON interface
	private DataController dataController;
	/* Object containing one instance of each card in the players deck, 
	and the number of times that card is in the deck */
	private List<DeckBuilder> playerDeck;
	//Data on all class cards
	private List<CardData> cardData;
	//C# class instantiated from JSON, feeds values to the Player Controller
	private PlayerData playerData;
	//C# class instantiated from JSON, feeds values to the Enemy Controller
	private EnemyData enemyData;
	//C#c class instantiated from JSON, feeds values to the Class Controller
	private ClassData classData;
	//Player Controller
	private PlayerController player;
	//Enemy Controller
	private EnemyController enemy;
	//Class Conroller
	private ClassController classController;
	//C# class instantiated from JSON, feeds values to enemy's current attack
	private EnemyAttackData enemyAttack;
	//Generic object used for component lookups
	private GameObject go;
	//Text field in which to display the next enemy attack
	private Text nextAttackDisplay;
	//Whether or not the player has chosen their starting row
	private bool rowInit;

	void Start () 
	{
		//Set false until player picks a row
		rowInit = false;
		//Tell the player to pikck a row
		go = GameObject.Find("nextAttackDisplay");
		nextAttackDisplay = (Text) go.GetComponent(typeof(Text));
		nextAttackDisplay.text = "Pick a row to begin the fight.";
		//Instantiate the Data Controller JSON interface
		dataController = FindObjectOfType<DataController> ();
		//Load class data
		classData = dataController.GetClassData ();
		//Load enemy data
		enemyData = dataController.GetEnemyData ("Gutter Blob");
		//Instantiate enemy
		enemy = EnemyInit(enemyData);
		//Load player data
		playerData = dataController.GetPlayerData ();
		//Instantiate the Class Controller
		classController = ClassInit(classData);
		//Load the player data
		playerDeck = playerData.deck;
		//Load All Card Data
		cardData = dataController.GetDeck ();
		//Instantiate Player
		player = PlayerInit(playerData, playerDeck, cardData);
		//Shuffle the draw deck
		Shuffle(player.draw);
	}

	//Called when a player clicks on an interactable row
	public void AssignRow(int row)
	{
		//Set player row to the new row
		player.row = row;
		//If row is 0, player is in the back row
		if(row == 0)
		{
			//Disable row swap buttons
			frontRow.interactable = false;
			backRow.interactable = false;
			//Update row to reflect players choice
			Text t = backRow.GetComponentInChildren<Text>();
			t.text = "Back Row (Current)";
		}
		//If row is 1, player is in the front row
		else
		{
			//Disable row swap buttons
			backRow.interactable = false;
			frontRow.interactable = false;
			//Update row to reflect players choice
			Text t = frontRow.GetComponentInChildren<Text>();
			t.text = "Front Row (Current)";
		}
		//Check if this is the first time the player has entered a row
		if(!rowInit)
		{
			//This if the first time, run start of game functions and update rowInit boolean
			Begin();
			rowInit = true;
		}
	}
	
	//Called once player has picked their first row, starts the game
	public void Begin()
	{
		//Get data on the enemy's next attack
		enemyAttack = GetEnemyAttack(enemy);
		//Draw 5 cards
		DrawCards(5);
		//Enable card interaction
		player.EnablePlay();
	}	

	//Called only in Start() to send data from the JSON generated C# class ClassData to our Class Controller
	private ClassController ClassInit(ClassData classData)
	{	
		//Access Class Controller script
		classController = GetComponentInChildren<ClassController>();
		//Set Class Controller values from PlayerData class
		classController.className = classData.className;
		//Loop through all the cards in the class, specifying those with special functionality and starter cards
		for(int i = 0; i < classData.cards.Count; i++)
		{
			classController.classCards.Add(classData.cards[i]);
			if(classData.cards[i].isSpecial)
			{
				classController.specialCards.Add(classData.cards[i]);
			}
		}
		//Return the player controller with all data in place
		return classController;
	}

	/* Called only in Start() to send data fom the JSON generated C# class PlayerData to our Player Controller,
	DeckBuilder list to tell us how many of what cards the player has, as well as 
	CardData containing the details of all available cards (damage, cost, etc) */
	private PlayerController PlayerInit(PlayerData playerData, List<DeckBuilder> deck, List<CardData> cardData)
	{
		//Access Player Controller script
		player = GetComponentInChildren<PlayerController>();
		//Set Player Controller values from PlayerData class
		player.playerData = playerData;
		/* Loop through each DeckBuilder data in the players deck,
		each iteration contains which card to add and in what quantity */
		for(int i = 0; i < player.playerData.deck.Count; i++)
		{
			//Look up data on current card from the CardData list
			int cardToAdd = cardData.FindIndex(item => item.cardName == player.playerData.deck[i].cardName);
			//Add all instances this card to the players draw pile 
			for(int j = 0; j < player.playerData.deck[i].count; j++)
			{
				cardData[cardToAdd].id = cardData[cardToAdd].cardName;
				player.draw.Add(cardData[cardToAdd]);
			}
		}
		//Return the player controller with all data in place
		return player;
	}

	//Called only in Start() to sent data from the JSON generated C# class EnemyData to our Enemy Controller
	private EnemyController EnemyInit(EnemyData enemyData)
	{
		//Access Enemy Controller script
		enemy = GetComponentInChildren<EnemyController>();
		//Set Enemy Controller values from EnemyData class
		enemy.enemyData = enemyData;
	
		//Return the Enemy Controller with all data in place
		return enemy;
	}

	//Called at the start of each turn to determine the next enemy attack
	private EnemyAttackData GetEnemyAttack(EnemyController enemy)
	{
		//Select a random attack from all possible attacks, equal weighting
		int randomIndex = Random.Range(0, enemy.enemyData.attacks.Count);
		//Update UI to inform player about the next enemy attack
		attackInfoStr =  "Next Attack: " + enemy.enemyData.attacks[randomIndex].attackName + ".  It will deal " +  enemy.enemyData.attacks[randomIndex].damage.ToString() + " damage.";
		nextAttackDisplay.text = attackInfoStr;
		//Return data on the randomly selected attack
		return enemy.enemyData.attacks[randomIndex];
	}

	//Shuffles whatever deck you pass to it (draw, discard, or hand(but idk why you would shuffle yor hand))
	private void Shuffle (List<CardData> deck)
	{
		//Loop through each card in the deck you passed
		for (int i = 0; i < deck.Count; i++) 
		{
			//Set temporary variable to current CardData
			CardData temp = deck[i];
			//Select a random valid iterator value
			int randomIndex = Random.Range(i, deck.Count);
			//Set current CardData to the CardData at the randomly selected iterator value
			deck[i] = deck[randomIndex];
			//Set the CardData at the randomly selected card iterator value to the temporary variable
			deck[randomIndex] = temp;
		}
	}

	//Called when player clicks the "End Turn" button
	public void EndTurn()
	{
		//Disable card interactability
		player.DisablePlay();
		//Set enemy shields to 0 
		enemy.UpdateShields(0);
		//Inform player of the incoming attack
		nextAttackDisplay.text = enemyAttack.attackName + " incoming!";
		//Execute attack and begin the next turn
		StartCoroutine(Reset(2));
	}

	//Only called from EndTurn(), waits for indicated length, then executes the enemy attack and begins the next turn.
	IEnumerator Reset(int amt)
    {
		//Wait for the indicated amount of time (seconds)
        yield return new WaitForSeconds(amt);
		//Apply damage to the player, if there is any
		if(enemyAttack.damage != 0)
		{
			player.TakeDamage(enemyAttack.damage);
		}
		//Make sure the player didn't die
		if(player.playerData.health > 0)
		{
			//Add enemy shields, if there are any
			if(enemyAttack.gainShield != 0)
			{
				enemy.GainShields(enemyAttack.gainShield);
			}
			//Enemy attack over, discard all current cards and draw more
			DiscardAndDraw(5);
			//Get data on the next enemy attack
			enemyAttack = GetEnemyAttack(enemy);
			//Reset the back row damage bonus
			enemy.damageBonus = true;
			//Reset player energy
			player.UpdateEnergy(3);
			//Set player shields to 0
			player.UpdateShields(0);
			//Enable card interactability
			player.EnablePlay();
		}
	}

	//Discard all cards in current hand and draw more
	public void DiscardAndDraw(int drawAmount)
	{
		//Add each card in current hand to the discard pile
		foreach (CardData card in player.hand)
		{
			player.discard.Add(card);	
		}
		//Set counter so it doesn't change along with the list as we remove items
		int counter = player.hand.Count;
		//Remove all cards from the playe rhand
		for (int i = 0; i < counter; i++)
		{
			player.hand.RemoveAt(0);
		}
		//Destroy everything in the players hand canvas
		foreach (Transform child in handCanvas)
		{
			Destroy(child.gameObject);
		}
		//Draw cards
		DrawCards(drawAmount);
	}

	//Puts drawAmount number of cards into the players hand
	public void DrawCards(int drawAmount)
	{
		//Determine however many cards
		int remaining = player.draw.Count;
		//If there are more cards in the draw pile than we are drawing, then just draw the cards
		if(remaining >= drawAmount)
		{	
			AddCardsToHand(drawAmount);
		}
		//If we need to draw more cars than are in the draw pile we gotta do some shit
		else
		{
			//Shuffle the discard pile
			Shuffle(player.discard);
			//Set counter so it doesn't change along with the list as we remove items
			int remove = player.discard.Count;
			//Remove cards from the discard pile and add them to the draw pile (underneath whatever cards where already there)
			for(int i = 0; i < remove; i++)
			{
				player.draw.Add(player.discard[0]);
				player.discard.RemoveAt(0);
			}
			//Spawn the cards into the game world
			AddCardsToHand(drawAmount);
		}
	}

	//Instantiates cards into the game world
	public void AddCardsToHand(int amount)
	{
		for(int i = 0; i < amount; i++)
		{
			//Instantiate card into the handCanvas from the card prefab.  Position doesn't matter, it's fully controlled by the handCanvas
			Vector3 spawnPosition = new Vector3(0, 0, 0);
			GameObject currentCard = Instantiate(card, spawnPosition, Quaternion.identity, handCanvas);
			//Name the card game object
			currentCard.name = player.draw[i].cardName;
			//Access the cards controller to set values
			CardController cardController = currentCard.GetComponent<CardController>();
			//Set card data equal to that of the card we are about to draw in the draw pile
			cardController.cardData = player.draw[i];
			//Get all text fields in the card template
			Component[] fields = currentCard.GetComponentsInChildren<Text>();
			//Iterate through each text field
			foreach (Text t in fields)
			{
				//Update the card template text fields with card data
				switch (t.name)
				{
					case "cardName":
						t.text = player.draw[i].cardName;
						break;
					case "cost":
						t.text = player.draw[i].energyCost.ToString();
						break;
					case "description":
						t.text = player.draw[i].description;
						break;
					case "type":
						t.text = player.draw[i].type;
						break;
					default:	
						break;
				}
			}
			//Load all card face sprites
			Sprite[] sprites = Resources.LoadAll<Sprite>("Card_Faces/bestestFaces");
			//Get all images in card template
			Image[] images = currentCard.GetComponentsInChildren<Image>();
			//Iterate through each image
			foreach (Image s in images)
			{
				//Update the card template imagery according to card data
				switch(s.name)
				{
					case "art":
						Sprite art =  Resources.Load<Sprite>("Card_Art/" + player.draw[i].art); 
						s.sprite = art;
						break;
					case "face":
						Sprite face = sprites[player.draw[i].face];
						s.sprite = face;
						break;
					default: 
						break;
				}
			}
			/*Add the fully configured card to the players hand (hand in this case is a 
			data object, as card is already in the UI canvas) */
			player.hand.Add(player.draw[i]);
		}
		//Once all cards have been added to the players hand, remove those cards from the draw pile
		for(int i = 0; i < amount; i++)
		{
			player.draw.RemoveAt(0);
		}
	}
}