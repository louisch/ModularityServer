using UnityEngine;
using System.Collections;

/**
* Script responsible for connecting to the photon cloud and setting up the server.
*/
[RequireComponent(typeof(PhotonView))]
public class ConnectionManager : MonoBehaviour {
	/* Game/server/room information */
	public string gameVersion = "v0.1dev";
	public string roomName = "The Room";
	bool connected = false;
	bool roomOn = false;

	/* Manual button setup (provisional) */
	public float buttonSizeMultiplier = 0.1f;
	float btnX, btnY, btnW, btnH;

	/**
	* Self-documenting setup method.
	*/
	void Start ()
	{
		btnX = Screen.width * buttonSizeMultiplier;
		btnY = Screen.height * buttonSizeMultiplier;
		btnH = Screen.width * buttonSizeMultiplier;
		btnW = Screen.width * buttonSizeMultiplier;
	}

	/**
	* Called when the server establishes a connection with the photon cloud.
	* Supposedly. It actually doesn't work. Sike!
	*/
	void OnConnectedToMaster ()
	{
		Debug.Log ("Creating room");
		PhotonNetwork.CreateRoom (roomName);
	}

	/**
	* Button and connection setup. Should be replaced with proper UI elements when applicable.
	*/
	void OnGUI ()
	{
        if(!connected && GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Connect to cloud server"))
		{
			Debug.Log ("Connecting to cloud server");
			connected = PhotonNetwork.ConnectUsingSettings (gameVersion);
			if (connected)
			{
				Debug.Log ("Connection successful");
			}
		}
        if(connected && !roomOn && GUI.Button(new Rect(btnX, btnY + 1 + btnH, btnW, btnH), "Create room"))
		{
			Debug.Log ("Creating Room");
			roomOn = PhotonNetwork.CreateRoom (roomName);
			if (roomOn)
			{
				Debug.Log ("Room maded");
			}
		}
	}
}
