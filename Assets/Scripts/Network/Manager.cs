﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Manager : NetworkManager
{
  enum CustomMsgType
  {
	JoinGroup = MsgType.Highest + 1,
	LeaveGroup,
	JoinSide,
	LeaveSide,
	PlayerStatus,

  Test = 99,
  RegisterView = 100
  }

  private bool serverStarted = false;
  private int worldClientId;
  private string lastPullTime = "NOTSET";
	public UnityEngine.UI.Text PullTimeText; 


  public void JoinGroup (int group)
  {
	if (client != null)
	{
	  client.Send ((short)CustomMsgType.JoinGroup, new IntegerMessage (group));
	}
  }

  public void LeaveGroup (int group)
  {
	if (client != null)
	{
	  client.Send ((short)CustomMsgType.LeaveGroup, new IntegerMessage (group));
	}
  }

  public void JoinSide (int side)
  {
	if (client != null)
	{
	  client.Send ((short)CustomMsgType.JoinSide, new IntegerMessage (side));
	}
  }

  public void LeaveSide (int side)
  {
	if (client != null)
	{
	  client.Send ((short)CustomMsgType.LeaveSide, new IntegerMessage (side));
	}
  }


  private void OnGUI ()
  {

	
	if (!serverStarted) {
		if (GUILayout.Button("Server")) {
			StartServerAndRegister();
		}
	} else {
		GUILayout.Label("Server running"); 
	}


	if (client == null) {
		if (GUILayout.Button("Client")) {
			client = StartClient();
		}
	}
	else {
		GUILayout.Label("Client is running"); 
	}
	

	if (GUILayout.Button ("TEST Ruderern"))
	{
	  client.Send ((short)CustomMsgType.Test, new IntegerMessage (1337));
	}

	if (GUILayout.Button ("TEST WorldView"))
	{
	  client.Send ((short)CustomMsgType.RegisterView, new IntegerMessage (0));
      client.RegisterHandler((short)CustomMsgType.Test, (netMsg) =>
      {
          var msg = netMsg.ReadMessage<IntegerMessage>();
          this.PullTimeText.text = lastPullTime;
          lastPullTime = "Received: " + msg.value.ToString() + System.DateTime.Now.Millisecond.ToString();
      });
    }

	GUILayout.Label (lastPullTime);


  }

  private void StartServerAndRegister ()
  {
	serverStarted = StartServer ();
	NetworkServer.RegisterHandler ((short)CustomMsgType.JoinGroup, OnJoinGroup);
	NetworkServer.RegisterHandler ((short)CustomMsgType.LeaveGroup, OnLeaveGroup);
	NetworkServer.RegisterHandler ((short)CustomMsgType.JoinSide, OnJoinSide);
	NetworkServer.RegisterHandler ((short)CustomMsgType.LeaveSide, OnLeaveSide);

	NetworkServer.RegisterHandler ((short)CustomMsgType.Test, (netMsg) =>
	{
	  var msg = netMsg.ReadMessage<IntegerMessage> ();
		this.PullTimeText.text = lastPullTime; 
		lastPullTime = "SEND START: " + msg.value.ToString() + System.DateTime.Now.Millisecond.ToString();
		NetworkServer.SendToClient(worldClientId, (short)CustomMsgType.Test, msg);
		lastPullTime = "SEND DONE: " + msg.value.ToString() + System.DateTime.Now.Millisecond.ToString();

	});
	NetworkServer.RegisterHandler ((short)CustomMsgType.RegisterView, (netMsg) =>
	{
		worldClientId = netMsg.conn.connectionId; 
	});
  }

  private void OnJoinGroup (NetworkMessage netMsg)
  {
	var gameState = GetComponent<GameState> ();
	var player = gameState.FindPlayer (netMsg.conn.connectionId);

	if (player == null)
	{
	  gameState.AddPlayer (netMsg.conn.connectionId, groupId: netMsg.ReadMessage<IntegerMessage> ().value);
	}
	else
	{
	  player.Side = netMsg.ReadMessage<IntegerMessage> ().value;
	}
  }

  private void OnLeaveGroup (NetworkMessage netMsg)
  {
	var gameState = GetComponent<GameState> ();
	var player = gameState.FindPlayer (netMsg.conn.connectionId);
	player.ResetGroup ();
  }

  private void OnJoinSide (NetworkMessage netMsg)
  {
	var gameState = GetComponent<GameState> ();
	var player = gameState.FindPlayer (netMsg.conn.connectionId);

	if (player == null)
	{
	  gameState.AddPlayer (netMsg.conn.connectionId, sideId: netMsg.ReadMessage<IntegerMessage> ().value);
	}
	else
	{
	  player.Side = netMsg.ReadMessage<IntegerMessage> ().value;
	}
  }

  private void OnLeaveSide (NetworkMessage netMsg)
  {
	var gameState = GetComponent<GameState> ();
	var player = gameState.FindPlayer (netMsg.conn.connectionId);
	player.ResetSide ();
  }


  public override void OnClientConnect (NetworkConnection conn)
  {
	var gameState = GetComponent<GameState> ();
	gameState.AddPlayer (conn.connectionId);
	base.OnClientConnect (conn);
  }

  public override void OnClientDisconnect (NetworkConnection conn)
  {
	var gameState = GetComponent<GameState> ();
	gameState.RemovePlayer (conn.connectionId);
	base.OnClientDisconnect (conn);
  }
}