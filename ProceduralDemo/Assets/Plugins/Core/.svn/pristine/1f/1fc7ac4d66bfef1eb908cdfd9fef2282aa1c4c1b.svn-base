
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ActTrackClipboard
{
	static List<ActTrack> s_Tracks = new List<ActTrack>();
	public static List<ActTrack> GetTracks() { return s_Tracks; }

	public static bool IsEmpty()
	{
		foreach (ActTrack track in s_Tracks)
		{
			if (track != null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanAdd(ActTrack track)
	{
		foreach (ActTrack t in s_Tracks)
		{
			if (t != null && t.GetInstanceID() == track.GetInstanceID())
			{
				return false;
			}
		}
		return true;
	}

	public static void Add(ActTrack track)
	{
		s_Tracks.Add(track);
	}

	public static void Clear()
	{
		s_Tracks.Clear();
	}

	public static void Paste(int destNodeID, ActTree desTree, ref SerializedObject sDestTree)
	{
		Undo.RecordObject(desTree, "DupTrack");
		foreach (ActTrack track in s_Tracks)
		{
			if (track != null)
			{
				ActTrack dup = Object.Instantiate<ActTrack>(track);
				dup.EditorSetNodeID(destNodeID);
				Undo.RegisterCreatedObjectUndo(dup, "DupTrack");
				AssetDatabase.AddObjectToAsset(dup, desTree);
			}
		}
		Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
		
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(desTree)); // Update AssetDatabase
		ActTrackDrawer.UpdateTrackList(desTree, ref sDestTree);
	}
}
