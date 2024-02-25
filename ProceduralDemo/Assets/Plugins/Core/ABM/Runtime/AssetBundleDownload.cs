
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Core
{
	public class ForceAcceptAll : CertificateHandler
	{
		protected override bool ValidateCertificate(byte[] certificateData) => true;
	}

	public class AssetBundleDownload : IMessageData
	{		
		private static readonly float RETRY_TIME = 2.0f;

		string m_AssetBundleName = null;
		public string GetAssetBundleName() { return m_AssetBundleName; }
		string m_Url = string.Empty;
		public string GetSourceURL() { return m_Url; }

		// These memebers are primarily for analytics
		DownloadSpeedMonitor m_DownloadSpeed = new DownloadSpeedMonitor();
		public DownloadSpeedMonitor Speed => m_DownloadSpeed;
		string m_GUID = Core.Str.EMPTY;
		public string GUID => m_GUID;
		string m_Error = Core.Str.EMPTY;
		public string Error => m_Error;

		UnityWebRequest m_Request = null;
		public UnityWebRequest GetRequest() { return m_Request; }
		AssetBundle m_AssetBundle = null;
		public AssetBundle GetAssetBundle() { return m_AssetBundle; }
		Hash128 m_Hash = new Hash128();
		public Hash128 GetHash() { return m_Hash; }
		SafeTimer m_RetryTimer = new SafeTimer();
		ulong m_Progress = 0UL;
		public ulong GetDownloadedBytes() { return m_Progress; }
		ulong m_ExpectedSize = 0;
		public ulong GetTotalBytes() { return m_ExpectedSize; }
		float m_ElapsedTime = 0.0f;
		public float GetTimer() { return m_ElapsedTime; }
		int m_Frames = 0;
		int m_RequestCount = 0;

		private AssetBundleDownloader.Priority m_DownloadPriority = AssetBundleDownloader.Priority.None;
		public AssetBundleDownloader.Priority DownloadPriority => m_DownloadPriority;
		public bool RequiresDownload() => m_DownloadPriority != AssetBundleDownloader.Priority.None;
		public void SetDownloadPriority(AssetBundleDownloader.Priority priority) { m_DownloadPriority = priority; }

		public int RequestCount => m_RequestCount;
		bool m_Started = false;
		public bool Started() { return m_Started; }
		bool m_Done;
		public bool IsDone() { return m_Done; }

		public AssetBundleDownload(string assetBundleName, string url, Hash128 hash, ulong expectedSize, bool requiresDownload)
		{
			m_AssetBundleName = assetBundleName;
			m_Url = url;
			m_Hash = hash;
			m_ExpectedSize = expectedSize;
			m_DownloadPriority = requiresDownload ? AssetBundleDownloader.Priority.BackgroundRandom : AssetBundleDownloader.Priority.None;
		}

		public void Abort()
		{
			if (m_Request != null)
			{
				m_Request.Abort();
				m_Request.Dispose();
				m_Request = null;
			}
			m_Done = true;
		}

		public void Update()
		{
			if (m_Done)
			{
				return;
			}
			TrySendRequest();
			UpdateRequest();
		}

		private void TrySendRequest()
		{
			if (m_Request != null)
			{
				return;
			}
			// Set progress but don't actually try to send a request unless we think it can succeed
			m_Started = true;
			m_Frames = 0;
			m_Progress = 0UL;
			m_DownloadSpeed.Reset();
			m_GUID = System.Guid.NewGuid().ToString(); // For analytics
			m_Error = Core.Str.EMPTY;
			if (!m_RetryTimer.IsDone())
			{
				m_RetryTimer.Update();
				if (!m_RetryTimer.IsDone())
				{
					return;
				}
			}
			if (RequiresDownload() && InternetReachabilityDirector.InternetReachability == NetworkReachability.NotReachable)
			{
				return;
			}
			m_RequestCount++;
			if (m_Hash.isValid)
			{
				m_Request = UnityWebRequestAssetBundle.GetAssetBundle(
					m_Url,
					new CachedAssetBundle(m_AssetBundleName, m_Hash),
					0);
			}
			else
			{
				// This should be the manifest file
				m_Request = UnityWebRequestAssetBundle.GetAssetBundle(m_Url);
			}
			m_Request.certificateHandler = new ForceAcceptAll();
			m_Request.disposeCertificateHandlerOnDispose = true;
			m_Request.SendWebRequest();
			if (RequiresDownload())
			{
				ABMMessage.Route(ABMMessage.Type.DownloadStart, this);
			}
		}

		private void UpdateProgress()
		{
			if (!RequiresDownload())
			{
				return;
			}
			m_ElapsedTime += Time.unscaledDeltaTime;
			ulong progress = m_Request.downloadedBytes;
			m_DownloadSpeed.Update(m_ElapsedTime, progress);
			if (progress != m_Progress) // Record new progress and return
			{
				m_Progress = progress;
				m_Frames = 0;
				return;
			}
			// Progress hasn't changed increment frame counter to time how long the download is stuck for
			m_Frames++; // Use frames because we often get huge time spikes when loading, a single frame could take 5s
			if (m_Frames > 300) // ~10 seconds, we've been stuck for too long retry
			{
				Failed(true, Core.Str.Build(m_AssetBundleName,
					" AssetBundleDownload.Update() Timed out waiting for byte ",
					progress.ToString(), " (", Core.UIUtil.BytesToMB(progress), "MB) / ",
					m_ExpectedSize.ToString(), " (", Core.UIUtil.BytesToMB(m_ExpectedSize), "MB)",
					" progress: ", m_Request.downloadProgress.ToString(),
					" time: ", m_ElapsedTime.ToString(),
					" frames: ", m_Frames.ToString(),
					" requests: ", m_RequestCount.ToString()));
				return;
			}
			if (Core.DebugOptions.ABMLogs.IsSet() &&
				m_Frames % 15 == 0 && // Time slice warnings so they aren't too spammy
				(m_Frames >= 150 || m_RequestCount > 1)) // Warn every ~0.5 seconds after ~5 seconds or if the bundle has had multiple retries
			{
				Debug.Log(Core.Str.Build(m_AssetBundleName,
					" AssetBundleDownload.Update() Waiting on byte ",
					progress.ToString(), " (", Core.UIUtil.BytesToMB(progress), "MB) / ",
					m_ExpectedSize.ToString(), " (", Core.UIUtil.BytesToMB(m_ExpectedSize), "MB)",
					" progress: ", m_Request.downloadProgress.ToString(),
					" time: ", m_ElapsedTime.ToString(),
					" frames: ", m_Frames.ToString(),
					" requests: ", m_RequestCount.ToString()));
			}
		}

		private void UpdateRequest()
		{
			if (m_Request == null)
			{
				return;
			}
			UpdateProgress();
			if (m_Request == null || !m_Request.isDone) // Requests that timeout during UpdateProgress() will get set null
			{
				return;
			}
			if (m_Request.result == UnityWebRequest.Result.ConnectionError)
			{
				Failed(true, Core.Str.Build(m_AssetBundleName,
					" AssetBundleDownload.UpdateDownload() ", m_Request.url,
					" download error: ", m_Request.error));
				return;
			}
			// Response code 400+ indicates an HTML error
			// If the assetbundle url is wrong isError is false, but the response code is 404 page not found
			if (m_Request.responseCode >= 400)
			{
				Failed(false, Core.Str.Build(m_AssetBundleName,
					" AssetBundleDownload.UpdateDownload() ", m_Request.url,
					" response code: ", m_Request.responseCode.ToString()));
				return;
			}

			if (!AssetBundleUtil.IsSimMode() && ABMs.IsBundleLoaded(m_AssetBundleName)) // sim mode always thinks a bundle is loaded
			{
				if (Core.DebugOptions.ABMLogs.IsSet())
				{
					Debug.LogFormat("{0} AssetBundleDownload.UpdateDownload() Asset Bundle finished download but was already loaded! Using loaded version.", m_AssetBundleName);
				}
				m_AssetBundle = ABMs.GetLoadedBundle(m_AssetBundleName);
			}
			else
			{
				m_AssetBundle = DownloadHandlerAssetBundle.GetContent(m_Request);
			}
			if (m_AssetBundle == null)
			{
				Failed(false, Core.Str.Build(m_AssetBundleName, " AssetBundleDownload.UpdateDownload() Asset bundle is null"));
				return;
			}

			m_Request.Dispose();
			m_Request = null;
			m_Done = true;
			if (RequiresDownload())
			{
				m_DownloadSpeed.Finish(m_ElapsedTime, m_Progress);
				ABMMessage.Route(ABMMessage.Type.DownloadComplete, this);
			}
		}

		private void Failed(bool warning, string error)
		{
			m_RetryTimer.InitTimer(RETRY_TIME);
			m_Request.Dispose();
			m_Request = null;
			m_Error = error;
			if (warning)
			{
				Debug.LogWarning(m_Error);
			}
			else
			{
				Core.DebugUtil.DevException(m_Error);
			}
			if (RequiresDownload())
			{
				ABMMessage.Route(ABMMessage.Type.DownloadFail, this);
			}
		}

		public override string ToString()
		{
			string s = m_AssetBundleName + "-Download: ";
			if (!m_Started)
			{
				s += "Queued";
				return s;
			}
			if (m_Request != null)
			{
				if (m_Request.isDone)
				{
					s += "Done";
					return s;
				}
				return s + m_Request.downloadProgress.ToString("0.0");
			}
			return s;
		}
	}
}
