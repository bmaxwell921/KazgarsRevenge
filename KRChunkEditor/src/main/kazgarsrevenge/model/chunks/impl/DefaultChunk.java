package main.kazgarsrevenge.model.chunks.impl;

import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.model.IRoom;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;

import com.google.gson.Gson;

/**
 * The default implementation of a chunk
 * @author bmaxwell
 *
 */
public class DefaultChunk implements IChunk {

	// The name of this chunk
	private String name;
	
	// All of the rooms in this chunk
	private List<DefaultRoom> rooms;
	
	// This chunk's location in 2D space
	private Location location;
	
	public DefaultChunk(String name, Location location) {
		this.name = name;
		this.location = location;
		rooms = new ArrayList<>();
	}
	
	@Override
	public void addRoom(DefaultRoom... rooms) {
		// JAVA GOTO!
		outer:
		for (DefaultRoom room : rooms) {
			for (IRoom other : this.rooms) {
				Rectangle roomBound = room.getBoundingRect();
				Rectangle otherBound = other.getBoundingRect();
				if (room.getBoundingRect().intersects(other.getBoundingRect())) {
					System.out.println("Found a collision, moving on");
					continue outer;
				}
			}
			this.rooms.add(room);
		}
	}
	
	@Override
	public List<DefaultRoom> getRooms() {
		return new ArrayList<>(rooms);
	}

	@Override
	public Location getLocation() {
		return location;
	}

	@Override
	public String getChunkName() {
		return name;
	}
	
	@Override
	public String getJsonRep() {
		// TODO move the rooms so the top left is (0, 0)
		return new Gson().toJson(this);
	}

}
