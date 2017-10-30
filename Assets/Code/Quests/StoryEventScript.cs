﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System.Data;
using System.Text;
using System.Xml;
using System.IO;
using System;

public class StoryEventScript
{
	public List<string> Script;


	public StoryEventScript()
	{
		Script = new List<string>();
	}

	//returns true if script is over and won't be called again
	//otherwise returns false
	public bool Trigger(object [] parameters)
	{
		foreach(string line in Script)
		{
			string [] tokens = line.Split(new char[]{'/'}, System.StringSplitOptions.None);

			switch(tokens[0])
			{
			case "object":
				ExecuteObjectScript(tokens);
				break;
			case "door":
				ExecuteDoorScript(tokens);
				break;
			case "condition":
				ExecuteConditionScript(tokens);
				break;
			case "if":
				if(!CheckPrerequisite(tokens, parameters))
				{
					//script won't run based on parameters
					return false;
				}
				break;
			case "hook":
				ExecuteHookEventScript(tokens);
				break;
			case "run":
				ExecuteRunScript(tokens);
				break;
			case "message":
				ExecuteMessageScript(tokens);
				break;
			case "item":
				ExecuteItemScript(tokens);
				break;
			case "journal":
				ExecuteJournalScript(tokens);
				break;
			case "topic":
				ExecuteTopicScript(tokens);
				break;
			case "task":
				ExecuteTaskScript(tokens);
				break;
			case "expedition":
				ExecuteExpeditionScript(tokens);
				break;
			case "charstat":
				ExecuteCharStatScript(tokens);
				break;
			case "level":
				ExecuteLevelScript(tokens);
				break;
			}
		}

		return true;


	}



	private bool CheckPrerequisite(string [] tokens, object [] parameters)
	{
		if(tokens[1] == "param")
		{
			
			int paramNumber = Convert.ToInt32(tokens[2]);
			string operation = tokens[3];
			string compValue = tokens[4];

			Debug.Log("Check prerequisite, compValue " + tokens[4] + " param " + parameters[paramNumber]);
				
			if(operation == "is")
			{
				//string comparison
				if(compValue == (string)parameters[paramNumber])
				{
					return true;
				}
			}
			else if(operation == "equals")
			{
				if(Convert.ToInt32(compValue) == Convert.ToInt32(parameters[paramNumber]))
				{
					return true;
				}
			}
		}
		else
		{
			
		}

		return false;
	}

	private void ExecuteObjectScript(string [] tokens)
	{
		GameObject o = GameObject.Find(tokens[1]);
		ToggleObject tO = o.GetComponent<ToggleObject>();
		if(tokens[2] == "on")
		{
			tO.Toggle(true);
		}
		else if(tokens[2] == "off")
		{
			tO.Toggle(false);
		}
		else if(tokens[2] == "toggle")
		{
			tO.Toggle();
		}
	}

	private void ExecuteDoorScript(string [] tokens)
	{
		GameObject o = GameObject.Find(tokens[1]);
		Door door = o.GetComponent<Door>();
		if(tokens[2] == "open")
		{
			if(!door.IsOpen)
			{
				door.Open(door.OpenTarget1, door.IsMachine);
			}
		}
		else if(tokens[2] == "close")
		{
			if(door.IsOpen)
			{
				door.Close(door.IsMachine);
			}
		}
		else if(tokens[2] == "toggle")
		{
			if(door.IsOpen)
			{
				door.Close(door.IsMachine);
			}
			else
			{
				door.Open(door.OpenTarget1, door.IsMachine);
			}
		}
		else if(tokens[2] == "lock")
		{
			door.IsLocked = true;
		}
		else if(tokens[2] == "unlock")
		{
			door.IsLocked = false;
		}
	}

	private void ExecuteConditionScript(string [] tokens)
	{
		if(GameManager.Inst.QuestManager.StoryConditions.ContainsKey(tokens[1]))
		{
			StoryCondition condition = GameManager.Inst.QuestManager.StoryConditions[tokens[1]];
			if(tokens[2] == "true")
			{
				condition.SetValue(1);
			}
			else if(tokens[2] == "false")
			{
				condition.SetValue(0);
			}
			else if(tokens[2] == "toggle")
			{
				if(condition.GetValue() == 1)
				{
					condition.SetValue(0);
				}
				else
				{
					condition.SetValue(1);
				}
			}
			else if(tokens[2] == "activate")
			{
				condition.IsActive = true;
			}
			else if(tokens[2] == "deactivate")
			{
				condition.IsActive = false;
			}
			else
			{
				condition.SetValue(Convert.ToInt32(tokens[2]));
				//Debug.Log("setting condition " + condition.ID + " to " + condition.GetValue());
			}
		}
	}

	private void ExecuteHookEventScript(string [] tokens)
	{
		if(GameManager.Inst.QuestManager.Scripts.ContainsKey(tokens[1]))
		{
			StoryEventType eventType = (StoryEventType)Enum.Parse(typeof(StoryEventType), tokens[2]);
			StoryEventHandler.Instance.AddScriptListener(tokens[1], eventType);
		}
	}

	private void ExecuteRunScript(string [] tokens)
	{
		if(GameManager.Inst.QuestManager.Scripts.ContainsKey(tokens[1]))
		{
			GameManager.Inst.QuestManager.Scripts[tokens[1]].Trigger(new object[]{});
		}
	}

	private void ExecuteMessageScript(string [] tokens)
	{
		
		GameManager.Inst.UIManager.SetConsoleText(tokens[1]);
	}

