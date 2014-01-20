package main.kazgarsrevenge.model;

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
}
