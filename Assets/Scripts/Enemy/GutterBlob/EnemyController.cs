using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* CONTROLLER FOR GUTTER BLOB */
public class EnemyController : MonoBehaviour {

	//Enemy data that gets set from JSON
	public EnemyData enemyData;
	//Gutter Blob specific back row damage bonus
	public int fisrtAttackDamageBonus;
	//Main game canvas, and victory screen canvas
	public GameObject canvas;
	public GameObject victory;
	//Bool for tracking back row damage bonus
	public bool damageBonus;

	//Various UI text displays
	private Text enemyHealth;
	private Text playerInfo;
	private Text brbDisplay;
	private Text enemyShields;
	private Text enemyDamageDisplay;
	//Player Controller
	private PlayerController player;
	//Generic gameobject used for component lookups
	private GameObject go;
	
	void Start () 
	{	
		//Set back row damage bonus to true
		damageBonus = true;
		//Set the Player Controller
		go = GameObject.Find("Player");
		player = (PlayerController) go.GetComponent(typeof(PlayerController));
		//Set various UI text displays
		go = GameObject.Find("enemyHealth");
		enemyHealth = (Text) go.GetComponent(typeof(Text));
		go = GameObject.Find("brbDisplay");
		brbDisplay = (Text) go.GetComponent(typeof(Text));
		brbDisplay.text = enemyData.backRowBonusText;
		go = GameObject.Find("enemyShields");
		enemyShields = (Text) go.GetComponent(typeof(Text));
		enemyShields.text = enemyData.shields.ToString();
		go = GameObject.Find("enemyDamageDisplay");
		enemyDamageDisplay = (Text) go.GetComponent(typeof(Text));
		//Update with starting health value
		Updatehealth(enemyData.health);
	}

	//Updates the enemy health, and handles win condition
	public void Updatehealth(int x)
	{
		//If the enemy has no health left and the player won
		if(x <= 0)
		{
			//Disable main canvas and enable victory screen
			canvas.SetActive(false);
			victory.SetActive(true);
		}
		else
		{	
			//Update health and health display
			enemyData.health = x;
			enemyHealth.text = enemyData.health.ToString();
		}
	}

	//Update shields and shield display
	public void UpdateShields(int x)
	{
		enemyData.shields = x;
		enemyShields.text = enemyData.shields.ToString();
	}

	//Lost shields
	public void LoseShields(int x)
	{
		enemyData.shields -= x;
		UpdateShields(enemyData.shields);
	}
	
	//Add shields (useful comment)
	public void GainShields(int x)
	{
		enemyData.shields += x;
		UpdateShields(enemyData.shields);
	}

	//Take damage, called in Card Controller
	public void TakeDamage(int x)
	{
		//Add 10 bonus damage if applicable and player is in the back row
		if(damageBonus && player.row == 0)
		{
			x += 10;
			damageBonus = false;
		}
		enemyData.health -= x;
		//Make sure the enemy didn't die
		if(enemyData.health > 0)
		{
			//No death, update health and the damage display
			Updatehealth(enemyData.health);
			EnemyDamageDisplay("-" + x.ToString());		
		}
		else
		{
			//Death happened, show victory screen
			canvas.SetActive(false);
			victory.SetActive(true);
		}
	}

	//Display something in the "damage taken" area
	public void EnemyDamageDisplay(string x)
	{
		enemyDamageDisplay.text = x;
		//Clear the display after two seconds
		StartCoroutine(ClearInfo(2, x, enemyDamageDisplay));
	}

	IEnumerator ClearInfo(int amt, string x, Text t)
    {
        yield return new WaitForSeconds(amt);
		//Wait however long, then clear the display (unless it has been update while this was waiting, then it won't clear it)
		if(t.text == x)
		{
			t.text = "";
		}
	}

}