	private void ExecuteItemScript(string [] tokens)
	{
		if(tokens[1] == "receive")
		{
			string itemID = tokens[2];
			int quantity = Convert.ToInt32(tokens[3]);


			int colPos;
			int rowPos;
			GridItemOrient orientation;
			Item item = GameManager.Inst.ItemManager.LoadItem(itemID);
			float durability = item.MaxDurability;
			if(tokens.Length >= 5)
			{
				durability = item.MaxDurability * Convert.ToSingle(tokens[4]);
			}
			item.Durability = durability;

			HumanCharacter player = GameManager.Inst.PlayerControl.SelectedPC;
			if(player.Inventory.FitItemInBackpack(item, out colPos, out rowPos, out orientation))
			{
				Debug.Log("Found backpack fit " + colPos + ", " + rowPos + " orientation " + orientation);

				GridItemData itemData = new GridItemData(item, colPos, rowPos, orientation, quantity);
				player.Inventory.Backpack.Add(itemData);

				GameManager.Inst.PlayerControl.Party.RefreshAllMemberWeight();

			}
			else
			{
				var resource = Resources.Load(item.PrefabName + "Pickup");
				if(resource != null)
				{
					GameObject pickup = GameObject.Instantiate(resource) as GameObject;
					pickup.transform.position = player.transform.position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 1f, UnityEngine.Random.Range(-0.2f, 0.2f));
					Transform parent = GameManager.Inst.ItemManager.FindPickupItemParent(pickup.transform);
					if(parent != null)
					{
						pickup.transform.parent = parent;
					}
					pickup.GetComponent<PickupItem>().Item = item;
					pickup.GetComponent<PickupItem>().Quantity = quantity;
				}

			}

			GameManager.Inst.UIManager.SetConsoleText("Received item: " + item.Name + " x " + quantity);

		}
		else if(tokens[1] == "lose")
		{
			string itemID = tokens[2];
			int quantity = Convert.ToInt32(tokens[3]);
			HumanCharacter player = GameManager.Inst.PlayerControl.SelectedPC;

			if(tokens.Length >= 5)
			{
				float desiredDurabilityPercent = Convert.ToSingle(tokens[4]) / 100f;
				//look for an item with durablity greater than desired
				List<GridItemData> items = player.Inventory.FindItemsInBackpack(itemID);
				foreach(GridItemData item in items)
				{
					float durability = item.Item.Durability / item.Item.MaxDurability;
					if(durability >= desiredDurabilityPercent)
					{
						player.Inventory.RemoveItemFromBackpack(item);
					}
				}
			}
			else
			{
				player.Inventory.RemoveItemsFromBackpack(itemID, quantity);
				GameManager.Inst.UIManager.SetConsoleText("Lost item: " + GameManager.Inst.ItemManager.GetItemNameFromID(itemID) + " x " + quantity);
			}
		}
	}

	private void ExecuteJournalScript(string [] tokens)
	{
		string entry = tokens[1];

		//check if journal entry is text or tag
		if(entry[0] == '{')
		{
			string journalID = entry.Split('{','}')[1];
			string text = GameManager.Inst.DBManager.DBHandlerStoryEvent.LoadJournalEntry(Convert.ToInt32(journalID));
			GameManager.Inst.PlayerProgress.AddJournalEntry(text);
		}
		else
		{
			GameManager.Inst.PlayerProgress.AddJournalEntry(entry);
		}
	}

	private void ExecuteTopicScript(string [] tokens)
	{
		if(tokens[1] == "discover")
		{
			GameManager.Inst.PlayerProgress.AddDiscoveredTopic(tokens[2]);
		}
		else if(tokens[1] == "forget")
		{
			GameManager.Inst.PlayerProgress.RemoveDiscoveredTopics(tokens[2]);
		}
	}

	private void ExecuteTaskScript(string [] tokens)
	{
		int id = Convert.ToInt32(tokens[2]);
		if(tokens[1] == "complete")
		{
			GameManager.Inst.PlayerProgress.ResolveTask(id);
		}
		if(tokens[1] == "add")
		{
			GameManager.Inst.PlayerProgress.AddNewTask(id);
		}
	}

	private void ExecuteExpeditionScript(string [] tokens)
	{
		if(tokens[1].Length > 0)
		{
			string navNodeName = tokens[1];
			NavNode node = GameManager.Inst.NPCManager.GetNavNodeByName(navNodeName);
			if(node == null)
			{
				//return;
			}
			if(tokens[2] == "enable")
			{
				node.IsOpenToExpedition = true;
			}
			else if(tokens[2] == "disable")
			{
				node.IsOpenToExpedition = false;
			}
		}
	}

	private void ExecuteCharStatScript(string [] tokens)
	{
		Character target = null;
		if(tokens[1] == "player")
		{
			target = GameManager.Inst.PlayerControl.SelectedPC;
		}

		if(target != null)
		{
			if(tokens[2] == "health")
			{
				if(tokens[3] == "restore")
				{
					target.MyStatus.Health = target.MyStatus.MaxHealth;
				}
			}
			else if(tokens[2] == "radiation")
			{
				if(tokens[3] == "restore")
				{
					target.MyStatus.Radiation = 0;
				}
			}
		}
	}

	private void ExecuteLevelScript(string [] tokens)
	{
		if(tokens[2] == "enable")
		{
			GameObject portalGO = GameObject.Find(tokens[1]);
			if(portalGO != null)
			{
				LevelPortal portal = portalGO.GetComponent<LevelPortal>();
				if(portal != null)
				{
					portal.IsAccessible = true;
				}
			}
		}
	}


}
