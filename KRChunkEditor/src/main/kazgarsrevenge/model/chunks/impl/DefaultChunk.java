package main.kazgarsrevenge.model.chunks.impl;

import java.util.ArrayList;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.model.IRoom;

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
	private List<IRoom> rooms;
	
	// This chunk's location in 2D space
	private Location location;
	
	public DefaultChunk(String name, Location location) {
		this.name = name;
		this.location = location;
		rooms = new ArrayList<>();
	}
	
	@Override
	public void addRoom(IRoom... rooms) {
		for (IRoom room : rooms) {
			this.rooms.add(room);
		}
	}
	
	@Override
	public List<IRoom> getRooms() {
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
