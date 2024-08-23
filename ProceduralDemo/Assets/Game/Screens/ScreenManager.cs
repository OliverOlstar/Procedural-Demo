using System;
using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class ScreenManager : Singleton<ScreenManager>
{
	private readonly Dictionary<Type, ScreenBase> m_Screens = new();

	public static bool TryGet<T>(out T oScreen) where T : ScreenBase
	{
		if (!Instance.m_Screens.TryGetValue(typeof(T), out ScreenBase screen))
		{
			oScreen = null;
			return false;
		}
		oScreen = (T)screen;
		return true;
	}

	public static void RegisterScreen(ScreenBase pScreen) => Instance?.RegisterScreenInternal(pScreen);
	public static void DeregisterScreen(ScreenBase pScreen) => Instance?.DeregisterScreenInternal(pScreen);

	private void RegisterScreenInternal(ScreenBase pScreen)
	{
		if (pScreen == null)
		{
			DevException(new NullReferenceException("pScreen can not be null"));
			return;
		}
		Type type = pScreen.GetType();
		if (m_Screens.ContainsKey(type))
		{
			LogError($"Screen type {type} is already registered, deleting duplicate screen");
			UnityEngine.Object.Destroy(pScreen);
			return;
		}
		m_Screens.Add(type, pScreen);
	}

	private void DeregisterScreenInternal(ScreenBase pScreen)
	{
		if (pScreen == null)
		{
			DevException(new NullReferenceException("pScreen can not be null"));
			return;
		}
		Type type = pScreen.GetType();
		if (!m_Screens.Remove(type))
		{
			DevException($"Screen type {type} wasn't registered");
		}
	}
}
