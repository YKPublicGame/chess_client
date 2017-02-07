using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetWork;
using UnityEngine.UI;
using Room;
using Table;

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
	SortedDictionary<int, GameObject> cms = new SortedDictionary<int, GameObject>();
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
			cms.Add(i, cm);
		}

		selected_me_obj = GameObject.Instantiate (selected_me_prefab, new Vector3(0,0,0), Quaternion.identity);
		selected_me_obj.transform.SetParent (GameObject.Find ("board").transform);
		selected_other_obj = GameObject.Instantiate (selected_other_prefab, new Vector3(0,0,0), Quaternion.identity);
		selected_other_obj.transform.SetParent (GameObject.Find ("board").transform);
	}

	void register_btn(){
		GameObject obj = GameObject.Find ("match_btn");
		Button matchBtn = obj.GetComponent<Button> ();
		matchBtn.onClick.AddListener (delegate() {
			this.onMatchClick ();
		});

		obj = GameObject.Find ("ready_btn");
		Button readyBtn = obj.GetComponent<Button> ();
		readyBtn.onClick.AddListener (delegate() {
			this.onReadyClick ();
		});
	}

	// Use this for initialization
	void Start () {
		Debug.Log ("playing is starting...");
		init ();
		register_btn ();
		network = NetClient.Instance ();
	}

	void onMatchClick(){
		Table.MatchReq req = new Table.MatchReq ();
		network.WriteMsg ("Table.MatchReq", req);
		Debug.Log ("begin match...");
	}

	void onReadyClick(){
		Debug.Log ("ready...");
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

	void OnMouseDown(){
		if (!game.IsMyTurn ())
			return;

		RowCol rowCol = GetRowCol(Input.mousePosition);

		// 选中的是自己的子
		if (game.IsMyCM (rowCol.row, rowCol.col)){
			selected_me.row = rowCol.row;
			selected_me.col = rowCol.col;
			selected_me_obj.transform.SetPositionAndRotation (GetPos (rowCol.row, rowCol.col), Quaternion.identity);
			click_ogg.Play ();
			Debug.Log (String.Format("select {0} {1}", rowCol.row, rowCol.col));
			return;
		}

		if (selected_me.row == 0) {
			return;
		}

		// 选了子，检查能不能走
		if (!game.CanMove (selected_me.row, selected_me.col, rowCol.row, rowCol.col)) {
			return;
		}

		Table.MoveReq req = new Table.MoveReq ();
		req.move = new Table.Move ();
		req.move.srow = selected_me.row;
		req.move.scol = selected_me.col;
		req.move.drow = rowCol.row;
		req.move.dcol = rowCol.col;

		if (game.is_red == false) {
			Debug.Log ("move notify not red");
			req.move.srow = 11 - req.move.srow;
			req.move.drow = 11 - req.move.drow;
		}

		network.WriteMsg ("Table.MoveReq", req);
	}

	// Update is called once per frame
	void Update () {
		NetWork.Msg msg = network.PeekMsg ();
		if (msg == null)
			return;

		if (msg.name == "Table.MatchRsp") {
			onMatch (msg);
		} else if (msg.name == "Table.MatchResult") {
			onMatchResult (msg);
		} else if (msg.name == "Table.MoveNotify") {
			onMoveNotify(msg);
		}
	}

	void onMatch(NetWork.Msg msg){
		Debug.Log ("match rsp");
	}

	void onMatchResult(NetWork.Msg msg){
		Debug.Log ("match result");
		Table.MatchResult result = (Table.MatchResult)msg.body;
		Debug.Log (result.i_am_red);

		game.init (result.i_am_red, result.i_am_red);
		loadAllChess ();
	}

	void onMoveNotify(NetWork.Msg msg){
		Table.MoveNotify notify = (Table.MoveNotify)msg.body;
		Table.Move mv = notify.move;

		if (game.is_red == false) {
			Debug.Log ("move notify not red");
			mv.srow = 11 - mv.srow;
			mv.drow = 11 - mv.drow;
		}

		int index = (mv.srow - 1) * 9 + mv.scol - 1;
		// 遍历棋子，找到
		GameObject cm = cms[index];

		Single r = 0.89f;
		Vector3 old_pos = cm.transform.position;
		Vector3 pos = new Vector3 (old_pos.x + (mv.dcol-mv.scol)*r, old_pos.y + (mv.drow-mv.srow) * -r, 0);
		cm.transform.position = pos;
		game.Move (mv.srow, mv.scol, mv.drow, mv.dcol);

		selected_me.row = 0;
	}
}
