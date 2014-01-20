package main.kazgarsrevenge.model;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;

/**
 * An object representing ChunkComponents that can be edited.
 * @author Brandon
 *
 */
public abstract class EditableChunkComponent extends ChunkComponent implements
		Editable<ChunkComponent> {
	
	public EditableChunkComponent() {
		this(ChunkComponent.DEFAULT_LOCATION, ChunkComponent.DEFAULT_NAME, ChunkComponent.DEFAULT_ROTATION);
	}

	public EditableChunkComponent(Location location, String name,
			Rotation rotation) {
		super(location, name, rotation);
	}
}
