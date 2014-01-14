package main.kazgarsrevenge.model;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * Object used to represent one block of a Kazgar's Revenge room
 * @author Brandon
 *
 */
public interface IRoomBlock {

	/**
	 * Returns the location of this block in it's current room,
	 * relative to the upper left corner of the room
	 * @return
	 */
	public Location getRelativeLocation();
	
	/**
	 * Sets this block's location relative to the upper left
	 * corner of the room it resides in
	 * @param loc
	 */
	public void setRelativeLocation(Location loc);
	
	/**
	 * Gets the rotation of this block. Used when blocks have exploitable symmetry
	 * @return
	 */
	public Rotation getRotation();
	
	/**
	 * Sets the rotation of this block.
	 * @param rot
	 */
	public void setRotation(Rotation rot);
	
	/**
	 * Returns the string representation of this block
	 * @return
	 */
	public String getStringRep();
	
	/**
	 * Returns the length of this block's size
	 * @return
	 */
	public int getSideLength();
}
