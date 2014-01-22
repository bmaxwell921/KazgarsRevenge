package main.kazgarsrevenge.model;

import java.util.ArrayList;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * An object representing ChunkComponents that can be edited.
 * @author Brandon
 *
 */
public abstract class EditableChunkComponent<T extends ChunkComponent> extends ChunkComponent implements
		Editable<T> {
	
	public EditableChunkComponent() {
		this(ChunkComponent.DEFAULT_LOCATION, ChunkComponent.DEFAULT_NAME, ChunkComponent.DEFAULT_ROTATION);
	}

	public EditableChunkComponent(Location location, String name,
			Rotation rotation) {
		super(location, name, rotation);
	}
	
	public abstract List<T> getComponents();
	
	/**
	 * Returns a list of all the occupied locations in this object, based on how it's rotated
	 * @return
	 */
	public List<Location> getRotatedLocations() {
		List<T> components = this.getComponents();
		List<Location> result = new ArrayList<Location>();
		int width = this.getWidth();
		int height = this.getHeight();
		for (T comp : components) {
			Location rotated = comp.getLocation();
			int num90Rotations = this.getRotation().getDegrees() / 90;
			// Rotate 90 degrees multiple times
			for (int i = 0; i < num90Rotations; ++i) {
				rotated = this.ninetyRotation(rotated, width, height);
			}
			result.add(rotated);
		}
		return result;
	}
	
	private Location ninetyRotation(Location loc, int width, int height) {
		// Just the formula for rotating by 90 degrees counterclockwise
		return new Location(loc.getY(), width - loc.getX() - 1);
	}
}
