package main.kazgarsrevenge.util.IO;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;

import com.google.gson.Gson;

/**
 * Class used for saving and loading all types of Chunk Components
 * @author Brandon
 *
 */
public class ComponentIO {
	
	private static final String DEFAULT_ROOMS_PATH = "." + File.separatorChar + "rooms";
	private static final String DEFAULT_BLOCKS_PATH = "." + File.separatorChar + "blocks";
	
	/**
	 * Loads the specified chunkComponent from the given file. If there are any errors while loading,
	 * they'll be reported in 'errors'
	 * @param componentClass
	 * @param file
	 * @param errors
	 * @return
	 */
	public static ChunkComponent loadChunkComponent(Class<? extends ChunkComponent> componentClass, File file, StringBuilder errors) {
		StringBuilder fileContents = FullFileIO.readEntirely(file, errors);
		try {
			return new Gson().fromJson(fileContents.toString(), componentClass);
		} catch (Exception e) {
			errors.append("Unable to parse Json\n");
			errors.append(e.getMessage());
			return null;
		}
	}
	
	/**
	 * Saves the given component to the given file using its Json representation
	 * @param component
	 * @param saveFile
	 * @param errors
	 */
	public static void saveChunkComponent(ChunkComponent component, File saveFile, StringBuilder errors) {
		try (BufferedWriter bw = new BufferedWriter(new FileWriter(saveFile))) {
			bw.write(component.getJsonRep());
		} catch (IOException ioe) {
			errors.append(String.format("Unable to save file to: %s", saveFile.getName()));
			errors.append(ioe.getMessage());
		}
	}
	
	/**
	 * Loads all the known room files from the default location
	 * @return
	 */
	public static List<Room> loadAllRooms() {
		List<Room> rooms = new ArrayList<>();
		StringBuilder errors = new StringBuilder();
		
//		File defaultLoc = new File(DEFAULT_ROOMS_PATH);
//		for (File subFile : defaultLoc.listFiles()) {
//			if (!subFile.isDirectory()) {
//				rooms.add(ComponentIO.loadChunkComponent(Room.class, subFile, errors));
//			}
//		}
		loadAllComponents(Room.class, new HashSet<String>(), DEFAULT_ROOMS_PATH, rooms);
		if (!errors.toString().isEmpty()) {
			System.out.println(errors.toString());
		}
		return rooms;
	}
	
	/**
	 * Loads all of the know block files from the default location
	 * @return
	 */
	public static List<RoomBlock> loadAllBlocks() {
		List<RoomBlock> blocks = new ArrayList<>();
		StringBuilder errors = new StringBuilder();
		
//		File defaultLoc = new File(DEFAULT_BLOCKS_PATH);
//		for (File subFile : defaultLoc.listFiles()) {
//			if (!subFile.isDirectory()) {
//				rooms.add(ComponentIO.loadChunkComponent(RoomBlock.class, subFile, errors));
//			}
//		}
		loadAllComponents(RoomBlock.class, new HashSet<String>(), DEFAULT_BLOCKS_PATH, blocks);
		if (!errors.toString().isEmpty()) {
			System.out.println(errors.toString());
		}
		return blocks;
	}
	
	private static void loadAllComponents(Class<? extends ChunkComponent> clazz, Set<String> knownComps, 
			String folderPath, List destination) {
		File[] comps = new File(folderPath).listFiles();
		for (File comp : comps) {
			if (comp.isDirectory()) {
				loadAllComponents(clazz, knownComps, comp.getAbsolutePath(), destination);
				continue;
			}
			
			if (!isComponent(comp)) {
				continue;
			}
			
			String compName = comp.getName().substring(0, comp.getName().indexOf('.'));
			
			if (knownComps.contains(compName)) {
				continue;
			}
			
			// There's the warning here, but it should be ok. Idk how else to fix it
			destination.add(loadChunkComponent(clazz, comp, new StringBuilder()));
		}
	}
	
	private static boolean isComponent(File comp) {
		return comp.getName().endsWith(".json");
	}

	/**
	 * Returns all rooms whose file names aren't in the given set
	 * @param knownRooms
	 * @return
	 */
	public static List<ChunkComponent> loadNewRooms(Set<String> knownRooms) {
		List<ChunkComponent> newRooms = new ArrayList<>();
		loadAllComponents(Room.class, knownRooms, DEFAULT_ROOMS_PATH, newRooms);
		return newRooms;
	}
}
