using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using ProtoBuf.Meta;
using UnityEngine.UI;
using NetWork;

public class Loading : MonoBehaviour {
	int tick = 0;
	NetClient network;
	UnityEngine.UI.InputField accountInput;
	UnityEngine.UI.InputField passwdInput;
	Text errTip;
	int errTipCount = 0;

	// Use this for initialization
	void Start () {
		try{
			Debug.Log("Start Loading...");
			network = NetClient.Instance();
			network.Connect("120.79.91.100", 8888);
			NetClient.Register ();

			GameObject obj = GameObject.Find ("login_btn");
			Button loginBtn = obj.GetComponent<Button> ();
			loginBtn.onClick.AddListener (delegate() {
				this.onLoginClick ();
			});

			obj = GameObject.Find ("quit_btn");
			Button quitBtn = obj.GetComponent<Button> ();
			quitBtn.onClick.AddListener (delegate() {
				this.onQuitClick ();
			});

			obj = GameObject.Find ("acc_input");
			accountInput = obj.GetComponent<InputField> ();

			obj = GameObject.Find ("password_input");
			passwdInput = obj.GetComponent<InputField> ();

			obj = GameObject.Find ("err_tip");
			errTip = obj.GetComponent<Text> ();
		}catch(Exception e){
			Debug.Log (e.ToString ());
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateTip ();

		NetWork.Msg msg = network.PeekMsg ();
		if (msg == null)
			return;

		if (msg.name == "Login.LoginRsp") {
			onLogin (msg);
		}
	}

	void UpdateTip(){
		try{
	//		if (errTip.text == "") {
	//			return;
	//		}
	//		errTipCount += 1;
	//		if (errTipCount < 120) {
	//			return;
	//		}
		//
	//		errTip.text = "";
	//		errTipCount = 0;
		}
		catch(Exception e){
			Debug.Log (e.ToString ());
		}
	}

	void ShowTip(String info){
		errTip.text = info;
		errTipCount = 0;
		Debug.Log (info);
	}

	void onLogin(Msg msg){
		Login.LoginRsp login = (Login.LoginRsp)msg.body;
		Debug.Log ("onLogin " + login.account + " and " + login.token);
		try{
			UnityEngine.SceneManagement.SceneManager.LoadScene ("hall");
		}catch(Exception e){
			Debug.Log (e.ToString ());
		}
	}

	public void onLoginClick(){
		if (accountInput.text == "") {
			ShowTip ("please input account");
			return;
		}

		if (passwdInput.text == "") {
			ShowTip ("please input passwd");
			return;
		}

		Login.LoginReq lo = new Login.LoginReq ();
		lo.account = accountInput.text;
		lo.token = passwdInput.text;

		network.WriteMsg ("Login.LoginReq", lo);
	}

	void onQuitClick(){
		Application.Quit ();
	}
}
