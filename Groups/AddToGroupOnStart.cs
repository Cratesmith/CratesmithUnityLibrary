using UnityEngine;
using System.Collections;

public class AddToGroupOnStart : MonoBehaviour 
{
	public GroupId groupId;
	
	void Awake()
	{
		if(groupId)
			Group.AddToGroup(groupId, gameObject);
	}
}
