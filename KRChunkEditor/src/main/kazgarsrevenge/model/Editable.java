package main.kazgarsrevenge.model;

import main.kazgarsrevenge.data.Location;

/**
 * An interface defining behavior for items that are editable
 * @author Brandon
 *
 * @param <T>
 */
public interface Editable<T extends Locatable> {

	/**
	 * Adds the given item to this editable at the location in the given Item
	 * @param item
	 * 				The item to be added
	 * @return	
	 * 		Returns success or failure of the add
	 */
	public boolean add(T item);
	
	/**
	 * Removes and returns the item which resides at the given location
	 * @param location
	 * 				The location where the item to remove is found
	 * @return
	 * 		The removed item
	 */
	public T remove(Location location);
}
