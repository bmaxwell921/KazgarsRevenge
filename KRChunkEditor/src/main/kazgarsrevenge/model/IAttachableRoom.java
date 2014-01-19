package main.kazgarsrevenge.model;

import main.kazgarsrevenge.model.blocks.impl.DefaultBlock;

/**
 * Object used to represent editable rooms. Users can
 * attach new blocks to this room to create an entirely
 * different room.
 * @author Brandon
 *
 */
public interface IAttachableRoom extends IRoom {

	/**
	 * Attaches the given block to this room
	 * @param block
	 */
	public void attachBlock(DefaultBlock block);
}
