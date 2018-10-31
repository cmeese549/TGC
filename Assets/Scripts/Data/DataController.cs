using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/* 	UNIVERSAL JSON INTERFACE */
public class DataController : MonoBehaviour 
{
	//Setup the objects we will create from JSON
	public List<CardData> cards;
	public PlayerData player;
	public EnemyData enemy;
	public ClassData classData;
	//These are file paths set in the unity editor used to locate the JSON files
	public string classPath;
	public string cardDataPrefix;
	public string playerDataPrefix;
	public string enemyDataPrefix;
	public string classDataPrefix;
	
	//Creates session persistent object dataController for us to use
	void Start () 
	{
		DontDestroyOnLoad (gameObject);
		SceneManager.LoadScene ("Menu");	
	}

	//Because the class is insantiated first in our game world, we will load all game data along with returning the class data
	public ClassData GetClassData()
	{
		classDataPrefix = "classes/" + classPath;
		LoadClassData();
		LoadPlayerData();
		LoadCardData();
		return classData;
	}

	//Called once at the start of each scene to get player data
	public PlayerData GetPlayerData()
	{
		return player;
	}

	//Called once at the start of each scene to get all card data
	public List<CardData> GetDeck()
	{
		return cards;
	}

	//Called once at the start of each scene to load that scenes enemy
	public EnemyData GetEnemyData(string enemyName)
	{
		LoadEnemyData(enemyName);
		return enemy;
	}

	//Go through the Streaming Assets folder and load all class data
	private void LoadClassData()
	{
		//Read the classData.json file and create ClassData from it
		string filepath = Path.Combine(Application.streamingAssetsPath, classDataPrefix);
		DirectoryInfo dir = new DirectoryInfo (filepath);
		string jsonData = File.ReadAllText(Path.Combine(filepath, "classData.json"));
		classData = JsonUtility.FromJson<ClassData>(jsonData);
		//Look through all the card files and create CardData lists from them
		string classPath = classDataPrefix + "/cards";
		filepath = Path.Combine(Application.streamingAssetsPath, classPath);
		dir = new DirectoryInfo (filepath);
		FileInfo[] cardFiles  = dir.GetFiles("*.json");
		for(int i = 0 ; i < cardFiles.Length; i++)
		{
			//Ignore Unity meta files
			if(cardFiles[i].Extension != ".meta")
			{
				//Add card to the master list of cards, and the current instance of ClassData
				jsonData = File.ReadAllText(cardFiles[i].FullName);
				CardData cardToAdd = JsonUtility.FromJson<CardData>(jsonData);
				cards.Add(cardToAdd);
				classData.cards.Add(cardToAdd);
				//Add special cards and starter cards to unique lists
				if(cardToAdd.isSpecial)
				{
					classData.specialCards.Add(cardToAdd);
				}
				if(cardToAdd.isStarter)
				{
					classData.starterCards.Add(cardToAdd.starterData);
				}
			}
		}
	}

	//Go through the Streaming Assets folder for all card data
	private void LoadCardData()
	{
		//Read through all card files
		string filepath = Path.Combine(Application.streamingAssetsPath, cardDataPrefix);
		DirectoryInfo dir = new DirectoryInfo (filepath);
		FileInfo[] cardFiles = dir.GetFiles("*.json*");
		int count = dir.GetFiles().Length;
		for(int i = 0; i < count; i++)
		{	
			//Ignore Unity meta files
			if(cardFiles[i].Extension != ".meta")
			{
				//Add card to the master list of cards
				string jsonData = File.ReadAllText(cardFiles[i].FullName);
				cards.Add(JsonUtility.FromJson<CardData>(jsonData));
			}
		}
	}

	//Go through the Streaming Assets folder for all player data (future home of save games)
	private void LoadPlayerData()
	{
		//Read the playerData.json file and create PlayerData from it
		string filepath = Path.Combine(Application.streamingAssetsPath, playerDataPrefix);
		string jsonData = File.ReadAllText(Path.Combine(filepath, "playerData.json"));
		player = JsonUtility.FromJson<PlayerData>(jsonData);
		//Read through all deck data files
		filepath = Path.Combine(Application.streamingAssetsPath, playerDataPrefix);
		DirectoryInfo dir = new DirectoryInfo (Path.Combine(filepath, "deckData"));
		FileInfo[] deckFile = dir.GetFiles("*.json*");
		int count = dir.GetFiles().Length;
		for(int i = 0; i < count; i++)
		{
			//Ignore Unity meta files
			if(deckFile[i].Extension != ".meta")
			{
				//Create an instance of DeckBuilder for each deck data file in the players directory
				jsonData = File.ReadAllText(deckFile[i].FullName);
				player.deck.Add(JsonUtility.FromJson<DeckBuilder>(jsonData));
			}
		}
		//Load class specific starter cards into newly created player deck
		foreach(DeckBuilder card in classData.starterCards)
		{
			player.deck.Add(card);
		}	
	}

	//Go through the Streaming Assets folder to load enemy data
	private void LoadEnemyData(string enemyName)
	{
		//Read the enemyData.json file land create EnemyData from it
		string filepath = Path.Combine(Application.streamingAssetsPath, enemyDataPrefix);
		filepath = Path.Combine(filepath, enemyName);
		string jsonData = File.ReadAllText(Path.Combine(filepath, "enemyData.json"));
		enemy = JsonUtility.FromJson<EnemyData>(jsonData);
		//Read through all enemy attacks
		DirectoryInfo dir = new DirectoryInfo (Path.Combine(filepath, "attacks"));
		FileInfo[] deckFile = dir.GetFiles("*.json*");
		int count = dir.GetFiles().Length;
		for(int i = 0; i < count; i++)
		{
			//Ignore Unity meta files
			if(deckFile[i].Extension != ".meta")
			{
				//Add attack to list of EnemyAttackData
				jsonData = File.ReadAllText(deckFile[i].FullName);
				enemy.attacks.Add(JsonUtility.FromJson<EnemyAttackData>(jsonData));
			}
		}
	}
	
}
