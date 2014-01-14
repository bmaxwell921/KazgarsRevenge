package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * A block representing where the players would spawn
 * @author Brandon
 *
 */
public class PlayerSpawn extends ARoomBlock {

	public static final String STRING_REP = "P";
	
	public PlayerSpawn() {
		super();
	}

	public PlayerSpawn(Location roomLoc, Rotation rot) {
		super(roomLoc, rot);
	}

	@Override
	public String getStringRep() {
		return STRING_REP;
	}

}
