package main.kazgarsrevenge.util;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import main.kazgarsrevenge.model.IRoomBlock;
import main.kazgarsrevenge.model.blocks.impl.ARoomBlock;
import main.kazgarsrevenge.model.blocks.impl.BossRoom;
import main.kazgarsrevenge.model.blocks.impl.DefaultBlock;
import main.kazgarsrevenge.model.blocks.impl.DoorBlock;
import main.kazgarsrevenge.model.blocks.impl.FloorBlock;
import main.kazgarsrevenge.model.blocks.impl.MobSpawn;
import main.kazgarsrevenge.model.blocks.impl.PlayerSpawn;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;

/**
 * Factory class used to create IRooms
 * @author Brandon
 *
 *
 *
 *
 */
public class RoomFactory {
	
	private static final String NAME_KEY = "name";
	private static final String BLOCKS_KEY = "blocks";
	
	/**
	 * Converts the given json representation of a DefaultRoom into a concrete object.
	 * 
	 * Room Json should be:
	 * 		{
	 * 			name : <ROOM_NAME>,
	 * 			blocks : 
	 * 			[
	 * 				{
	 * 					type : <FLOOR_TYPE>,
	 * 					roomLoc : 
	 * 					{
	 * 						x : <X_LOC>,
	 * 						y : <Y_LOC>
	 * 					},
	 * 					rot : <ROTATION_ENUM_NAME>
	 * 				}
	 * 
	 * 		}
	 * 
	 * Where the values in <> are filled in appropriately
	 * @param json
	 * @return
	 */
	public DefaultRoom getRoom(JsonElement json) {
		Gson gson = new Gson();
		JsonObject jsonRoom = json.getAsJsonObject();
		
		String name = jsonRoom.get(NAME_KEY).toString();
		JsonArray blocks = jsonRoom.get(BLOCKS_KEY).getAsJsonArray();
		List<IRoomBlock> blockList = new ArrayList<IRoomBlock>();
		
		for (JsonElement block : blocks) {
			blockList.add(gson.fromJson(block, DefaultBlock.class));
		}
		
		return new DefaultRoom(name, blockList);
	}
}
