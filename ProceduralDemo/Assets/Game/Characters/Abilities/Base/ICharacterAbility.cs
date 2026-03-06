using BandoWare.GameplayTags;
using ODev.Input;

public interface ICharacterAbility
{
	public bool IsActive { get; }
	public IInputTrigger InputActivate { get; }

	/// <summary> Calls Activate() if CanActivate() is true </summary>
	public bool TryActivate(GameplayTagContainer pActiveTags, GameplayTagContainer pBlockedTags);
	public bool TryActivateUpdate(GameplayTagContainer pActiveTags, GameplayTagContainer pBlockedTags);
	public void Deactivate();
	public void ActiveTick(float pDeltaTime);
	public void SystemsTick(float pDeltaTime);
	public void Destory();

	public void TryCancel(GameplayTagContainer pActiveTags, GameplayTagContainer pCancelTags);
	public void AddTags(ref GameplayTagContainer rActiveTags, ref GameplayTagContainer rBlockedTags);
	public void GetTags(out GameplayTagContainer oTags, out GameplayTagContainer oCancelTags);
}
