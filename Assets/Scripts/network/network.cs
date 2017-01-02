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
				socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
				socket.BeginConnect(ip, port, new AsyncCallback(ConnectCallBack), null);
				return true ;
			}
			catch {
				return false ;
			}
		}

		public void ConnectCallBack(IAsyncResult ar){
			socket.EndConnect(ar);  
			connected = true; 
			BeginReceive ();
		}

		private void SendCallback(IAsyncResult ar)  
		{  
		}  

		public void Disconnect(){
			socket.Close();
		}

		public void BeginReceive(){
			try{
				socket.BeginReceive(recvBuff.buff, 0, RecvBuff.size, 0, new AsyncCallback(ReceiveCallback), socket);
			}
			catch(Exception e) {
				Debug.Log (e.ToString ());
			}
		}

		private void ReceiveCallback(IAsyncResult ar){
			try{
				int bytesRead = socket.EndReceive(ar);
				Debug.Log(bytesRead);
				if (bytesRead <= 0) {
					BeginReceive ();
					return;
				}
				Debug.Log (String.Format("received msg len={0}", bytesRead));
				ToHexString(recvBuff.buff);

				recvBuff.cur += bytesRead;
				int cur = 0;

				while (cur + 2 < recvBuff.cur) {
					int len = 0;
					Debug.Log ("slice buff");
					Msg msg = ReadMsg(recvBuff.buff, cur, ref len);
					if (msg == null)
						break;
					msgList.Add (msg);
					cur = cur + 2 + len;
				}
				Debug.Log ("receive msg end");
			}
			catch(Exception e){
				Debug.Log (e.ToString ());
			}
		}

		private Msg ReadMsg(byte[] buff, int offset, ref int len){
			len = ReadUInt16(buff, offset);
			if (offset + len + 2 > recvBuff.cur) {
				return null;
			}
			UInt16 nlen = ReadUInt16(buff, offset + 2);
			string name = System.Text.Encoding.ASCII.GetString (buff, offset + 2 + 2, nlen);
			Debug.Log (String.Format("read msg 1 len={0} name={1}", len, name));

			UInt16 dlen = ReadUInt16(buff, offset + 2 + 2 + nlen);
			Type type = mProtoTbl [name];
			if (type == null) {
				return new Msg(); 
			}
			MemoryStream stream = new MemoryStream ();
			stream.Write (buff, offset + 2 + 2 + nlen + 2, dlen);
			stream.Position = 0;
			ToHexString (stream.ToArray ());
			var body = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type);
			Msg msg = new Msg();
			msg.name = name;
			msg.body = body;
			return msg;
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
			ToHexString (packBuff);

			Debug.Log(string.Format("send msg len={0} name len={1} protolen{2} hole len={3}", len, nlen, protolen, packBuff.Length));
		}

		private void Write( int msgType, byte [] msgContent){
			socket.BeginSend(msgContent, 0, msgContent.Length, 0, new AsyncCallback(SendCallback), msgContent);
		}

		public static void Register()
		{
			//通过GetAssemblies 调用appDomain的所有程序集
			foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				//反射当前程序集的信息
				foreach(Type type in assembly.GetTypes())
				{
					if (!type.IsAbstract && !type.IsInterface && type.GetCustomAttributes (typeof(ProtoBuf.ProtoContractAttribute), false).Length > 0) {
						Debug.Log (type.Name);
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
