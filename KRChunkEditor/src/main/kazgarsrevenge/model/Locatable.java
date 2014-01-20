package main.kazgarsrevenge.model;

import main.kazgarsrevenge.data.Location;

/**
 * Interface defining behavior for objects that have locations
 * @author Brandon
 *
 */
public interface Locatable {

	/**
	 * Returns this object's location
	 * @return
	 */
	public Location getLocation();
	
	/**
	 * Sets this object's location to the given location
	 * @param location
	 */
	public void setLocation(Location location);
}
