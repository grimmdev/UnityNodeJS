using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Logger : MonoBehaviour {

	public Text outputLabel;
	private string log;

	public IEnumerator UpdateLog(string v)
	{
		log += v;
		outputLabel.text = log;
		yield return null;
	}
}