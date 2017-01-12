using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetWork;

public class Playing : MonoBehaviour {
	public Camera camera;
	public GameObject board;

	public GameObject rk;
	public GameObject ra;
	public GameObject rb;
	public GameObject rn;
	public GameObject rr;
	public GameObject rc;
	public GameObject rp;

	public GameObject bk;
	public GameObject ba;
	public GameObject bb;
	public GameObject bn;
	public GameObject br;
	public GameObject bc;
	public GameObject bp;

	public AudioSource click_ogg;

	struct RowCol {
		public RowCol(int r, int c){
			row = r;
			col = c;
		}
		public int row;
		public int col;
	};
	public GameObject selected_me_prefab;
	public GameObject selected_other_prefab;

	public GameObject selected_me_obj;
	public GameObject selected_other_obj;
	RowCol selected_me = new RowCol(0, 0);
	RowCol selected_other = new RowCol(0, 0); 

	NetClient network;

	SortedDictionary<int, GameObject> cm_map = new SortedDictionary<int, GameObject>();
	LinkedList<GameObject> cms = new LinkedList<GameObject>();
	Chess game = new Chess();

	void init(){
		cm_map.Add (Chess.KING, rk);
		cm_map.Add (Chess.ADVISOR, ra);
		cm_map.Add (Chess.BISHOP, rb);
		cm_map.Add (Chess.KNIGHT, rn);
		cm_map.Add (Chess.ROOK, rr);
		cm_map.Add (Chess.CANNON, rc);
		cm_map.Add (Chess.PAWN, rp);

		cm_map.Add (10 + Chess.KING, bk);
		cm_map.Add (10 + Chess.ADVISOR, ba);
		cm_map.Add (10 + Chess.BISHOP, bb);
		cm_map.Add (10 + Chess.KNIGHT, bn);
		cm_map.Add (10 + Chess.ROOK, br);
		cm_map.Add (10 + Chess.CANNON, bc);
		cm_map.Add (10 + Chess.PAWN, bp);
	}

	void loadAllChess(){
		Single r = 0.89f;
		Single x = 0 - 4 *r;
		Single y = 4.5f * r;
		for(int i=0; i<90; ++i) {
			int c = game.board [i];
			if (0 == c) {
				continue;
			}

			Vector3 pos = new Vector3 (x + (i % 9) * r, y - i / 9 * r, 0);
			GameObject cm = GameObject.Instantiate (cm_map[c], pos, Quaternion.identity);
			cm.transform.SetParent (GameObject.Find ("board").transform);
			cms.AddLast (cm);
		}

		selected_me_obj = GameObject.Instantiate (selected_me_prefab, new Vector3(0,0,0), Quaternion.identity);
		selected_me_obj.transform.SetParent (GameObject.Find ("board").transform);
		selected_other_obj = GameObject.Instantiate (selected_other_prefab, new Vector3(0,0,0), Quaternion.identity);
		selected_other_obj.transform.SetParent (GameObject.Find ("board").transform);
	}

	RowCol GetRowCol(Vector3 pos){
		Single r = 0.89f;
		Single x = 0 - 4 *r;
		Single y = 4.5f * r;
		Vector3 worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
		int row = (int)((y - worldPos.y + r/2) / r + 1);
		int col = (int)((worldPos.x - x + r/2) / r + 1);
		return new RowCol (row, col);
	}

	Vector3 GetPos(int row, int col){
		Single r = 0.89f;
		Single x = 0 - 4 *r;
		Single y = 4.5f * r;
		Vector3 pos = new Vector3 (x + (col-1) * r, y - (row -1) * r, 0);
		Debug.Log (pos);
		return pos;
	}

	// Use this for initialization
	void Start () {
		init ();
		loadAllChess();
		Login.Ready msg = new Login.Ready ();
		network = NetClient.Instance ();
		network.WriteMsg("Login.Ready", msg);

	}

	void OnMouseDown(){
		if (!game.IsMyTurn ())
			return;

		RowCol rowCol = GetRowCol(Input.mousePosition);
		Debug.Log (String.Format("{0} {1}", rowCol.row, rowCol.col));

		// 选中的是自己的子
		if (game.IsMyCM (rowCol.row, rowCol.col)){
			selected_me.row = rowCol.row;
			selected_me.col = rowCol.col;
			selected_me_obj.transform.SetPositionAndRotation (GetPos (rowCol.row, rowCol.col), Quaternion.identity);
			click_ogg.Play ();
			return;
		}

		if (selected_me.row == 0) {
			return;
		}

		// 选了子，检查能不能走
		if (!game.CanMove (selected_me.row, selected_me.col, rowCol.row, rowCol.col)) {
			game.Move (selected_me.row, selected_me.col, rowCol.row, rowCol.col);
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
