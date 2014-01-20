package main.kazgarsrevenge.model;

import main.kazgarsrevenge.data.Rotation;

/**
 * An interface defining behavior for objects that can rotate
 * @author Brandon
 *
 */
public interface Rotatable {

	/**
	 * Sets this object's rotation to the given value
	 * @param newRotation
	 */
	public void setRotation(Rotation newRotation);
	
	/**
	 * Returns the rotation of this object
	 * @return
	 */
	public Rotation getRotation();
}
