package main.kazgarsrevenge.model.rooms.impl;

import java.util.List;

import main.kazgarsrevenge.model.IAttachableRoom;
import main.kazgarsrevenge.model.blocks.impl.DefaultBlock;

/**
 * Object representing a room that can be built from scratch
 * @author Brandon
 *
 */
public class DefaultBuildableRoom extends DefaultRoom implements
		IAttachableRoom {

	public DefaultBuildableRoom(String name, List<DefaultBlock> rooms) {
		super(name, rooms);
	}

	@Override
	public void attachBlock(DefaultBlock block) {
		if (block == null) throw new NullPointerException("Null block added");
		super.rooms.add(block);
	}

}
