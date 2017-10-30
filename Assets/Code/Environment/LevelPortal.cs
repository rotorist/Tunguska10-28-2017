using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPortal : MonoBehaviour 
{
	public bool IsAccessible;
	public string NextLevelName;
	public int TravelDistance; 
	public Transform SpawnPoint;
	public string OtherPortalName;


	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.name == "RightFootCollider")
		{
			Debug.Log("going to load new level");
			GameManager.Inst.UIManager.TravelPanel.CurrentPortal = this;
			UIEventHandler.Instance.TriggerTravelPanel();
		}
	}

}
