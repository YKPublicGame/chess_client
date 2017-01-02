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
	NetClient network = new NetClient();
	UnityEngine.UI.InputField accountInput;
	UnityEngine.UI.InputField passwdInput;
	Text errTip;
	int errTipCount = 0;

	// Use this for initialization
	void Start () {
		network.Connect("127.0.0.1", 8888);
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

		obj = GameObject.Find ("passwd_input");
		passwdInput = obj.GetComponent<InputField> ();

		obj = GameObject.Find("err_tip");
		errTip = obj.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateTip ();

		NetWork.Msg msg = network.PeekMsg ();
		if (msg == null)
			return;

		if (msg.name == "Login.Login") {
			onLogin (msg);
		}
	}

	void UpdateTip(){
		if (errTip.text == "") {
			return;
		}
		errTipCount += 1;
		if (errTipCount < 120) {
			return;
		}

		errTip.text = "";
		errTipCount = 0;
	}

	void ShowTip(String info){
		errTip.text = info;
		errTipCount = 0;
	}

	void onLogin(Msg msg){
		Login.Login login = (Login.Login)msg.body;
		Debug.Log ("onLogin " + login.account + " and " + login.token);
		Debug.Log (login.account);
		UnityEngine.SceneManagement.SceneManager.LoadScene ("playing");
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

		Login.Login lo = new Login.Login ();
		lo.account = accountInput.text;
		lo.token = passwdInput.text;

		network.WriteMsg ("Login.Login", lo);
	}

	void onQuitClick(){
		Application.Quit ();
	}
}
