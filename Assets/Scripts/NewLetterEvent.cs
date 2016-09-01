using UnityEngine;
using System.Collections;
using System;

public class NewLetterEvent : EventArgs {

	public Letter PassedLetter{ get; private set;}
	//0 = harvested non-POTD item, 
	//1 = harvested POTD item
	//-1 = used POTD item
	//2+ = replicated POTD item with 2+ charges
	public int ChangeAmount{ get; private set; }
	public int Index { get; private set; }

	public NewLetterEvent () : this(null, 0, 0){
		
	}
		
	public NewLetterEvent(Letter _letter) : this(_letter,0, 0){
		PassedLetter = _letter;
	}

	public NewLetterEvent(Letter _letter, int _changeAmount) : this(_letter, _changeAmount, 0){
		PassedLetter = _letter;
		ChangeAmount = _changeAmount;
	}

	public NewLetterEvent(Letter _letter, int _changeAmount, int _index){
		PassedLetter = _letter;
		ChangeAmount = _changeAmount;
		Index = _index;
	}
}
