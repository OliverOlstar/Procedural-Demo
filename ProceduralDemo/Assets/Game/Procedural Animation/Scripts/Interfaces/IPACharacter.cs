using UnityEngine;

public interface IPACharacter
{
	public Vector3 Up { get; }
	public Vector3 Position { get; }
	public Vector3 Forward { get; }
	public Quaternion Rotation { get; }
	public Vector3 TransformPoint(in Vector3 pVector);
	public Vector3 InverseTransformPoint(in Vector3 pVector);

	public Vector3 Veclocity { get; }
}
