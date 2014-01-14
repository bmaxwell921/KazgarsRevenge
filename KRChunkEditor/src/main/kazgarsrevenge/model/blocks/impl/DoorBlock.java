package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * Block representing a door
 * @author Brandon
 *
 */
public class DoorBlock extends ARoomBlock {

	private static final String STRING_REP = "D";
	
	public DoorBlock() {
		super();
	}
	
	public DoorBlock(Location loc, Rotation rot) {
		super(loc, rot);
	}

	@Override
	public String getStringRep() {
		return STRING_REP;
	}
	
	
}
