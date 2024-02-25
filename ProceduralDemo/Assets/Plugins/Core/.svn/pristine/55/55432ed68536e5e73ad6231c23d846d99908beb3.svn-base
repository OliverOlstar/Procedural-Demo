
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class ABMMessage : Messaging<ABMMessage.Type>
	{
		public enum Type
		{
			DownloadStart,
			DownloadFail,
			DownloadComplete
		}

		public class FailedData : IMessageData
		{
			public string Error { get; private set; }
			public AssetBundleDownload Download { get; private set; }

			public FailedData(string error, AssetBundleDownload download)
			{
				Error = error;
				Download = download;
			}
		}
	}
}
