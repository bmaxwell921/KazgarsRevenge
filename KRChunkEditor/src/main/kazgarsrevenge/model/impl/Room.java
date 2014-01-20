package main.kazgarsrevenge.model.impl;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Set;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.EditableChunkComponent;

/**
 * A room is only ever made up of blockMap, which are 1x1.
 * 
 * Well this USED to be super awesome with how i held the blocks, but since GSON APPARENTLY CAN'T 
 * HANDLE DESERIALIZING MAPS I HAD TO CHANGE IT TO USING A SHITTY LIST!
 * @author Brandon
 *
 */
public class Room extends EditableChunkComponent<RoomBlock> {
	
	private List<RoomBlock> blocks;
	
	public Room() {
		this(ChunkComponent.DEFAULT_LOCATION, ChunkComponent.DEFAULT_NAME, ChunkComponent.DEFAULT_ROTATION);
	}

	public Room(Location location, String name, Rotation rotation) {
		super(location, name, rotation);
		blocks = new ArrayList<RoomBlock>();
	}

	/**
	 * This will throw a ClassCastException is anything but RoomBlocks are added
	 */
	@Override
	public boolean add(RoomBlock item) {
		// Only add if there isn't a collision with something already there
		if (hasCollision(item)) {
			return false;
		}
		blocks.add((RoomBlock) item);
		return true;
	}
	
	private boolean hasCollision(ChunkComponent item) {
		// Checks to see if this added location collides with any other blockMap
		for (RoomBlock block : blocks) {
			if (block.getLocation().equals(item.getLocation())) {
				return true;
			}
		}
		
		return false;
	}

	@Override
	public RoomBlock remove(Location location) {	
		for (Iterator<RoomBlock> iter = blocks.iterator(); iter.hasNext(); ){
			RoomBlock cur = iter.next();
			if (cur.getLocation().equals(location)) {
				iter.remove();
				return cur;
			}
		}
		return null;
	}

	@Override
	public int getWidth() {		
		if (blocks.isEmpty()) {
			return 0;
		}
		
		if (blocks.size() == 1) {
			return RoomBlock.BLOCK_SIZE;
		}
		
		// If there's at least 2 blockMap we subtract the largest x location from the smallest x location
		List<Location> sortedLocs = getOccupiedLocations();
		
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
		List<Location> sortedLocs = getOccupiedLocations();
		
		Collections.sort(sortedLocs, new YComponentComp());
		return sortedLocs.get(sortedLocs.size() - 1).getY() - sortedLocs.get(0).getY() + 1;
	}
	
	public List<Location> getOccupiedLocations() {
		List<Location> locs = new ArrayList<>();
		for (RoomBlock block : blocks) {
			locs.add(block.getLocation());
		}
		return locs;
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

	@Override
	public List<RoomBlock> getComponents() {
		return blocks;
	}
}
