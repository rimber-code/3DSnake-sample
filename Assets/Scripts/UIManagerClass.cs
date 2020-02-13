//
//  UIManagerClass.cs
//
//  Author:
//       Rimber <r1mb3r@gmail.com>
//
//  Copyright (c) 2015 Rimber
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Simple UI manager class to handle UI states.
public class UIManagerClass : MonoBehaviour
{
	[Header("Screens")]
	[SerializeField] private GameObject m_TitleScreen;
	[SerializeField] private GameObject m_GameScreen;
	[Header("UI Elements")]
	[SerializeField] private Text m_GameOverText;
	
	private void Start()
	{
		// Map actions for smooth hands-off transitions.
		CoreClass.Instance.OnGameStart += OnGameStart;
		CoreClass.Instance.OnGameOver += OnGameOver;
	}
	
	private void OnDestroy()
	{
		if(CoreClass.Instance == null)
			return;
			
		// Cleanup actions to avoid a mess.
		CoreClass.Instance.OnGameStart -= OnGameStart;
		CoreClass.Instance.OnGameOver -= OnGameOver;
	}
	
	// Callback for when game starts.
	private void OnGameStart()
	{
		m_TitleScreen.SetActive(false);
		m_GameScreen.SetActive(true);
	}
	
	// Callback for when game ends.
	private void OnGameOver(string reason)
	{
		// Let player know what happened.
		m_GameOverText.text = reason;
		
		m_TitleScreen.SetActive(true);
		m_GameScreen.SetActive(false);
	}
}
