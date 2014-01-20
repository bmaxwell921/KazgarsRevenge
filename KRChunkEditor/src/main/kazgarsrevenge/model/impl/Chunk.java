package main.kazgarsrevenge.model.impl;

import java.awt.Rectangle;
import java.util.HashSet;
import java.util.Iterator;
import java.util.Set;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.EditableChunkComponent;

/**
 * A class representing one Chunk in Kazgar's Revenge
 * @author Brandon
 *
 */
public class Chunk extends EditableChunkComponent {

	// Apparently we decided this?
	public static final int CHUNK_SIZE = 24;
	
	// All the rooms in this chunk
	private Set<Room> rooms;
	
	public Chunk() {
		this(ChunkComponent.DEFAULT_LOCATION, ChunkComponent.DEFAULT_NAME, ChunkComponent.DEFAULT_ROTATION);
	}
	
	public Chunk(Location location, String name, Rotation rotation) {
		this.rooms = new HashSet<>();
	}

	@Override
	public boolean add(ChunkComponent item) {
		if (hasCollision(item)) {
			return false;
		}
		rooms.add((Room) item);
		return true;
	}
	
	private boolean hasCollision(ChunkComponent item) {
		// We already have to iterate over everything so just check if it's there already
		if (rooms.contains(item)) {
			return true;
		}
		
		/*
		 *  Here's the idea, it's similar to sophisticated image detection:
		 *  	1) We use the 'bounding box' of rooms to do a general collision detection
		 *  		if the bounding box doesn't collide we know everything is good
		 *  	2) If we see a bounding box collision, that doesn't mean there's going to be
		 *  		a RoomBlock collision, so check all the Rooms to make sure.
		 */
		Rectangle addBox = new Rectangle(item.getLocation().getX(), item.getLocation().getY(), item.getWidth(), item.getHeight());
		
		for (Room room : rooms) {
			Rectangle otherBox = new Rectangle(room.getLocation().getX(), room.getLocation().getY(), room.getWidth(), room.getHeight());
			if (addBox.intersects(otherBox) && hasBlockCollision((Room) item, room)) {
				return true;
			}
		}
		return false;
	}
	
	private boolean hasBlockCollision(Room add, Room existing) {
		Set<Location> addLocs = add.getOccupiedLocations();
		Set<Location> existingLocs = existing.getOccupiedLocations();
		
		for (Location addL : addLocs) {
			if (existingLocs.contains(addL)) {
				return true;
			}
		}
		return false;
	}

	@Override
	public ChunkComponent remove(Location location) {
		// Good ol' iterators
		for (Iterator<Room> iter = rooms.iterator(); iter.hasNext(); ) {
			Room cur = iter.next();
			if (cur.getLocation().equals(location)) {
				iter.remove();
				return cur;
			}
		}
		return null;
	}

	@Override
	public int getWidth() {
		return CHUNK_SIZE;
	}

	@Override
	public int getHeight() {
		return CHUNK_SIZE;
	}
	
	public static void main(String[] args) {
		Chunk c = new Chunk(new Location(), "My Chunk", Rotation.ZERO);
		Room r = new Room(new Location(), "My Room", Rotation.ZERO);
		c.add(r);
		System.out.println(c.getJsonRep());
	}
}