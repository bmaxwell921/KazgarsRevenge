package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.IRoomBlock;

/**
 * Abstract implementation of the IRoomBlock interface.
 * Implementing classes only need to implement the toString method.
 * 
 * Default blocks are only 1x1
 * @author Brandon
 *
 */
public abstract class ARoomBlock implements IRoomBlock {

	private static final int DEFAULT_SIZE = 1;
	
	// Location of the block
	private Location roomLoc;
	
	// Rotation of the block
	private Rotation rot;
	
	public ARoomBlock() {
		this(new Location(), Rotation.ZERO);
	}
	
	public ARoomBlock(Location roomLoc, Rotation rot) {
		this.roomLoc = roomLoc;
		this.rot = rot;
	}

	@Override
	public Location getRelativeLocation() {
		return roomLoc;
	}

	@Override
	public void setRelativeLocation(Location loc) {
		this.roomLoc = loc;
	}

	@Override
	public Rotation getRotation() {
		return rot;
	}

	@Override
	public void setRotation(Rotation rot) {
		this.rot = rot;
	}
	
	@Override
	public int getSideLength() {
		return DEFAULT_SIZE;
	}
	
	@Override
	public String toString() {
		String template = "[%s:%s]";
		return String.format(template, getStringRep(), roomLoc.toString());
	}
}
