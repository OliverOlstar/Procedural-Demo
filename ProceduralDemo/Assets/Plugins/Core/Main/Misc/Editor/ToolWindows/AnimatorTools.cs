
using UnityEngine;
using UnityEditor;
 
namespace Core
{
	public class AnimatorTools : EditorWindow
	{
		string m_Text = string.Empty;
	 
		[MenuItem("Core/Animator/Tools")]
		static void CreateWizard()
		{
			AnimatorTools window = GetWindow<AnimatorTools>("Animator Tools");
			window.Show();
		}
	 
		public static string IDToString(int id)
		{
			for (int length = 4; length <= MAX_LENGTH; length++)
			{
				EditorUtility.DisplayProgressBar("Guessing...", length + "/" + MAX_LENGTH, (float)length / MAX_LENGTH);
				char[] str = new char[length];
				if (IDToStringRecursive(id, str))
				{
					EditorUtility.ClearProgressBar();
					return new string(str);
				}
			}

			EditorUtility.ClearProgressBar();
			return "FAILED";
		}

		static readonly int MAX_LENGTH = 6;
		static readonly char[] TEST_CHARS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '_' };
		//static readonly char[] TEST_CHARS = new char[] { '0', '1', '2', '3', '4', '5' };

		static bool IDToStringRecursive(int id, char[] str, int index = 0)
		{
			if (index < str.Length)
			{
				for (int i = 0; i < TEST_CHARS.Length; i++)
				{
					str[index] = TEST_CHARS[i];
					if (IDToStringRecursive(id, str, index+1))
					{
						return true;
					}
				}
			}
			else
			{
				//Debug.Log(new string(str) + " " + index);
				if (id == Animator.StringToHash(new string(str)))
				{
					return true;
				}
			}

			return false;
		}

		void OnGUI()
		{
			m_Text = GUILayout.TextArea(m_Text);
			GUILayout.Label(Animator.StringToHash(m_Text).ToString());
		}
	}
}