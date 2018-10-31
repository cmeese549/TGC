using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class PlayerData  {

	public string playerName;
	public int health;
	public int shields;
	public int energy;
	public int deckLength;
	public List<DeckBuilder> deck;

}
