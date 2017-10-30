using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class TravelPanel : PanelBase
{
	public UIButton GoButton;
	public LevelPortal CurrentPortal;
	public UILabel LevelName;
	public UILabel LevelDescription;
	public UILabel Distance;
	public UILabel Arrival;

	private float _hours;

	public override void Initialize ()
	{

		Hide();
	}

	public override void PerFrameUpdate ()
	{

	}

	public override void Show ()
	{
		Camera.main.GetComponent<BlurOptimized>().enabled = true;

		Time.timeScale = 0;

		NGUITools.SetActive(this.gameObject, true);
		this.IsActive = true;

		string levelDisplayName;
		string levelDesc;
		if(CurrentPortal.IsAccessible)
		{
			GameManager.Inst.DBManager.DBHandlerEnvironment.LoadLevelData(CurrentPortal.NextLevelName, out levelDisplayName, out levelDesc);
			LevelName.text = levelDisplayName;
			LevelDescription.text = levelDesc;
			Distance.text = CurrentPortal.TravelDistance + " km";
			_hours = GameManager.Inst.WorldManager.GetTravellingTime(CurrentPortal.TravelDistance * 1f);
			Arrival.text = _hours.ToString() + " Hours";
			//NGUITools.SetActive(GoButton.gameObject, true);
			GoButton.isEnabled = true;
		}
		else
		{
			LevelName.text = "Unknown";
			LevelDescription.text = "Unknown";
			Distance.text = "--";
			Arrival.text = "--";
			//NGUITools.SetActive(GoButton.gameObject, false);
			GoButton.isEnabled = false;
		}


		InputEventHandler.Instance.State = UserInputState.PopupOpen;

		GameManager.Inst.SoundManager.UI.PlayOneShot(GameManager.Inst.SoundManager.GetClip("OpenSplitMenu"), 0.5f);
	}

	public override void Hide ()
	{
		UIEventHandler.Instance.TriggerCloseWindow();
		Camera.main.GetComponent<BlurOptimized>().enabled = false;
		Time.timeScale = 1;

		NGUITools.SetActive(this.gameObject, false);
		this.IsActive = false;


		InputEventHandler.Instance.State = UserInputState.Normal;


	}

	public override bool HasBodySlots (out List<BodySlot> bodySlots)
	{
		bodySlots = null;
		return false;
	}

	public override bool HasTempSlots (out List<TempSlot> tempSlots)
	{
		tempSlots = null;

		return false;
	}





	public void OnCancelButtonPress()
	{
		GameManager.Inst.PlayerControl.SelectedPC.SendCommand(CharacterCommands.Idle);
		GameManager.Inst.PlayerControl.SelectedPC.transform.position = CurrentPortal.SpawnPoint.position;
		Hide();
	}

	public void OnYesButtonPress()
	{
		GameManager.Inst.PlayerControl.Survival.CompleteTraveling(_hours);
		GameManager.Inst.PlayerProgress.LevelSpawnPointName = CurrentPortal.OtherPortalName;
		GameManager.Inst.SaveGameManager.Save("AutoSave", CurrentPortal.NextLevelName);
		GameManager.Inst.LoadLevel(CurrentPortal.NextLevelName);

		Hide();

	}


}
