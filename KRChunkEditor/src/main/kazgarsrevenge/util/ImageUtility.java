package main.kazgarsrevenge.util;

import java.awt.Graphics2D;
import java.awt.geom.AffineTransform;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.imageio.ImageIO;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.impl.RoomBlock;

public class ImageUtility {

	/**
	 * A method used to 'stitch' together images to create a new room. The images should all 
	 * be square and the same size, otherwise this won't work...that's why it's for making
	 * rooms. 
	 * 
	 * 	[0][1]
	 * 	[2][3]
	 * The resulting image will be:
	 * 	[0 1]
	 * 	[2 3]
	 * All in one image
	 * @param rooms
	 * 				Mapping from BufferedImage to a list of the RoomBlocks associated with that image. The RoomBlocks
	 * 				are also used to know where to draw the image
	 * @param rows
	 * @param cols
	 * @return
	 */
	public static BufferedImage stitchRoomBlocksTogether(Map<BufferedImage, List<RoomBlock>> rooms, int imageSize, int rows, int cols) {
		int totalWidth = imageSize * cols;
		int totalHeight = imageSize * rows;
		
		// The image to return
		BufferedImage stitched = new BufferedImage(totalWidth, totalHeight, BufferedImage.TYPE_INT_RGB);
		Graphics2D g2 = stitched.createGraphics();
		
		for (BufferedImage key : rooms.keySet()) {
			for (RoomBlock room : rooms.get(key)) {
				g2.drawImage(key, room.getLocation().getX() * imageSize, room.getLocation().getY() * imageSize, null);
			}
		}
		
		return stitched;
	}
	
	/**
	 * Returns a new Image that is a copy of the given image, rotated by the given amount, in the counter clockwise
	 * direction.
	 * The given image must be a square for this to work
	 * @param image
	 * @param counterClockRotation
	 * @return
	 */
	public static BufferedImage rotateSquareImage(BufferedImage image, Rotation counterClockRotation) {
		BufferedImage rotated = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_RGB);
		
		Graphics2D g2 = rotated.createGraphics();
		AffineTransform at = new AffineTransform();
		at.translate(image.getWidth() / 2, image.getHeight() / 2);
		// Do the negative because AffineTransform rotates backwards...
		at.rotate(-counterClockRotation.getRadians());
		at.translate(-image.getWidth() / 2, -image.getHeight() / 2);
		
		g2.drawImage(image, at, null);
		
		return rotated;
	}
	
	public static void main(String[] args) throws IOException {
		ImageLoader.loadImages();
		final BufferedImage player = ImageLoader.getBlockImage("playerSpawn");
		final RoomBlock playerBlock = new RoomBlock();
		
		final BufferedImage door = ImageLoader.getBlockImage("door");
		final BufferedImage rotatedDoor = ImageUtility.rotateSquareImage(door, Rotation.NINETY);
		final RoomBlock doorBlock = new RoomBlock(new Location(0, 1), "door", Rotation.NINETY);
		
//		Map<BufferedImage, List<RoomBlock>> rooms = new HashMap<>();
//		rooms.put(player, new ArrayList<RoomBlock>() {{
//			add(playerBlock);
//		}});
//		rooms.put(rotatedDoor, new ArrayList<RoomBlock>() {{
//			add(doorBlock);
//		}});
//		
//		BufferedImage result = ImageUtility.stitchRoomBlocksTogether(rooms, 25, 2, 1);
		ImageIO.write(rotatedDoor, "png", new File(".\\img\\rooms\\test.png"));
	}
}
