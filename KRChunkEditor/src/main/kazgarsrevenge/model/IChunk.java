package main.kazgarsrevenge.model;

import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;

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
	public void addRoom(DefaultRoom... room);
	
	/**
	 * Returns the rooms in this Chunk
	 * @return
	 */
	public List<DefaultRoom> getRooms();
	
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
