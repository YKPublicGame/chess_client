using UnityEngine;  
using System.Collections;  
using System.Collections.Generic;  
using System.IO;  
using System.Text; 
using System;
using System.Reflection;

public class Log : MonoBehaviour {  
	static List<string> mLines = new List<string>();  
	static List<string> mWriteTxt = new List<string>();  
	private string outpath;  
	void Awake()  
	{  
		//Application.persistentDataPath Unity中只有这个路径是既可以读也可以写的。  
		outpath = Application.dataPath + "/outLog.txt";  
		//每次启动客户端删除之前保存的Log  
		if (System.IO.File.Exists(outpath))  
		{  
			File.Delete(outpath);  
		}  
		//在这里做一个Log的监听  
		//转载的原文中是用Application.RegisterLogCallback(HandleLog);但是这个方法在unity5.0版本已经废弃不用了  
		Application.logMessageReceived += HandleLog;  
		//一个输出  
		Debug.Log("log inited...");  
		Type type = Type.GetType("Mono.Runtime");
		if (type != null)
		{
			MethodInfo info = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

			if (info != null)
				Debug.Log(info.Invoke(null, null));
		}
	}  

	void Update()  
	{  
		//因为写入文件的操作必须在主线程中完成，所以在Update中哦给你写入文件。  
		if (mWriteTxt.Count > 0)  
		{  
			string[] temp = mWriteTxt.ToArray();  
			foreach (string t in temp)  
			{  
				using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))  
				{  
					writer.WriteLine(t);  
				}  
				mWriteTxt.Remove(t);  
			}  
		}  
	}  

	void HandleLog(string logString, string stackTrace, LogType type)  
	{  
		mWriteTxt.Add(logString);  
		if (type == LogType.Error || type == LogType.Exception)  
		{  
			PLog(logString);  
			PLog(stackTrace);  
		}  
	}  

	//这里我把错误的信息保存起来，用来输出在手机屏幕上  
	static public void PLog(params object[] objs)  
	{  
		string text = "";  
		for (int i = 0; i < objs.Length; ++i)  
		{  
			if (i == 0)  
			{  
				text += objs[i].ToString();  
			}  
			else  
			{  
				text += ", " + objs[i].ToString();  
			}  
		}  
		if (Application.isPlaying)  
		{  
			if (mLines.Count > 20)  
			{  
				mLines.RemoveAt(0);  
			}  
			mLines.Add(text);  

		}  
	}  

	void OnGUI()  
	{  
		GUI.color = Color.red;  
		for (int i = 0, imax = mLines.Count; i < imax; ++i)  
		{  
		//	GUILayout.Label(mLines[i]);  
		}  
	}  
}  