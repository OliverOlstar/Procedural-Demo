using UnityEngine;

namespace RootMotion.FinalIK
{

	/// <summary>
	/// A wrapper for making IKSolverVRArm work with other IK components.
	/// </summary>
	[System.Serializable]
	public class IKSolverArm : IKSolver {

		[Range(0f, 1f)]
		public float IKRotationWeight = 1f;
		/// <summary>
		/// The %IK rotation target.
		/// </summary>
		public Quaternion IKRotation = Quaternion.identity;

		public Point chest = new();
		public Point shoulder = new();
		public Point upperArm = new();
		public Point forearm = new();
		public Point hand = new();

		public bool isLeft;
		
		public IKSolverVR.Arm arm = new();

		private Vector3[] positions = new Vector3[6];
		private Quaternion[] rotations = new Quaternion[6];

		public override bool IsValid(ref string message) {
			if (chest.transform == null || shoulder.transform == null || upperArm.transform == null || forearm.transform == null || hand.transform == null) {
				message = "Please assign all bone slots of the Arm IK solver.";
				return false;
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[5] { chest.transform, shoulder.transform, upperArm.transform, forearm.transform, hand.transform });
			if (duplicate != null) {
				message = duplicate.name + " is represented multiple times in the ArmIK.";
				return false;
			}

			return true;
		}

        /// <summary>
        /// Set IK rotation weight for the arm.
        /// </summary>
        public void SetRotationWeight(float weight)
        {
            IKRotationWeight = weight;
        }

		/// <summary>
		/// Reinitiate the solver with new bone Transforms.
		/// </summary>
		/// <returns>
		/// Returns true if the new chain is valid.
		/// </returns>
		public bool SetChain(Transform chest, Transform shoulder, Transform upperArm, Transform forearm, Transform hand, Transform root) {
			this.chest.transform = chest;
			this.shoulder.transform = shoulder;
			this.upperArm.transform = upperArm;
			this.forearm.transform = forearm;
			this.hand.transform = hand;
			
			Initiate(root);
			return initiated;
		}

		public override Point[] GetPoints() {
			return new Point[5] { (Point)chest, (Point)shoulder, (Point)upperArm, (Point)forearm, (Point)hand };
		}
		
		public override Point GetPoint(Transform transform) {
			if (chest.transform == transform)
			{
				return (Point)chest;
			}

			if (shoulder.transform == transform)
			{
				return (Point)shoulder;
			}

			if (upperArm.transform == transform)
			{
				return (Point)upperArm;
			}

			if (forearm.transform == transform)
			{
				return (Point)forearm;
			}

			if (hand.transform == transform)
			{
				return (Point)hand;
			}

			return null;
		}
		
		public override void StoreDefaultLocalState() {
			shoulder.StoreDefaultLocalState();
			upperArm.StoreDefaultLocalState();
			forearm.StoreDefaultLocalState();
			hand.StoreDefaultLocalState();
		}
		
		public override void FixTransforms() {
			if (!initiated)
			{
				return;
			}

			shoulder.FixTransform();
			upperArm.FixTransform();
			forearm.FixTransform();
			hand.FixTransform();
		}

		protected override void OnInitiate() {
			IKPosition = hand.transform.position;
			IKRotation = hand.transform.rotation;

			Read ();
		}

		protected override void OnUpdate() {
			Read ();
			Solve ();
			Write ();
		}
		
		private void Solve() {
			arm.PreSolve (1f);
			arm.ApplyOffsets(1f);
			arm.Solve (isLeft);
			arm.ResetOffsets ();
		}
		
		private void Read() {
			arm.IKPosition = IKPosition;
			arm.positionWeight = IKPositionWeight;
			arm.IKRotation = IKRotation;
			arm.rotationWeight = IKRotationWeight;

			positions [0] = root.position;
			positions [1] = chest.transform.position;
			positions [2] = shoulder.transform.position;
			positions [3] = upperArm.transform.position;
			positions [4] = forearm.transform.position;
			positions [5] = hand.transform.position;
			
			rotations [0] = root.rotation;
			rotations [1] = chest.transform.rotation;
			rotations [2] = shoulder.transform.rotation;
			rotations [3] = upperArm.transform.rotation;
			rotations [4] = forearm.transform.rotation;
			rotations [5] = hand.transform.rotation;
			
			arm.Read(positions, rotations, false, false, true, false, false, 1, 2);
		}
		
		private void Write() {
			arm.Write (ref positions, ref rotations);
			
			shoulder.transform.rotation = rotations [2];
			upperArm.transform.rotation = rotations [3];
			forearm.transform.rotation = rotations [4];
			hand.transform.rotation = rotations [5];

			forearm.transform.position = positions[4];
			hand.transform.position = positions[5];
		}
	}
}