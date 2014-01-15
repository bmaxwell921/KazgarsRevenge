package main.kazgarsrevenge.model.blocks.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.IRoomBlock;

/**
 * This class might work to just read in the block types...
 * Pretty much makes all the other block types useless
 * @author Brandon
 *
 */
public class DefaultBlock implements IRoomBlock {

	// Default blocks are 1x1
	private static final int SIZE = 1;
	
	private String type;
	
	// Location of the block
	private Location roomLoc;
	
	// Rotation of the block
	private Rotation rot;
	
	public DefaultBlock() {
		this(null, new Location(), Rotation.ZERO);
	}
	
	public DefaultBlock(String type, Location roomLoc, Rotation rot) {
		this.type = type;
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
	public String getStringRep() {
		return type;
	}

	@Override
	public int getSideLength() {
		return SIZE;
	}
	
	@Override
	public String toString() {
		String template = "[%s:%s]";
		return String.format(template, getStringRep(), roomLoc.toString());
	}
	
	@Override
	public Object clone() {
		try {
			DefaultBlock ret = (DefaultBlock) super.clone();
			ret.roomLoc = this.roomLoc;
			ret.rot = this.rot;
			ret.type = this.type;
			return ret;
		} catch (CloneNotSupportedException cnse) {
			cnse.printStackTrace();
			return null;
		}
	}

}
