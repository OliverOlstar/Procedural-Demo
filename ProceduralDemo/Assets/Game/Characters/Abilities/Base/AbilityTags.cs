using System;

[Flags]
public enum AbilityTags
{
	Passive		= (1 << 9),
	Locomotion	= (1 << 0),
	Sprint		= (1 << 1),
	Crouch		= (1 << 2),
	Slide		= (1 << 3),
	Jump		= (1 << 4),
	GroundJump	= (1 << 5),
	WallJump	= (1 << 6),
	Glide		= (1 << 7),
	Mantle 		= (1 << 8),
	WallCling	= (1 << 9),
	Combat		= (1 << 20),
	Spear		= (1 << 10),
	SpearJump	= (1 << 11),
	SpearPull	= (1 << 12),
	SpearThrow	= (1 << 13),
	SpearGrapple= (1 << 14),
	Attack		= (1 << 21),
}

public static class AbilityTagsExtentions
{
	public static bool HasAnyFlag(this AbilityTags pTags, AbilityTags pOtherTags)
	{
		return pTags != 0 && pOtherTags != 0 && (pOtherTags & pTags) != 0;
	}
}