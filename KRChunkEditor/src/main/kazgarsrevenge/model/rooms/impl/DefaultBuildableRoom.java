package main.kazgarsrevenge.model.rooms.impl;

import java.util.List;

import main.kazgarsrevenge.model.IAttachableRoom;
import main.kazgarsrevenge.model.IRoomBlock;

/**
 * Object representing a room that can be built from scratch
 * @author Brandon
 *
 */
public class DefaultBuildableRoom extends DefaultRoom implements
		IAttachableRoom {

	public DefaultBuildableRoom(String name, List<IRoomBlock> rooms) {
		super(name, rooms);
	}

	@Override
	public void attachBlock(IRoomBlock block) {
		if (block == null) throw new NullPointerException("Null block added");
		super.rooms.add(block);
	}

}
