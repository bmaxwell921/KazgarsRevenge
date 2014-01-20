package main.kazgarsrevenge.util.IO;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;

import com.google.gson.Gson;

/**
 * Class used for saving and loading all types of Chunk Components
 * @author Brandon
 *
 */
public class ChunkComponentIO {
	
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
	public static <T extends ChunkComponent> T loadChunkComponent(Class<T> componentClass, File file, StringBuilder errors) {
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
		
		File defaultLoc = new File(DEFAULT_ROOMS_PATH);
		for (File subFile : defaultLoc.listFiles()) {
			if (!subFile.isDirectory()) {
				rooms.add(ChunkComponentIO.loadChunkComponent(Room.class, subFile, errors));
			}
		}
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
		List<RoomBlock> rooms = new ArrayList<>();
		StringBuilder errors = new StringBuilder();
		
		File defaultLoc = new File(DEFAULT_BLOCKS_PATH);
		for (File subFile : defaultLoc.listFiles()) {
			if (!subFile.isDirectory()) {
				rooms.add(ChunkComponentIO.loadChunkComponent(RoomBlock.class, subFile, errors));
			}
		}
		if (!errors.toString().isEmpty()) {
			System.out.println(errors.toString());
		}
		return rooms;
	}
}
