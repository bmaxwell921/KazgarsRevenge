package main.kazgarsrevenge.model;

import java.util.List;

import main.kazgarsrevenge.data.Location;

/**
 * Object representing a Chunk in Kazgar's Revenge
 * @author bmaxwell
 *
 */
public interface IChunk {

	/**
	 *  Adds the given room to this chunk
	 * @param room
	 */
	public void addRoom(IRoom... room);
	
	/**
	 * Returns the rooms in this Chunk
	 * @return
	 */
	public List<IRoom> getRooms();
	
	/**
	 *  Returns the location of this chunk in 2D space
	 * @return
	 */
	public Location getLocation();	
	
	/**
	 * The name of this chunk
	 * @return
	 */
	public String getChunkName();
	
	/**
	 * The Json representation of this chunk
	 * @return
	 */
	public String getJsonRep();
}
