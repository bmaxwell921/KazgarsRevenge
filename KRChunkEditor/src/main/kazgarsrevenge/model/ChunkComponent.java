package main.kazgarsrevenge.model;

import java.lang.reflect.Constructor;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

/**
 * All basic objects in the Chunk are locatable, nameable, and rotatable.
 * @author Brandon
 *
 */
public abstract class ChunkComponent implements Locatable, Nameable, Rotatable, Cloneable {

	protected static final Location DEFAULT_LOCATION = new Location();
	protected static final String DEFAULT_NAME = "DEFAULT";
	protected static final Rotation DEFAULT_ROTATION = Rotation.ZERO;
	
	// The current location of this component
	private Location location;
	
	// The name of this component
	private String name;
	
	// The rotation of this component
	private Rotation rotation;
	
	public ChunkComponent() {
		this(DEFAULT_LOCATION, DEFAULT_NAME, DEFAULT_ROTATION);
	}
	
	public ChunkComponent(Location location, String name, Rotation rotation) {
		this.location = location;
		this.name = name;
		this.rotation = rotation;
	}
	
	@Override
	public Location getLocation() {
		return location;
	}

	@Override
	public void setLocation(Location location) {
		this.location = location;
	}

	@Override
	public String getName() {
		return name;
	}

	@Override
	public void setName(String name) {
		this.name = name;
	}
	
	@Override
	public void setRotation(Rotation newRotation) {
		this.rotation = newRotation;
	}
	
	@Override
	public Rotation getRotation() {
		return rotation;
	}
	
	/**
	 * Returns the Rotation.ZERO width of this component
	 * @return
	 */
	public abstract int getWidth();
	
	/**
	 * Returns the Rotation.ZERO height of this component
	 * @return
	 */
	public abstract int getHeight();
	
	/**
	 * Returns the width of the component, applying its current rotation
	 * @return
	 */
	public int getRotatedWidth() {
		if (this.rotation == Rotation.NINETY || this.rotation == Rotation.ONE_EIGHTY) {
			return this.getHeight();
		}
		return this.getWidth();
	}
	
	/**
	 * Returns the height of the component, applying its current rotation
	 * @return
	 */
	public int getRotatedHeight() {
		if (this.rotation == Rotation.NINETY || this.rotation == Rotation.ONE_EIGHTY) {
			return this.getWidth();
		}
		return this.getHeight();
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((location == null) ? 0 : location.hashCode());
		result = prime * result + ((name == null) ? 0 : name.hashCode());
		result = prime * result
				+ ((rotation == null) ? 0 : rotation.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		ChunkComponent other = (ChunkComponent) obj;
		if (location == null) {
			if (other.location != null)
				return false;
		} else if (!location.equals(other.location))
			return false;
		if (name == null) {
			if (other.name != null)
				return false;
		} else if (!name.equals(other.name))
			return false;
		if (rotation != other.rotation)
			return false;
		return true;
	}

	@Override
	public String toString() {
		return "ChunkComponent [location=" + location + ", name=" + name
				+ ", rotation=" + rotation + "]";
	}
	
	@Override
	public Object clone() {
		// Get the default constructor
		try {
			Constructor<?> cons = this.getClass().getConstructor(new Class[] {});
			ChunkComponent clone = (ChunkComponent) cons.newInstance();
			clone.location = (Location) this.location.clone();
			clone.name = new String(this.name);
			clone.rotation = this.rotation;
			return clone;
		} catch (Exception e) {
			System.out.println("Problems cloning");
			return null;
		}
	}
	
	public String getJsonRep() {
		Gson gson = new GsonBuilder().setPrettyPrinting().create();
		return gson.toJson(this);
	}
}
