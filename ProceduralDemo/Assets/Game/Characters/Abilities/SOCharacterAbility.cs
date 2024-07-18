using UnityEngine;

public abstract class SOCharacterAbility : ScriptableObject
{
	public abstract ICharacterAbility CreateInstance(PlayerRoot pPlayer);
}

public interface ICharacterAbility
{
	public void Destory();
}

public abstract class CharacterAbility<TData> : ICharacterAbility where TData : SOCharacterAbility
{
	private readonly PlayerRoot m_Player = null;
	private readonly TData m_Data = null;

	public PlayerRoot Player => m_Player;
	public TData Data => m_Data;

	public CharacterAbility(PlayerRoot pPlayer, TData pData)
	{
		m_Player = pPlayer;
		m_Data = pData;
		Initalize();
	}
	public abstract void Initalize();
	public abstract void Destroy();
	void ICharacterAbility.Destory() => Destroy();
}