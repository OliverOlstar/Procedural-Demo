using UnityEngine;

public abstract class SOCharacterAbility : ScriptableObject
{
	public abstract void Initalize(PlayerRoot pPlayer);
	public abstract void Destory();
}
