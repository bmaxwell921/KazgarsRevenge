package main.kazgarsrevenge.model;

import java.awt.Rectangle;
import java.util.List;

import main.kazgarsrevenge.data.Location;

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
	public List<IRoomBlock> getRoomBlocks();
	
	/**
	 * Returns this room's location in 2D space
	 * @return
	 */
	public Location getLocation();
	
	/**
	 * Returns the bounds of this room, as a rectangle  
	 * @return
	 */
	public Rectangle getBoundingRect();
}
