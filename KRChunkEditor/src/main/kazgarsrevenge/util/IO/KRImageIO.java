package main.kazgarsrevenge.util.IO;

import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import javax.imageio.ImageIO;

import main.kazgarsrevenge.util.ImageUtility;

public class KRImageIO {
	
	public static final int IMAGE_SIZE = 25;
	
	public static final double IMAGE_SCALE = 0.25;
	
	// This should be plenty
	private static final Set<String> ACCEPTED_EXT = new HashSet<String>() {{
		add(".png");
		add(".jpeg");
		add(".jpg");
	}};
	
	// Where the images are located
	public static final String BASE_BLOCK_FILE_PATH = "." + File.separatorChar + "img" + File.separatorChar + "blocks";	
	public static final String BASE_ROOM_FILE_PATH = "." + File.separatorChar + "img" + File.separatorChar + "rooms";	
	public static final String UNKNOWN_IMAGE_PATH = "." + File.separatorChar + "img" + File.separator + "unknown.png";
	
	/**
	 * Loads all of the room images and returns them in a map.
	 * @return
	 * 		A mapping from imageName to image for all the rooms
	 */
	public static Map<String, BufferedImage> loadRooms() {
		return loadNewRooms(new HashSet<String>());
	}
	
	public static Map<String, BufferedImage> loadNewRooms(Set<String> knownRooms) {
		Map<String, BufferedImage> newRooms = new HashMap<>();
		loadImagesInto(knownRooms, BASE_ROOM_FILE_PATH, newRooms);
		return newRooms;
	}
	
	/**
	 * Loads all the images for blocks found at the default location.
	 * @return
	 * 		A mapping from imageName to image for all the blocks
	 */
	public static Map<String, BufferedImage> loadBlocks() {
		Map<String, BufferedImage> blocks = new HashMap<>();
		loadImagesInto(new HashSet<String>(), BASE_BLOCK_FILE_PATH, blocks);
		return blocks;
	}
	
	/**
	 * Loads the default 'Unknown' block image
	 * @return
	 */
	public static BufferedImage loadUnknownImage() {
		return loadAndScaleImage(new File(UNKNOWN_IMAGE_PATH), IMAGE_SCALE);
	}
	
	/*
	 *  Loads all images in the given folder into the given map. If the folder contains other folders
	 *  it recursively loads all images in that folder
	 */
	private static void loadImagesInto(Set<String> knownImages, String folderPath, Map<String, BufferedImage> destination) {
		File[] images = new File(folderPath).listFiles();
		if (images == null) {
			return;
		}
		for (File image : images) {
			if (image.isDirectory()) {
				loadImagesInto(knownImages, image.getAbsolutePath(), destination);
				continue;
			}
			
			// Ignore things that aren't images
			if (!isImage(image)) {
				continue;
			}
			
			String imgName = image.getName().substring(0, image.getName().indexOf('.'));
			
			// Ignore things that have already been loaded
			if (knownImages.contains(imgName)) {
				continue;
			}
			
			BufferedImage scaled = loadAndScaleImage(image, IMAGE_SCALE);
			destination.put(imgName, scaled);
		}
	}
	
	private static boolean isImage(File file) {
		return ACCEPTED_EXT.contains(getExtension(file));
	}
	
	private static String getExtension(File file) {
		String fullName = file.getName();
		return fullName.substring(fullName.indexOf('.'));
	}
	
	private static BufferedImage loadAndScaleImage(File location, double scale) {
		try {
			BufferedImage img = ImageIO.read(location);
			return ImageUtility.scaleImage(img, scale);
		} catch (IOException ioe) {
			System.out.println(String.format("Unable to load image: %s", location.getAbsolutePath()));
			ioe.printStackTrace();
			return null;
		}
	}
	
	public static void saveNewRoom(BufferedImage image, String imageName) {
		if (!imageName.endsWith(".png")) {
			imageName += ".png";
		}
		try {
			ImageIO.write(image, "png", new File(BASE_ROOM_FILE_PATH + File.separatorChar + imageName));
		} catch (IOException e) {
			System.out.println("Unable to save image");
			e.printStackTrace();
		}
	}
}
