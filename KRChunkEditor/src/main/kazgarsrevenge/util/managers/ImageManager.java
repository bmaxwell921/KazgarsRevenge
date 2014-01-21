package main.kazgarsrevenge.util.managers;

import java.awt.image.BufferedImage;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;

import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;
import main.kazgarsrevenge.util.IO.KRImageIO;

/**
 * Manages operations using Images. Uses Singleton pattern
 * @author Brandon
 *
 */
public class ImageManager {
	
	// THE INSTANCE
	private static ImageManager instance;

	// Map of images for chunk components. Mapped by type, then name
	private Map<Class<? extends ChunkComponent>, Map<String, BufferedImage>> images;
	
	private BufferedImage unknownImage;
	
	private ImageManager() {
		images = new HashMap<>();
		addBlocks();
		addRooms();
		addUnknown();
	}
	
	/**
	 * Gets an instance of the class
	 * @return
	 */
	public static ImageManager getInstance() {
		if (instance == null) {
			instance = new ImageManager();
		}
		return instance;
	}
	
	// Adds all of the known RoomBlock images
	private void addBlocks() {
		Map<String, BufferedImage> blockImages = KRImageIO.loadBlocks();
		images.put(RoomBlock.class, blockImages);
	}
	
	// Adds all of the know Room images
	private void addRooms() {
		Map<String, BufferedImage> roomImages = KRImageIO.loadRooms();
		images.put(Room.class, roomImages);
	}
	
	private void addUnknown() {
		unknownImage = KRImageIO.loadUnknownImage();
	}
	
	/**
	 * Loads any rooms found in the default folder that haven't already been loaded
	 */
	public void loadNewRooms() {
		Map<String, BufferedImage> knownImages = images.get(Room.class);
		Map<String, BufferedImage> newImages = KRImageIO.loadNewRooms(knownImages.keySet());
		for (String key : newImages.keySet()) {
			knownImages.put(key, newImages.get(key));
		}
	}
	
	/**
	 * Returns the image with the associated with the given component and name
	 * @param clazz
	 * @param imageName
	 * @return
	 */
	public BufferedImage getImage(Class<? extends ChunkComponent> clazz, String imageName) {
		Map<String, BufferedImage> map = images.get(clazz);
		if (!map.containsKey(imageName)) {
			return unknownImage;
		}
		return map.get(imageName);
	}
	
	/**
	 * Returns the name for the images associated with the given class
	 * @param clazz
	 * @return
	 */
	public Set<String> getNames(Class<? extends ChunkComponent> clazz) {
		return images.get(clazz).keySet();
	}
}
