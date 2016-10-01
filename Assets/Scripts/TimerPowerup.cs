using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerPowerup : MonoBehaviour {

	int secondsLeft;
	Text text;

	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();
	}

	public void SetTimer(){
		text.enabled = true;
		secondsLeft = 10;
		StopCoroutine ("Countdown");
		StartCoroutine ("Countdown");
	}

	public void StopTimer(){
		StopCoroutine ("Countdown");
		text.enabled = false;
	}

	IEnumerator Countdown(){
		do {
			yield return new WaitForSeconds(1);
			secondsLeft--;
			text.text = "<b>00:" + secondsLeft.ToString("D2") + "</b>";
		} while(secondsLeft > 0);

		GameManager.ResetLevel ();
		yield return null;
	}
}
