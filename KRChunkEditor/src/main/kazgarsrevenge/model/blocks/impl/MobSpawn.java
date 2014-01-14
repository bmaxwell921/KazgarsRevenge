package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * A block representing where mobs would spawn
 * @author Brandon
 *
 */
public class MobSpawn extends ARoomBlock {

	public static final String STRING_REP = "M";
	
	public MobSpawn() {
		super();
	}

	public MobSpawn(Location roomLoc, Rotation rot) {
		super(roomLoc, rot);
	}

	@Override
	public String getStringRep() {
		return STRING_REP;
	}

}
