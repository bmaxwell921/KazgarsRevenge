package main.kazgarsrevenge.util;

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Image;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;

import javax.imageio.ImageIO;

public class ImageLoader {
	
	// Where the images are located
	private static final String BASE_BLOCK_FILE_PATH = "." + File.separatorChar + "img" + File.separatorChar + "blocks";	
	private static final String BASE_ROOM_FILE_PATH = "." + File.separatorChar + "img" + File.separatorChar + "rooms";	
	private static final String UNKNOWN_IMAGE_PATH = "." + File.separatorChar + "unknown.png";
	
	private static Map<String, Image> roomImageMap;
	private static Map<String, Image> blockImageMap;
	
	private static Image UNKNOWN_IMAGE; 
	
	static {
		try {
			BufferedImage unknown = ImageIO.read(new File(UNKNOWN_IMAGE_PATH));
			UNKNOWN_IMAGE = scaleImage(unknown);
		} catch (IOException ioe) {
			UNKNOWN_IMAGE = null;
		}
	}
	
	public static void loadImages() {
		roomImageMap = new HashMap<String, Image>();
		blockImageMap = new HashMap<String, Image>();
		
		loadImagesInto(BASE_ROOM_FILE_PATH, roomImageMap);
		loadImagesInto(BASE_BLOCK_FILE_PATH, blockImageMap);
	}
	
	private static void loadImagesInto(String folderPath, Map<String, Image> destination) {
		File[] images = new File(folderPath).listFiles();
		for (File image : images) {
			if (image.isDirectory()) {
				System.out.println(String.format("Found directory while loading image: %s", image.getAbsolutePath()));
				continue;
			}
			try {
				BufferedImage img = ImageIO.read(image);
				Image reImg = scaleImage(img);
				destination.put(image.getName().substring(0, image.getName().indexOf('.')), reImg);
			} catch (IOException e) {
				System.out.println(String.format("Unable to read file: %s", image.getAbsolutePath()));
				continue;
			}
		}
	}
	
	private static Image scaleImage(BufferedImage img) {
		Image scaledImage = img.getScaledInstance(img.getWidth() / 4, img.getHeight() / 4, Image.SCALE_DEFAULT);
		return scaledImage;
	}
	
	// Gets the specified image
	public static Image getBlockImage(String name) {
		if (!blockImageMap.containsKey(name)) throw new IllegalArgumentException(String.format("Image name not found: %s", name));
		return blockImageMap.get(name);
	}
	
	public static Set<String> getBlockImageNames() {
		return blockImageMap.keySet();
	}
	
	public static Image getRoomImage(String name) {
		if (!roomImageMap.containsKey(name)) {
			System.out.println(String.format("Couldn't find a room with the name: %s", name));
			return UNKNOWN_IMAGE;
		}
		return roomImageMap.get(name);
	}
	
	public static Set<String> getRoomImageNames() {
		return roomImageMap.keySet();
	}

}
