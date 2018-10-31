using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyData  
{

	public string enemyName;
	public int health;
	public int shields;
	public string type;
	public string backRowBonusText;
	public string art;
	public List<EnemyAttackData> attacks;

}
