using BandoWare.GameplayTags;
using ODev.Input;

public interface ICharacterAbility
{
	public bool IsActive { get; }
	public IInputTrigger InputActivate { get; }
	public SOCharacterAbility Data { get; }
	public GameplayTagContainer Tags { get; }

	/// <summary> Calls Activate() if CanActivate() is true </summary>
	public bool TryActivate(IReadOnlyGameplayTagContainer pActiveTags, IReadOnlyGameplayTagContainer pBlockedTags);
	public bool TryActivateUpdate(IReadOnlyGameplayTagContainer pActiveTags, IReadOnlyGameplayTagContainer pBlockedTags);
	public void Deactivate();
	public void ActiveTick(float pDeltaTime);
	public void SystemsTick(float pDeltaTime);
	public void Destory();

	public void TryCancel(IReadOnlyGameplayTagContainer pActiveTags, IReadOnlyGameplayTagContainer pCancelTags);
	public void GetTags(out IReadOnlyGameplayTagContainer oTags, out IReadOnlyGameplayTagContainer oCancelTags, out IReadOnlyGameplayTagContainer oBlockTags);
}
