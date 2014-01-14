package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * A block representing empty floor space
 * @author Brandon
 *
 */
public class FloorBlock extends ARoomBlock {

	public static final String STRING_REP = "F";
	
	public FloorBlock() {
		super();
	}

	public FloorBlock(Location roomLoc, Rotation rot) {
		super(roomLoc, rot);
	}

	@Override
	public String getStringRep() {
		return STRING_REP;
	}

}
