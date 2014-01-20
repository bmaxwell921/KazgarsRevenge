package main.kazgarsrevenge.model.impl;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;

/**
 * An object representing one 'block' of a room.
 * Blocks take up a single 1x1 area
 * @author Brandon
 *
 */
public class RoomBlock extends ChunkComponent {
	
	// Rooms are 1x1 blocks
	public static final int BLOCK_SIZE = 1;
	
	public RoomBlock() {
		super();
	}
	
	public RoomBlock(Location location, String name, Rotation rotation) {
		super(location, name, rotation);
	}

	@Override
	public int getWidth() {
		return BLOCK_SIZE;
	}

	@Override
	public int getHeight() {
		return BLOCK_SIZE;
	}
}
