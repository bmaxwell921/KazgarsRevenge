package main.kazgarsrevenge.model;

/**
 * Interface defining behavior for objects that have names
 * @author Brandon
 *
 */
public interface Nameable {

	/**
	 * Returns the name of this object
	 * @return
	 * 		The name of this object
	 */
	public String getName();
	
	/**
	 * Sets this object's name
	 * @param name
	 * 			The name of this object
	 */
	public void setName(String name);
}
