using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Reflection; 
using UnityEngine;

namespace NetWork{
	
	public class Msg {
		public string name;
		public object body;
	}

	public class RecvBuff {
		public const int size = 1024;
		public int cur = 0;
		public byte[] buff = new Byte[size];
	}

	public class NetClient{
		private Socket socket;
		bool connected = false;
		private RecvBuff recvBuff = new RecvBuff();
		private static Dictionary<String, Type> mProtoTbl = new Dictionary<String, Type>();
		private List<Msg> msgList = new List<Msg>();

		private static NetClient _netClient;

		private NetClient(){
		}

		public static NetClient Instance(){
			if (_netClient == null) {
				_netClient = new NetClient ();
			}

			return _netClient;
		}

		private UInt16 ReadUInt16(byte[] buff, int offset){
			UInt16 i = (ushort)(buff[offset]*256 + buff[offset + 1]);
			return i;
		}

		private void WriteUInt16(UInt16 i, byte[] buff, int offset){
			byte[] b = BitConverter.GetBytes (i);
			buff[offset] = b[1];
			buff [offset + 1] = b [0];
		}

		public bool Connect( string ip, int port){
			try {
				Debug.Log ("begin connect " + ip);
				socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect(ip, port, new AsyncCallback(ConnectCallBack), null);
				return true ;
			}
			catch {
				return false ;
			}
		}

		public void ConnectCallBack(IAsyncResult ar){
			try {
				socket.EndConnect(ar);  
				connected = true; 
				BeginReceive ();
			}catch(Exception e){
				Debug.Log (e.ToString());
			}
		}

		private void SendCallback(IAsyncResult ar)  
		{  
		}  

		public void Disconnect(){
			socket.Close();
		}

		public void BeginReceive(){
			try{
				socket.BeginReceive(recvBuff.buff, recvBuff.cur, RecvBuff.size - recvBuff.cur, 0, new AsyncCallback(ReceiveCallback), socket);
			}
			catch(Exception e) {
				Debug.Log (e.ToString ());
			}
		}

		private void ReceiveCallback(IAsyncResult ar){
			try{
				int bytesRead = socket.EndReceive(ar);
				if (bytesRead <= 0) {
					BeginReceive ();
					return;
				}
				Debug.Log(String.Format("recv {0}",bytesRead));
					
				recvBuff.cur += bytesRead;
				int cur = 0;

				while (cur + 2 < recvBuff.cur) {
					int len = 0;
					Msg msg = ReadMsg(recvBuff.buff, cur, ref len);
					if (msg == null)
						break;
					msgList.Add (msg);
					cur = cur + len;
				}
				Debug.Log(string.Format("{0},{1}",recvBuff.cur, cur));
				if(cur > 0){
					Buffer.BlockCopy(recvBuff.buff, cur, recvBuff.buff, 0, recvBuff.cur - cur);
					recvBuff.cur = recvBuff.cur - cur;
				}

			}
			catch(Exception e){
				Debug.Log (e.ToString ());
			}
			BeginReceive ();
		}

		private Msg ReadMsg(byte[] buff, int offset, ref int len){
			try{
				len = ReadUInt16(buff, offset) + 2;
				if (offset + len > recvBuff.cur) {
					return null;
				}
				UInt16 nlen = ReadUInt16(buff, offset + 2);
				string name = System.Text.Encoding.ASCII.GetString (buff, offset + 2 + 2, nlen);
				Debug.Log (String.Format("received msg {0} offset={1}, len={2}", name, offset, len-2));

				UInt16 dlen = ReadUInt16(buff, offset + 2 + 2 + nlen);
				Type type = mProtoTbl [name];
				if (type == null) {
					return new Msg(); 
				}
				MemoryStream stream = new MemoryStream ();
				stream.Write (buff, offset + 2 + 2 + nlen + 2, dlen);
				stream.Position = 0;
				var body = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type);
				Msg msg = new Msg();
				msg.name = name;
				msg.body = body;
				return msg;
			}catch(Exception e){
				Debug.Log (e.ToString ());
				return null;
			}
		}

		public Msg PeekMsg(){
			if (msgList.Count == 0) {
				return null;
			}

			Msg msg = msgList [0];
			msgList.RemoveAt (0);
			return msg;
		}

		public void WriteMsg<MsgType>(string name, MsgType msg){
			MemoryStream ms = new MemoryStream ();
			ProtoBuf.Serializer.Serialize(ms, msg);
			byte[] protoByte = ms.ToArray();
			UInt16 protolen = (UInt16)protoByte.Length;

			byte[] nameByte = System.Text.Encoding.ASCII.GetBytes (name);
			UInt16 nlen = (UInt16)nameByte.Length;

			UInt16 len = (UInt16)(2 + nlen + 2 + protolen); 

			byte[] packBuff = new byte[len + 2];



			WriteUInt16 (len, packBuff, 0);
			WriteUInt16 (nlen, packBuff, 2);
			Buffer.BlockCopy(nameByte, 0, packBuff, 2 + 2, nlen);
			WriteUInt16 (protolen, packBuff, 2 + 2 + nlen);
			Buffer.BlockCopy(protoByte, 0, packBuff, 2 + 2 + nlen + 2, protolen);
			Write (0, packBuff);
			Debug.Log(string.Format("send msg {0}", name));
		}

		private void Write( int msgType, byte [] msgContent){
			socket.BeginSend(msgContent, 0, msgContent.Length, 0, new AsyncCallback(SendCallback), msgContent);
		}

		public static void Register()
		{
			//通过GetAssemblies 调用appDomain的所有程序集
			System.Reflection.Assembly assembly = Assembly.GetExecutingAssembly();
			{
				//反射当前程序集的信息
				foreach(Type type in assembly.GetTypes())
				{
					if (!type.IsAbstract && !type.IsInterface && type.GetCustomAttributes (typeof(ProtoBuf.ProtoContractAttribute), false).Length > 0) {
						mProtoTbl [type.FullName] = type;
					}
				}
			}


		}

		public void ToHexString(byte[] bytes)
		{
			try{
				string byteStr = string.Empty;
				if (bytes != null || bytes.Length > 0)
				{
					foreach (var item in bytes)
					{
						byteStr += string.Format("{0:X2} ", item);
					}
				}
				Debug.Log(byteStr);
			}catch(Exception e){
				Debug.Log (e.ToString ());
			}
		}
	}
} 
