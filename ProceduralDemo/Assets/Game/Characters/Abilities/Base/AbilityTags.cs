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
	Combat		= (1 << 20),
	Attack		= (1 << 21),
}