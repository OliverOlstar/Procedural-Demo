using ODev.Input;

public interface ICharacterAbility
{
	public bool IsActive { get; }
	public IInputTrigger InputActivate { get; }

	/// <summary> Calls Activate() if CanActivate() is true </summary>
	public bool TryActivate(AbilityTags pActiveTags, AbilityTags pBlockedTags);
	public bool TryActivateUpdate(AbilityTags pActiveTags, AbilityTags pBlockedTags);
	public void Deactivate();
	public void ActiveTick(float pDeltaTime);
	public void SystemsTick(float pDeltaTime);
	public void Destory();

	public void TryCancel(AbilityTags pActiveTags, AbilityTags pCancelTags);
	public void AddTags(ref AbilityTags rActiveTags, ref AbilityTags rBlockedTags);
	public void GetTags(out AbilityTags oTags, out AbilityTags oCancelTags);
}
