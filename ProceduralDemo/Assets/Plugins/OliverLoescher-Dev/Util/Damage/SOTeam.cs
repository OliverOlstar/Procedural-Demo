using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Team Data")]
	public class SOTeam : ScriptableObject
	{
		public bool IgnoreTeamCollisions = true;
		[ShowIf(@"ignoreTeamCollisions")]
		public bool TeamDamage = false;

		public static bool Compare(SOTeam teamA, SOTeam teamB)
		{
			if (teamA == null || teamB == null)
			{
				return false;
			}
			return teamA == teamB;
		}
	}
}