using UnityEngine;
using System.Collections;

public class TeleportController : ModuleController {
	Vector2 mousePosition;
	int currentTeleportFramecount = 0;
	public int totalTeleportFrameCount = 5;

	void FixedUpdate () {
		if (!info.sprite.enabled)
		{
			currentTeleportFramecount++;
			if (currentTeleportFramecount >= totalTeleportFrameCount)
			{
				info.sprite.enabled = true;
			}
		}
	}	


	[RPC]
	void MouseUpdate (Vector2 mouse, PhotonMessageInfo info)
	{
		if (info.sender == this.info.owner)
		{
			mousePosition = mouse;
		}
	}


	[RPC]
	void InitiateTeleport (PhotonMessageInfo info)
	{
		if (info.sender == this.info.owner)
		{
			currentTeleportFramecount = 0;
			this.info.sprite.enabled = false;
			transform.position = mousePosition;

			this.info.view.RPC ("Teleport", PhotonTargets.Others, mousePosition);
		}
	}
}
