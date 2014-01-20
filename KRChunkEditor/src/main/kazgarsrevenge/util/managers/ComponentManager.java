package main.kazgarsrevenge.util.managers;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;
import main.kazgarsrevenge.util.IO.ChunkComponentIO;

/**
 * Class uses to get get instances of each type of component. Uses factory pattern for convenience
 * @author Brandon
 *
 */
public class ComponentManager {

	// Mapping from class (in this case it will be RoomBlock and Room) to Map of component name to component
	private Map<Class<? extends ChunkComponent>, Map<String, ChunkComponent>> components;
	
	private static ComponentManager instance;
	
	private ComponentManager() {
		components = new HashMap<>();
		addBlocks();
		addRooms();
	}
	
	private void addBlocks() {
		Map<String, ChunkComponent> blockMap = new HashMap<>();
		List<RoomBlock> blocks = ChunkComponentIO.loadAllBlocks();
		for (RoomBlock block : blocks) {
			blockMap.put(block.getName(), block);
		}
		components.put(RoomBlock.class, blockMap);
	}
	
	private void addRooms() {
		Map<String, ChunkComponent> roomMap = new HashMap<>();
		List<Room> rooms = ChunkComponentIO.loadAllRooms();
		for (Room room : rooms) {
			roomMap.put(room.getName(), room);
		}
		components.put(Room.class, roomMap);
	}
	
	/**
	 * Method used to get a ComponentManager object
	 * @return
	 */
	public static ComponentManager getInstance() {
		if (instance == null) {
			instance = new ComponentManager();
		}
		return instance;
	}
	
	/**
	 * Returns a copy of the component with the given name and type
	 * @param clazz
	 * @param componentName
	 * @return
	 */
	public ChunkComponent getComponent(Class<? extends ChunkComponent> clazz, String componentName) {
		Map<String, ChunkComponent> map = components.get(clazz);
		if (!map.containsKey(componentName)) {
			System.out.println(String.format("Unrecognized component name: %s, type: %s", componentName, clazz));
			return null;
		}
		return (ChunkComponent) map.get(componentName).clone();
	}
	
	/**
	 * Gets a set of all ChunkComponent names associated with the given type
	 * @param clazz
	 * @return
	 */
	public Set<String> getNames(Class<? extends ChunkComponent> clazz) {
		return components.get(clazz).keySet();
	}
}
