using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
	public class CoreWebUtil
	{
		public static void OpenEmail(string address, string subject, string body)
		{

			subject = MakeSafeString(subject);
			body = MakeSafeString(body);
			OpenWebpage(Str.Build("mailto:", address, "?subject=", subject, "&body=", body));
		}

		public static void OpenWebpage(string address)
		{
			Application.OpenURL(address);
		}

		public static string MakeSafeString(string unSafeString)
		{
			return UnityWebRequest.EscapeURL(unSafeString).Replace("+", "%20");
		}


		public static void ShowRate()
		{
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=" + Application.identifier);
#elif UNITY_IOS
        if(!UnityEngine.iOS.Device.RequestStoreReview())
        {
            Application.OpenURL("itms-apps://itunes.apple.com/app/id" + Application.identifier);
        }
#endif
		}
		
		public static string GetMd5Hash(MD5 md5Hash, string input)
		{
			// Convert the input string to a byte array and compute the hash.
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new();

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}
	}
}
