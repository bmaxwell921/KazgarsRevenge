package main.kazgarsrevenge.model.impl;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.EditableChunkComponent;

/**
 * A room is only ever made up of blocks, which are 1x1
 * @author Brandon
 *
 */
public class Room extends EditableChunkComponent {

	// Since RoomBlocks only take up a 1x1 space we map from location to block for convenient collision detection
	private Map<Location, RoomBlock> blocks;
	
	public Room() {
		this(ChunkComponent.DEFAULT_LOCATION, ChunkComponent.DEFAULT_NAME, ChunkComponent.DEFAULT_ROTATION);
	}

	public Room(Location location, String name, Rotation rotation) {
		super(location, name, rotation);
		blocks = new HashMap<>();
	}

	/**
	 * This will throw a ClassCastException is anything but RoomBlocks are added
	 */
	@Override
	public boolean add(ChunkComponent item) {
		// Only add if there isn't a collision with something already there
		if (hasCollision(item)) {
			return false;
		}
		blocks.put(item.getLocation(), (RoomBlock)item);
		return true;
	}
	
	private boolean hasCollision(ChunkComponent item) {
		// Checks to see if this added location collides with any other blocks
		if (!blocks.containsKey(item.getLocation())) {
			return false;
		}
		ChunkComponent resident = blocks.get(item.getLocation());
		return !resident.equals(item);
	}

	@Override
	public ChunkComponent remove(Location location) {	
		if (!blocks.containsKey(location)) {
			return null;
		}
		return blocks.remove(location);
	}

	@Override
	public int getWidth() {
		if (blocks.isEmpty()) {
			return 0;
		}
		
		if (blocks.size() == 1) {
			return RoomBlock.BLOCK_SIZE;
		}
		
		// If there's at least 2 blocks we subtract the largest x location from the smallest x location
		List<Location> sortedLocs = new ArrayList<>(blocks.keySet());
		Collections.sort(sortedLocs, new XComponentComp());
		
		// Add 1 because that's how it works. Truuuuust me ;)
		return sortedLocs.get(sortedLocs.size() - 1).getX() - sortedLocs.get(0).getX() + 1;
	}

	@Override
	public int getHeight() {
		if (blocks.isEmpty()) {
			return 0;
		}
		
		if (blocks.size() == 1) {
			return RoomBlock.BLOCK_SIZE;
		}

		// Works the exact same was as above
		List<Location> sortedLocs = new ArrayList<>(blocks.keySet());
		Collections.sort(sortedLocs, new YComponentComp());
		return sortedLocs.get(sortedLocs.size() - 1).getY() - sortedLocs.get(0).getY() + 1;
	}
	
	public Set<Location> getOccupiedLocations() {
		return blocks.keySet();
	}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = super.hashCode();
		result = prime * result + ((blocks == null) ? 0 : blocks.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (!super.equals(obj))
			return false;
		if (getClass() != obj.getClass())
			return false;
		Room other = (Room) obj;
		if (blocks == null) {
			if (other.blocks != null)
				return false;
		} else if (!blocks.equals(other.blocks))
			return false;
		return true;
	}

	@Override
	public String toString() {
		return "Room [" + this.getName() + "]";
	}

	private class XComponentComp implements Comparator<Location> {
		public int compare(Location lhs, Location rhs) {
			return lhs.getX() - rhs.getX();
		}
	}
	
	private class YComponentComp implements Comparator<Location> {
		public int compare(Location lhs, Location rhs) {
			return lhs.getY() - rhs.getY();
		}
		
	}
}
