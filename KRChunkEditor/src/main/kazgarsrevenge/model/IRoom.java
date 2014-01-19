package main.kazgarsrevenge.model;

import java.awt.Rectangle;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.blocks.impl.DefaultBlock;

/**
 * An object used to represent a room. Rooms are made up of blocks
 * @author Brandon
 *
 */
public interface IRoom {
	
	/**
	 * Gets all of the blocks associated with this room
	 * @return
	 */
	public List<DefaultBlock> getRoomBlocks();
	
	/**
	 * Returns this room's location in 2D space
	 * @return
	 */
	public Location getLocation();
	
	/**
	 * Sets this room's location
	 * @param loc
	 */
	public void setLocation(Location loc);
	
	/**
	 * This room's rotation in 2D space
	 * @return
	 */
	public Rotation getRotation();
	
	/**
	 * Sets this room's rotation
	 * @param rot
	 */
	public void setRotation(Rotation rot);
	
	/**
	 * Returns the bounds of this room, as a rectangle  
	 * @return
	 */
	public Rectangle getBoundingRect();
	
	/**
	 * Returns the name of this room
	 * @return
	 */
	public String getRoomName();
}
