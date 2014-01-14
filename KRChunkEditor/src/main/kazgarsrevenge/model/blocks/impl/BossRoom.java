package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * Block representing a boss room
 * @author Brandon
 *
 */
public class BossRoom extends ARoomBlock {

	public static final String STRING_REP = "B";
	
	public BossRoom() {
		super();
	}

	public BossRoom(Location roomLoc, Rotation rot) {
		super(roomLoc, rot);
	}

	@Override
	public String getStringRep() {
		return STRING_REP;
	}

}
