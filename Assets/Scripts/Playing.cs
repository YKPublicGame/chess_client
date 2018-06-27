using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NetWork;
using Room;
using Table;

class Item {
	public GameObject o;
	public int row;
	public int col;
	public bool visiable;
	public int t;
};

struct RowCol
{
    public RowCol(int r, int c)
    {
        row = r;
        col = c;
    }
    public int row;
    public int col;
};

public class Playing : MonoBehaviour {
	public Camera camera;
	public GameObject board;

	//public AudioSource click_ogg;

	public GameObject select_1;
	public GameObject select_2;
    RowCol selected_me = new RowCol(0, 0);

	NetClient network;

	List<Item> chesses = new List<Item>();
	Chess game = new Chess();

	void addChess(string name,int type,int row, int col){
		GameObject o = GameObject.Find (name);
		Item e = new Item();
		e.o = o;
		e.row = row;
		e.col = col;
		e.visiable = true;
		e.t = type;
		chesses.Add(e);
        // 设置座标
        o.SetActive(true);
        o.transform.localPosition = GetPos(row, col);
	}
	
	void resetChesses(){
        chesses.Clear();
		// 增加黑方棋子
		addChess("black_king",Chess.KING,1,5);
		addChess("black_advisor_1",Chess.ADVISOR,1,4);
		addChess("black_advisor_2",Chess.ADVISOR,1,6);
		addChess("black_bishop_1",Chess.BISHOP,1,3);
		addChess("black_bishop_2",Chess.BISHOP,1,7);
		addChess("black_knight_1",Chess.KNIGHT,1,2);
		addChess("black_knight_2",Chess.KNIGHT,1,8);
		addChess("black_rook_1",Chess.ROOK,1,1);
		addChess("black_rook_2",Chess.ROOK,1,9);
		addChess("black_cannon_1",Chess.CANNON,3,2);
		addChess("black_cannon_2",Chess.CANNON,3,8);
		addChess("black_pawn_1",Chess.PAWN,4,1);
		addChess("black_pawn_2",Chess.PAWN,4,3);
		addChess("black_pawn_3",Chess.PAWN,4,5);
		addChess("black_pawn_4",Chess.PAWN,4,7);
		addChess("black_pawn_5",Chess.PAWN,4,9);
		// 增加红方棋子
		addChess("red_king",Chess.KING+10,10,5);
		addChess("red_advisor_1",Chess.ADVISOR+10,10,4);
		addChess("red_advisor_2",Chess.ADVISOR+10,10,6);
		addChess("red_bishop_1",Chess.BISHOP+10,10,3);
		addChess("red_bishop_2",Chess.BISHOP+10,10,7);
		addChess("red_knight_1",Chess.KNIGHT+10,10,2);
		addChess("red_knight_2",Chess.KNIGHT+10,10,8);
		addChess("red_rook_1",Chess.ROOK+10,10,1);
		addChess("red_rook_2",Chess.ROOK+10,10,9);
		addChess("red_cannon_1",Chess.CANNON+10,8,2);
		addChess("red_cannon_2",Chess.CANNON+10,8,8);
		addChess("red_pawn_1",Chess.PAWN+10,7,1);
		addChess("red_pawn_2",Chess.PAWN+10,7,3);
		addChess("red_pawn_3",Chess.PAWN+10,7,5);
		addChess("red_pawn_4",Chess.PAWN+10,7,7);
		addChess("red_pawn_5",Chess.PAWN+10,7,9);
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

        EventTrigger trigger = board.GetComponent<EventTrigger>();
        trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback = new EventTrigger.TriggerEvent();

        entry.callback.AddListener(onClick);
        trigger.triggers.Add(entry);
    }

    
	// Use this for initialization
	void Start () {
		Debug.Log ("playing is starting...");
		register_btn ();
		network = NetClient.Instance ();

        select_1.SetActive(false);
        select_2.SetActive(false);
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
		double row = Math.Round((360.0 + 297.0 - pos.y)/66.0) + 1;
        double col = Math.Round((pos.x - 640.0 + 264) /66.0) + 1;
        if (game.is_red == false)
        {
            row = 11 - row;
            col = 10 - col;
        }
        return new RowCol ((int)row, (int)col);
	}

	Vector3 GetPos(int row, int col){
        if (game.is_red == false)
        {
            row = 11 - row;
            col = 10 - col;
        }
        Single x = (col - 1) * 66 - 264;
		Single y = 297 - (row-1)*66;

		Vector3 pos = new Vector3 (x, y, 0);
		Debug.Log (pos);
		return pos;
	}

    private void onClick(BaseEventData arg0)
    {
        if (!game.IsMyTurn()) return;
        PointerEventData e = arg0 as PointerEventData;
        Debug.Log(String.Format("click {0}", e.position));

		RowCol rowCol = GetRowCol(e.position);
        if (rowCol.row <= 0 || rowCol.row > 10 || rowCol.col <= 0 || rowCol.col > 9) return;

		// 选中的是自己的子
		if (game.IsMyCM (rowCol.row, rowCol.col)){
            select_1.transform.localPosition = GetPos(rowCol.row, rowCol.col);
            select_1.SetActive(true);
            select_2.SetActive(false);
            selected_me = rowCol;
            //click_ogg.Play ();
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
        selected_me.row = 0;

        Debug.Log (String.Format("move src{0} {1} dst{2} {3}", req.move.srow, req.move.scol, req.move.drow, req.move.dcol));
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
		
		resetChesses();
	}

	void onMoveNotify(NetWork.Msg msg){
		Table.MoveNotify notify = (Table.MoveNotify)msg.body;
		Table.Move mv = notify.move;

        // 干掉目标棋子
        for (int i = 0; i < chesses.Count; ++i)
        {
            if (chesses[i].row == mv.drow && chesses[i].col == mv.dcol && chesses[i].visiable)
            {
                chesses[i].visiable = false;
                chesses[i].o.SetActive(false);
                break;
            }
        }
        // 移动待源棋子
        for (int i = 0; i < chesses.Count; ++i)
        {
            if (chesses[i].row == mv.srow && chesses[i].col == mv.scol && chesses[i].visiable)
            {
                chesses[i].row = mv.drow;
                chesses[i].col = mv.dcol;
                chesses[i].o.transform.localPosition = GetPos(mv.drow, mv.dcol);
                break;
            }
        }
        // select_1 select_2设置
        select_1.transform.localPosition = GetPos(mv.srow, mv.scol);
        select_2.transform.localPosition = GetPos(mv.drow, mv.dcol);
        select_1.SetActive(true);
        select_2.SetActive(true);
        selected_me.row = 0;

        game.Move (mv.srow, mv.scol, mv.drow, mv.dcol);
	}
}
