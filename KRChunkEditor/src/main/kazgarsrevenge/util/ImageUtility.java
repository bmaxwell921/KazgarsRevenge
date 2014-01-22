package main.kazgarsrevenge.util;

import java.awt.Graphics2D;
import java.awt.geom.AffineTransform;
import java.awt.image.AffineTransformOp;
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
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;
import main.kazgarsrevenge.util.managers.ImageManager;

public class ImageUtility {

	/**
	 * A method used to 'stitch' together images to create a new room. The images should all 
	 * be square and the same size, otherwise this won't work...that's why it's for making
	 * rooms. 
	 * 
	 * The images will be rotated and located according to each the RoomBlocks found in rooms
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
		BufferedImage stitched = new BufferedImage(totalWidth, totalHeight, BufferedImage.TYPE_INT_ARGB);
		Graphics2D g2 = stitched.createGraphics();
		
		for (BufferedImage key : rooms.keySet()) {
			for (RoomBlock room : rooms.get(key)) {
				BufferedImage rotated = ImageUtility.rotateSquareImage(key, room.getRotation());
				g2.drawImage(rotated, room.getLocation().getX() * imageSize, room.getLocation().getY() * imageSize, null);
			}
		}
		
		return stitched;
	}
	
	/**
	 * Creates a new bufferedImage for the given room
	 * @param room
	 * @param imageSize
	 * 				The size of a single RoomBlock image
	 * @return
	 */
	public static BufferedImage createImageFor(Room room, int imageSize) {
		Map<BufferedImage, List<RoomBlock>> rooms = new HashMap<>();
		List<RoomBlock> blocks = room.getComponents();
		
		// Create the map to send to stitch
		for (RoomBlock block : blocks) {
			BufferedImage baseImage = ImageManager.getInstance().getImage(RoomBlock.class, block.getName());
			List<RoomBlock> associatedRooms = rooms.get(baseImage);
			if (associatedRooms == null) {
				associatedRooms = new ArrayList<>();
			}
			associatedRooms.add(block);
			rooms.put(baseImage, associatedRooms);
		}
		
		return ImageUtility.stitchRoomBlocksTogether(rooms, imageSize, room.getHeight(), room.getWidth());
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
		BufferedImage rotated = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
		
		Graphics2D g2 = rotated.createGraphics();
		AffineTransform at = new AffineTransform();
		at.translate(image.getWidth() / 2, image.getHeight() / 2);
		// Do the negative because AffineTransform rotates backwards...
		at.rotate(-counterClockRotation.getRadians());
		at.translate(-image.getWidth() / 2, -image.getHeight() / 2);
		
		g2.drawImage(image, at, null);
		
		return rotated;
	}
	
	/**
	 * Returns a new image representing the scaled version of the given image
	 * @param img
	 * @param scale
	 * @return
	 */
	public static BufferedImage scaleImage(BufferedImage img, double scale) {
		AffineTransform at = new AffineTransform();
		at.scale(scale, scale);
		AffineTransformOp scaleOp = new AffineTransformOp(at, AffineTransformOp.TYPE_BILINEAR);
		return scaleOp.filter(img, null);
	}
}
