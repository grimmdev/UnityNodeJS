using UnityEngine;
using System.IO;
using System.Collections;
using System.Diagnostics;

public class NodeExec : MonoBehaviour {

	public TextAsset jsFile;
	public Logger log;

	private string NodePath;
	private Process NodeProcess;
	private StreamWriter NodeWriter;
	private StreamReader NodeReader;

	// Use this for initialization
	void Awake () {
		NodePath = GetNodePath (); // get the current path for node, relative to the unity game.
		print (NodePath); // output the path to make sure it's correct.
		StartCoroutine (StartNode ()); // begin the process.
	}

	// Unity stops for some reason
	private void OnDestroy()
	{
		if (NodeProcess != null) // Destroy the node process if it exists.
			NodeProcess.Kill ();
	}

	private string GetNodePath ()
	{
		return Application.streamingAssetsPath + "/Node/x86/node.exe";
	}

	private IEnumerator StartNode()
	{
		NodeProcess = new Process ();
		NodeProcess.StartInfo.FileName = NodePath;
		NodeProcess.StartInfo.CreateNoWindow = true;
		NodeProcess.StartInfo.UseShellExecute = false;
		NodeProcess.StartInfo.RedirectStandardInput = true;
		NodeProcess.StartInfo.RedirectStandardOutput = true;
		NodeProcess.StartInfo.RedirectStandardError = true;
		NodeProcess.StartInfo.Arguments = "-i";
		NodeProcess.OutputDataReceived += DataReceived;
		NodeProcess.Start ();
		NodeProcess.BeginOutputReadLine ();

		yield return StartCoroutine (OpenNode ()); // begin to actually use the node process
	}

	private IEnumerator OpenNode()
	{
		if (NodeProcess != null) {
			NodeWriter = NodeProcess.StandardInput;

			// didn't really need to use console log for this example, however I felt it was the simplest way to strip any special characters.
			Write ("function log(s) { console.log(s); }");
			Write ("let v = 'NodeJS Version ' + process.version;log(v);");
			Write ("v = '2+2=' + (2+2);log(v);");
			Write ("v = '50x50=' + (50*50);log(v);");
			Write (jsFile.text);
		}
		yield return null;
	}

	private void Write(string d)
	{
		if (NodeWriter != null) {
			print ("Writing to nodejs"); // make sure to let us know that the process is starting, to which we are communicating with nodejs.
			NodeWriter.WriteLine (d);
		}
	}

	private void DataReceived(object sender, DataReceivedEventArgs e)
	{
		if(!string.IsNullOrEmpty(e.Data)) {
			string results = e.Data;
			results = results.Remove (0, 2); // remove the whitespace at the front of each result (just spaces).
			results = results.Replace ("\r",""); // remove the new line from our result as well.
			if (!string.IsNullOrEmpty (results) && !results.Contains("undefined") && !results.Contains("defined")) {
				UnityMainThreadDispatcher.Enqueue (log.UpdateLog("[ " + results + " ]\n")); // format the process the info on the unity thread.
			}
		}
	}
}