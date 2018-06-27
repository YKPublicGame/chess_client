using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess{
	public const int KING = 1;    // 帅
	public const int ADVISOR = 2; // 士
	public const int BISHOP = 3;  // 象
	public const int KNIGHT = 4;  // 马
	public const int ROOK = 5;    // 车
	public const int CANNON = 6;  // 炮
	public const int PAWN = 7;    // 兵
	public bool my_turn = true;  // 走棋
	public bool is_red = true;

	// 初始棋盘
	public int[] board = new Int32[]{
		 15, 14, 13, 12, 11, 12, 13, 14, 15,
		  0,  0,  0,  0,  0,  0,  0,  0,  0,
		  0, 16,  0,  0,  0,  0,  0, 16,  0,
		 17,  0, 17,  0, 17,  0, 17,  0, 17,
		  0,  0,  0,  0,  0,  0,  0,  0,  0,
		  0,  0,  0,  0,  0,  0,  0,  0,  0,
		  7,  0,  7,  0,  7,  0,  7,  0,  7,
		  0,  6,  0,  0,  0,  0,  0,  6,  0,
		  0,  0,  0,  0,  0,  0,  0,  0,  0,
		  5,  4,  3,  2,  1,  2,  3,  4,  5,
	};

	public void init(bool i_am_red, bool is_my_turn){
		is_red = i_am_red;

		my_turn = is_my_turn;
	}

	public bool IsMyTurn(){
		return my_turn;
	}

    public bool isValidRowCol(int row,int col)
    {
        return row >= 1 && row <= 10 && col >= 1 && col <= 9;
    }

	public bool IsMyCM(int row, int col){
		int i = (row - 1) * 9 + col - 1;
		int cm = board [i];

		if (0 == cm) return false;
        return is_red == cm < 10;
	}

	public bool IsRedCM(int row, int col){
		return GetCM (row, col) < 10;
	}

	int GetCM(int row, int col){
		int index = (row - 1) * 9 + col - 1;
		return board [index];
	}

    int get_type(int cm)
    {
        if (cm > 10) return cm - 10;
        return cm;
    }

	void SetCM(int row, int col, int cm){
		int index = (row - 1) * 9 + col - 1;
		board [index] = cm;
	}

	public void Move(int sRow, int sCol, int dRow, int dCol){
		my_turn = !IsMyCM(sRow, sCol);

		int src = GetCM (sRow, sCol);
		SetCM (sRow, sCol, 0);
		SetCM (dRow, dCol, src);
	}

    int get_bt_count(int src_row, int src_col, int dest_row, int dest_col)
    {
        int count = 0;
        int step_row = dest_row - src_row;
        int step_col = dest_col - src_col;
        if(step_row!=0) step_row=step_row > 0 ? 1 : -1;
        if(step_col!=0) step_col=step_col > 0 ? 1 : -1;
        do
        {
            if (src_row + step_row == dest_row && src_col + step_col == dest_col) return count;
            src_row += step_row;
            src_col += step_col;
            if (GetCM(src_row, src_col) != 0) ++count;
            
        } while (true);
     }

    bool is_in_hall(int row,int col)
    {
        if (col < 4 || col > 6) return false;
        if (is_red) return row >= 8 && row <= 10;

        return row >= 1 && row <= 3;
    }

    public bool CanMove(int src_row, int src_col, int dest_row, int dest_col)
    {
        // 非法的格子
        if (!isValidRowCol(src_row, src_col) || !isValidRowCol(dest_row, dest_col)) return false;
        // 不是自己的棋子
        if (!IsMyCM(src_row, src_col)) return false;
        // 目标是自己的棋子
        if (IsMyCM(dest_row, dest_col)) return false;

        int src_cm = GetCM(src_row, src_col);

        int dest_cm = GetCM(dest_row, dest_col);

        int diff_row = dest_row - src_row;
        int diff_col = dest_col - src_col;
        // 将、帅
        if (get_type(src_cm) == KING)
        {
            // 对面笑
            if (get_type(dest_cm) == KING) {
                if (src_col != dest_col) return false;

                // 中间不能有子
                return get_bt_count(src_row, src_col, dest_row, dest_col) == 0;
            }

            // 只能在九宫格内部
            if (!is_in_hall(dest_row, dest_col)) return false;

            // 只能直着走一步
            return Math.Abs(diff_row) + Math.Abs(diff_col) == 1;
        }
        // 士
        else if (get_type(src_cm) == ADVISOR)
        {
            // 只能在九宫格内部
            if (!is_in_hall(dest_row, dest_col)) return false;

            // 只能斜着走一步
            return Math.Abs(diff_row * diff_col) == 1;
        }
        // 象、相
        else if (get_type(src_cm) == BISHOP)
        {
            // 不能过河
            if (Math.Abs((src_row-5.5) * (dest_row-5.5)) < 0) return false;

            // 只能飞田
            if (Math.Abs(diff_row) != 2 || Math.Abs(diff_col) != 2) return false;

            // 象心不能有子
            return GetCM(src_row + diff_row / 2, src_col + diff_col / 2) == 0;
        }
        // 马
        else if (get_type(src_cm) == KNIGHT)
        {
            // 只能走日
            if (Math.Abs(diff_row*diff_col) != 2) return false;

            // 马腿不能有子
            return GetCM(src_row + diff_row / 2, src_col + diff_col / 2) == 0;
        }
        // 车
        else if (get_type(src_cm) == ROOK)
        {
            // 走直线
            if (Math.Abs(diff_row * diff_col) != 0) return false;

            return get_bt_count(src_row, src_col, dest_row, dest_col) == 0;
        }
        // 炮
        else if (get_type(src_cm) == CANNON)
        {
            // 走直线
            if (Math.Abs(diff_row * diff_col) != 0) return false;
 
            // 没目标
            if (GetCM(dest_row, dest_col) == 0)
            {
                return get_bt_count(src_row, src_col, dest_row, dest_col) == 0;
            }

            return get_bt_count(src_row, src_col, dest_row, dest_col) == 1;
        }
        // 兵、卒
        else if(get_type(src_cm) == PAWN)
        {
            // 走直线
            if (diff_row * diff_col != 0) return false;
            // 走一步
            if (Math.Abs(diff_row + diff_col) != 1) return false;
            // 没过河不能左右走
            if (diff_col != 0)
            {
                if (src_cm < 10 && src_row > 5) return false;
                if (src_cm > 10 && src_row < 6) return false;
            }

            // 不能倒退
            if (src_cm < 10 && diff_row > 0) return false;
            if (src_cm > 10 && diff_row < 0) return false;

            return true;
        }
        return false;
    }
}
