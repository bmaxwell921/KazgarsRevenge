package main.kazgarsrevenge.ui.panels;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.geom.AffineTransform;
import java.awt.image.AffineTransformOp;
import java.awt.image.BufferedImage;
import java.util.List;

import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.model.IRoom;
import main.kazgarsrevenge.model.chunks.impl.DefaultChunk;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;
import main.kazgarsrevenge.util.ImageLoader;

/**
 * UI that actually shows what the user is building
 * @author bmaxwell
 *
 */
public class MainPanel extends JPanel {
	/**
	 * Version 1!
	 */
	private static final long serialVersionUID = 1L;
	
	private static final String DEFAULT_NAME = "Chunk Name";

	// The chunk the user is building
	private IChunk currentChunk;
	
	// The room the user currently has selected
	private DefaultRoom selectedRoom;
	
	// Current rotation of the selected Room
	private Rotation currentRot;
	
	// The bounding rectangle of the currently selected room, or a 20 x 20 square if nothing is selected
	private Rectangle selectedArea;
	
	// The image of the currently selected room
	private BufferedImage selectedImage;
	
	// The area of the grid that can currently be seen
	private Rectangle viewRectangle;
	
	public MainPanel(Rectangle viewRectangle) {
		currentChunk = new DefaultChunk(DEFAULT_NAME, new Location());
		this.setBackground(Color.WHITE);
		
		this.viewRectangle = viewRectangle;
		currentRot = Rotation.ZERO;
		updateSelectedArea(); 
		
		
	}
	
	// Called when the user presses enter and has a room selected
	public void placeRoom() {
		if (selectedRoom == null) {
			System.out.println("No selected room");
			return;
		}
		IRoom selCopy = (IRoom) selectedRoom.clone();
		// Make sure to put the room at the right location
		selCopy.setLocation(new Location((int) selectedArea.getX(), (int) selectedArea.getY()));
		snapToClosest(selCopy);
	}
	
	// Since the selection can move in between cells we need to 'snap' to a grid location
	private void snapToClosest(IRoom selCopy) {
		Location selLoc = selCopy.getLocation();
		
		// If the selectedCopy is intersecting 2 cells, this gets the x location of the leftmost cell
		int leftCellX = (selLoc.getX() / ImageLoader.IMAGE_SIZE) * ImageLoader.IMAGE_SIZE;
		int rightCellX = leftCellX + ImageLoader.IMAGE_SIZE;
		
		int leftXDiff = leftCellX - selLoc.getX();
		int rightXDiff = rightCellX - selLoc.getX();
		
		int topCellY = (selLoc.getY() / ImageLoader.IMAGE_SIZE) * ImageLoader.IMAGE_SIZE;
		int bottomCellY = topCellY + ImageLoader.IMAGE_SIZE;
		
		int topYDiff = topCellY - selLoc.getY();
		int bottomYDiff = bottomCellY - selLoc.getY();
		
		Location correction = new Location();
		correction.setX(Math.abs(leftXDiff) < Math.abs(rightXDiff) ? leftXDiff : rightXDiff);
		correction.setY(Math.abs(topYDiff) < Math.abs(bottomYDiff) ? topYDiff : bottomYDiff);
		
		selCopy.setLocation(correction);
		currentChunk.addRoom(selCopy);
	}
	
	/**
	 *  Sets the selected room as given and updates the selected area
	 * @param room
	 */
	public void setSelectedRoom(DefaultRoom room) {
		this.selectedRoom = room;
		updateSelectedArea();
	}
	
	/**
	 * Clears which room has been selected
	 */
	public void clearSelectedRoom() {
		this.selectedRoom = null;
		this.currentRot = Rotation.ZERO;
		updateSelectedArea();
	}
	
	/**
	 * Changes the selected area to the bounding rectangle of the selected room, or a 20x20 square
	 */
	public void updateSelectedArea() {
		// TODO handle rotation properly
		if (selectedRoom == null) {
			selectedArea = new Rectangle(0, 0, ImageLoader.IMAGE_SIZE, ImageLoader.IMAGE_SIZE);
			return;
		}
		Rectangle bound = selectedRoom.getBoundingRect();
		selectedArea.width = bound.width * ImageLoader.IMAGE_SIZE;
		selectedArea.height = bound.height * ImageLoader.IMAGE_SIZE;
	}
	
	@Override 
	public void paintComponent(Graphics g) {
		super.paintComponent(g);
		Graphics2D g2 = (Graphics2D) g;
		paintGrid(g2);
		paintChunk(g2);
		paintSelection(g2);
	}
	
	// Paints the grid on screen
	private void paintGrid(Graphics2D g2) {
		int rightBound = viewRectangle.x + viewRectangle.width;
		int lowerBound = viewRectangle.y + viewRectangle.height;
		
		g2.setColor(Color.GRAY);
		for (int i = 0; (i * ImageLoader.IMAGE_SIZE) < rightBound; ++i) {
			for (int j = 0; (j * ImageLoader.IMAGE_SIZE) < lowerBound; ++j) {
				g2.draw(new Rectangle(i * ImageLoader.IMAGE_SIZE, j * ImageLoader.IMAGE_SIZE, 
						ImageLoader.IMAGE_SIZE, ImageLoader.IMAGE_SIZE));
			}
		}
	}
	
	// Paints the entire chunk on screen
	private void paintChunk(Graphics2D g2) {
		List<IRoom> rooms = currentChunk.getRooms();		
		for (IRoom room : rooms) {
			Location roomLoc = room.getLocation();
			BufferedImage roomImage = (BufferedImage) ImageLoader.getRoomImage(room.getRoomName());
			drawRotatedImage(g2, roomImage, new Location(roomLoc.getX() + viewRectangle.x, roomLoc.getY() + viewRectangle.y), null);
		}
	}
	
	// Paints the curent selection
	private void paintSelection(Graphics2D g2) {
		if (selectedRoom == null) {
			g2.setColor(Color.YELLOW);
			g2.fill(new Rectangle(selectedArea.x, selectedArea.y, ImageLoader.IMAGE_SIZE, ImageLoader.IMAGE_SIZE));
			return;
		}
		drawRotatedImage(g2, selectedImage, 
				new Location((int) (selectedArea.getX() + viewRectangle.x), (int) (selectedArea.getY() + viewRectangle.y)), 
				currentRot);
	}
	
	private void drawRotatedImage(Graphics2D g2, BufferedImage img, Location loc, Rotation rot) {
		AffineTransform tx = AffineTransform.getRotateInstance(Rotation.toRadians(rot), loc.getX(), loc.getY());
		AffineTransformOp op = new AffineTransformOp(tx, AffineTransformOp.TYPE_BILINEAR);
		g2.drawImage(op.filter(img, null), loc.getX(), loc.getY(), null);
	}
	
	public void moveSelection(Location direction) {
		selectedArea.x += direction.getX();
		selectedArea.y += direction.getY();
	}
}
