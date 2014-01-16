package main.kazgarsrevenge.model.rooms.impl;

import java.awt.Rectangle;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.IRoom;
import main.kazgarsrevenge.model.IRoomBlock;

/**
 * Object used to represent pre-defined rooms. Default 
 * rooms cannot be changed after construction
 * @author Brandon
 *
 */
public class DefaultRoom implements IRoom {

	// The 'name' of this room
	private String name;
	
	// This room's location in 2D space
	private Location loc;
	
	// The blocks making up this room
	protected List<IRoomBlock> rooms;
	
	/**
	 * Constructs a room with the given blocks. For proper performance
	 * the given list should not be empty
	 * @param rooms
	 */
	public DefaultRoom(String name, List<IRoomBlock> rooms) {
		this.name = name;
		if (rooms == null) {
			this.rooms = new ArrayList<>();
			return;
		}
		this.rooms = new ArrayList<>(rooms);
	}

	@Override
	public List<IRoomBlock> getRoomBlocks() {
		// nobody is messin with my shit
		return new ArrayList<>(rooms);
	}

	@Override
	public Location getLocation() {
		return loc;
	}
	
	@Override
	public String getRoomName() {
		return name;
	}

	@Override
	public Rectangle getBoundingRect() {
		if (rooms.isEmpty()) {
			return new Rectangle();
		}
		
		// comparator to find the largest x value
		final Comparator<Location> maxX = new Comparator<Location>() {
			public int compare(Location lhs, Location rhs) {
				return lhs.getX() - rhs.getX();
			}
		};
		
		// comparator to find the smallest x value
		final Comparator<Location> minX = new Comparator<Location>() {
			public int compare(Location lhs, Location rhs) {
				return maxX.compare(rhs, lhs);
			}
		};
		
		// comparator to find the largest y value
		final Comparator<Location> maxY = new Comparator<Location>() {
			public int compare(Location lhs, Location rhs) {
				return lhs.getY() - rhs.getY();
			}
		};
		
		// comparator to find the smallest y value
		final Comparator<Location> minY = new Comparator<Location>() {
			public int compare(Location lhs, Location rhs) {
				return maxY.compare(rhs, lhs);
			}
		};
		
		int maxXVal = findMax(maxX, true);
		int minXVal = findMax(minX, true);
		int maxYVal = findMax(maxY, false);
		int minYVal = findMax(minY, false);
		
		return new Rectangle(minXVal, minYVal, maxXVal - minXVal + 1, maxYVal - minYVal + 1);
	}
	
	private int findMax(Comparator<Location> comp, boolean useX) {
		Location max = rooms.get(0).getRelativeLocation();
		
		for (int i = 0; i < rooms.size(); ++i) {
			if (comp.compare(rooms.get(i).getRelativeLocation(), max) > 0) {
				max = rooms.get(i).getRelativeLocation();
			}
		}
		
		return getValue(max, useX);
	}
	
	private int getValue(Location loc, boolean useX) {
		return useX ? loc.getX() : loc.getY();
	}
	
	@Override
	public Object clone() {
		try {
			DefaultRoom clone = (DefaultRoom) super.clone();
			clone.name = this.name;
			clone.loc = this.loc;
			clone.rooms = new ArrayList<>(this.rooms);
			return clone;
		} catch (CloneNotSupportedException cnse) {
			cnse.printStackTrace();
			return null;
		}
	}
}
