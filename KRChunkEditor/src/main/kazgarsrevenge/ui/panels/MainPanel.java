package main.kazgarsrevenge.ui.panels;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Image;
import java.awt.Rectangle;
import java.util.List;

import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.model.IRoom;
import main.kazgarsrevenge.model.chunks.impl.DefaultChunk;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.RoomFileUtil;

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
	
	// The bounding rectangle of the currently selected room, or a 20 x 20 square if nothing is selected
	private Rectangle selectedArea;
	
	// The image of the currently selected room
	private Image selectedImage;
	
	// The area of the grid that can currently be seen
	private Rectangle viewRectangle;
	
	public MainPanel(Rectangle viewRectangle) {
		currentChunk = new DefaultChunk(DEFAULT_NAME, new Location());
		this.setBackground(Color.WHITE);
		
		this.viewRectangle = viewRectangle;
		updateSelectedArea(); 
	}
	
	// Called when the user presses enter and has a room selected
	public void placeRoom() {
		if (selectedRoom == null) {
			System.out.println("No selected room");
			return;
		}
		currentChunk.addRoom((IRoom) selectedRoom.clone());
	}
	
	// Changes the selected area to the bounding rectangle of the selected room, or a 20x20 square
	public void updateSelectedArea() {
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
			Image roomImage = ImageLoader.getRoomImage(room.getRoomName());
			g2.drawImage(roomImage, roomLoc.getX() + viewRectangle.x, roomLoc.getY() + viewRectangle.y, null);
		}
	}
	
	// Paints the curent selection
	private void paintSelection(Graphics2D g2) {
		if (selectedRoom == null) {
			g2.setColor(Color.YELLOW);
			g2.fill(new Rectangle(selectedArea.x, selectedArea.y, ImageLoader.IMAGE_SIZE, ImageLoader.IMAGE_SIZE));
			return;
		}
		g2.drawImage(selectedImage, (int) (selectedArea.getX() + viewRectangle.x), (int) (selectedArea.getY() + viewRectangle.y), null);
	}
}
