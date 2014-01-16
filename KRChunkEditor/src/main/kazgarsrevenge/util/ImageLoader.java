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

	// size of scaled image
	private static final int SCALE_SIZE = 25;
	
	private static final String BASE_FILE_PATH = "." + File.separatorChar + "img";
	
	private static Map<String, Image> imageMap;
	
	public static void loadImages() {
		imageMap = new HashMap<String, Image>();
		File[] images = new File(BASE_FILE_PATH).listFiles();
		for (File image : images) {
			if (image.isDirectory()) {
				System.out.println(String.format("Found directory while loading images: %s", image.getAbsolutePath()));
				continue;
			}
			try {
				Image img = ImageIO.read(image);
				img = scaleImage(img);
				imageMap.put(image.getName(), img);
			} catch (IOException e) {
				System.out.println(String.format("Unable to read file: %s", image.getAbsolutePath()));
				continue;
			}
		}
	}
	
	private static Image scaleImage(Image img) {
		BufferedImage im = new BufferedImage(SCALE_SIZE, SCALE_SIZE, BufferedImage.TYPE_INT_RGB);
		Graphics2D g = im.createGraphics();
		g.setColor(Color.WHITE);
		g.fillRect(0, 0, im.getWidth(), im.getHeight());
		g.drawImage(img, 0, 0, SCALE_SIZE, SCALE_SIZE, null);
		g.dispose();
		return im;
	}
	
	// Gets the specified image
	public static Image getImage(String name) {
		if (!imageMap.containsKey(name)) throw new IllegalArgumentException(String.format("Image name not found: %s", name));
		return imageMap.get(name);
	}
	
	public static Set<String> getImageNames() {
		return imageMap.keySet();
	}

}
