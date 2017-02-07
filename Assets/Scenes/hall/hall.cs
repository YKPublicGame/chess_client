using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetWork;
using Room;
using Login;

public class hall : MonoBehaviour {
	private NetClient network;

	// Use this for initialization
	void Start () {
		try{
			Debug.Log("hall is starting...");
			network = NetClient.Instance ();

			Room.RoomListReq req = new Room.RoomListReq ();
			network.WriteMsg("Room.RoomListReq", req);
		}catch(Exception e){
			Debug.Log (e.ToString());
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		NetWork.Msg msg = network.PeekMsg ();
		if (msg == null)
			return;

		if (msg.name == "Room.RoomListRsp") {
			onRoomList (msg);
		} else if (msg.name == "Room.EnterRsp") {
			onEnter (msg);
		}
	}

	void onRoomList(NetWork.Msg msg){
		Room.RoomListRsp rsp = (Room.RoomListRsp)msg.body;

		Room.EnterReq req = new Room.EnterReq ();
		req.room_id = 1;
		network.WriteMsg ("Room.EnterReq",req);
		Debug.Log (rsp);
	}

	void onEnter(NetWork.Msg msg){
		UnityEngine.SceneManagement.SceneManager.LoadScene ("playing");
	}
}
