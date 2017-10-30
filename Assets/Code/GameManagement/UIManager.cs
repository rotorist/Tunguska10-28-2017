﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager
{
	public UIRoot Root;
	public Camera UICamera;
	public UIStateMachine UIStateMachine;

	public FadingPanel FadingPanel;
	public BarkPanel BarkPanel;
	public HUDPanel HUDPanel;
	public WindowPanel WindowPanel;
	public DialoguePanel DialoguePanel;
	public RestingPanel RestingPanel;
	public ConfirmPanel ConfirmPanel;
	public QuestDebugPanel QuestDebugPanel;
	public MapPanel MapPanel;
	public IntroPanel IntroPanel;
	public TravelPanel TravelPanel;

	public float UIZoom;
	public bool IsInHUDRegion;

	private List<PanelBase> _panels;

	public void Initialize()
	{
		_panels = new List<PanelBase>();

		GameObject uiRootObj = GameObject.Instantiate(Resources.Load("UI Root")) as GameObject;

		Root = uiRootObj.GetComponent<UIRoot>();

		Root.manualHeight = Screen.height;
		Root.manualWidth = Screen.width;

		UICamera = Root.transform.Find("UICamera").GetComponent<Camera>();


		BarkPanel = UICamera.transform.Find("BarkPanel").GetComponent<BarkPanel>();
		BarkPanel.Initialize();

		HUDPanel = UICamera.transform.Find("HUDPanel").GetComponent<HUDPanel>();
		HUDPanel.Initialize();

		WindowPanel = UICamera.transform.Find("WindowPanel").GetComponent<WindowPanel>();
		WindowPanel.Initialize();

		DialoguePanel = UICamera.transform.Find("DialoguePanel").GetComponent<DialoguePanel>();
		DialoguePanel.Initialize();

		RestingPanel = UICamera.transform.Find("RestingPanel").GetComponent<RestingPanel>();
		RestingPanel.Initialize();

		ConfirmPanel = UICamera.transform.Find("ConfirmPanel").GetComponent<ConfirmPanel>();
		ConfirmPanel.Initialize();

		FadingPanel = UICamera.transform.Find("FadingPanel").GetComponent<FadingPanel>();
		FadingPanel.Initialize();
		FadingPanel.Show();
		FadingPanel.FadeIn(2);

		QuestDebugPanel = UICamera.transform.Find("QuestDebugPanel").GetComponent<QuestDebugPanel>();
		QuestDebugPanel.Initialize();

		MapPanel = UICamera.transform.Find("MapPanel").GetComponent<MapPanel>();
		MapPanel.Initialize();

		IntroPanel = UICamera.transform.Find("IntroPanel").GetComponent<IntroPanel>();
		IntroPanel.Initialize();

		TravelPanel = UICamera.transform.Find("TravelPanel").GetComponent<TravelPanel>();
		TravelPanel.Initialize();

		_panels.Add(IntroPanel);
		_panels.Add(MapPanel);
		_panels.Add(QuestDebugPanel);
		_panels.Add(DialoguePanel);
		_panels.Add(RestingPanel);
		_panels.Add(ConfirmPanel);
		_panels.Add(WindowPanel);
		_panels.Add(HUDPanel);
		_panels.Add(BarkPanel);
		_panels.Add(TravelPanel);


		UIZoom = 1;


		UIStateMachine = new UIStateMachine();
		UIStateMachine.Initialize();
	}

	public void PerFrameUpdate()
	{
		BarkPanel.PerFrameUpdate();
		HUDPanel.PerFrameUpdate();

		if(WindowPanel.IsActive)
		{
			WindowPanel.PerFrameUpdate();
		}

		if(DialoguePanel.IsActive)
		{
			DialoguePanel.PerFrameUpdate();
		}

		if(FadingPanel.IsActive)
		{
			FadingPanel.PerFrameUpdate();
		}

		if(RestingPanel.IsActive)
		{
			RestingPanel.PerFrameUpdate();
		}

		if(QuestDebugPanel.IsActive)
		{
			QuestDebugPanel.PerFrameUpdate();
		}

		if(IntroPanel.IsActive)
		{
			IntroPanel.PerFrameUpdate();
		}

		UpdateUIZoom();
	}


	public bool IsCursorInHUDRegion()
	{
		return false;
		/*
		Vector3 cursorLoc = Input.mousePosition; 
		Vector3 worldPos = UICamera.ScreenToWorldPoint(cursorLoc);
		Vector3 localPos = HUDPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPos);
		Vector3 rightHUDLoc = HUDPanel.RightHUDAnchor.localPosition;

		if(localPos.x > rightHUDLoc.x - 282 && localPos.y < rightHUDLoc.y + 188)
		{
			IsInHUDRegion = true;
			return true;
		}

		IsInHUDRegion = false;
		return false;
		*/
	}

	public int GetScreenHeight()
	{
		float ratio = (float)Root.activeHeight / Screen.height;
		return Mathf.CeilToInt(Screen.height * ratio);

	}

	public int GetScreenWidth()
	{
		float ratio = (float)Root.activeHeight / Screen.height;
		return Mathf.CeilToInt(Screen.width * ratio);

	}


	public void HideAllPanels()
	{
		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				panel.Hide();
			}
		}
	}

	public void SetConsoleText(string text)
	{
		HUDPanel.SetConsoleText(text, new Color(0.384f, 0.7f, 0.23f));
	}

	public void SetAnnouncementText(string text)
	{
		HUDPanel.SetConsoleText(text, new Color(0.8f, 0.8f, 0.95f));
	}

	private void UpdateUIZoom()
	{
		if(Input.GetKeyDown(KeyCode.Equals))
		{
			UIZoom += 0.1f;
			HUDPanel.UpdateScaling();
		}

		if(Input.GetKeyDown(KeyCode.Minus))
		{
			UIZoom -= 0.1f;
			if(UIZoom < 0.3f)
			{
				UIZoom = 0.3f;
			}
			HUDPanel.UpdateScaling();
		}
	}
}
