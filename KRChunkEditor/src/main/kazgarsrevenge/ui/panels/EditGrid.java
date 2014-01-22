package main.kazgarsrevenge.ui.panels;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.image.BufferedImage;
import java.util.List;

import javax.swing.BorderFactory;
import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.Locatable;
import main.kazgarsrevenge.util.ImageUtility;
import main.kazgarsrevenge.util.IO.KRImageIO;
import main.kazgarsrevenge.util.managers.ImageManager;

/**
 * The gird shown while editing an object
 * 
 * @author Brandon
 * 
 */
public class EditGrid extends JPanel {

	/**
	 * Version 1!
	 */
	private static final long serialVersionUID = 1;

	private final int SQUARE_SIZE;
	
	// The parent for this EditGrid
	private final KREditorPanel parent;
	
	// The current area that is selected, or a 1x1 block
	private Rectangle selectedArea;

	// The gridWidth of the grid
	private int gridWidth;

	// The gridHeight of the grid
	private int gridHeight;

	public EditGrid(KREditorPanel parent, int squareSize, int gridWidth, int gridHeight) {
		SQUARE_SIZE = squareSize;
		this.parent = parent; 
		this.gridWidth = gridWidth;
		this.gridHeight = gridHeight;
		this.selectedArea = new Rectangle(0, 0, SQUARE_SIZE, SQUARE_SIZE);
		
		this.setBorder(BorderFactory.createLineBorder(Color.GREEN));
	}

	@Override
	public void paintComponent(Graphics g) {
		super.paintComponent(g);
		Graphics2D g2 = (Graphics2D) g;
		paintEditable(g2);
		paintGrid(g2);
		paintSelection(g2);
	}

	private void paintGrid(Graphics2D g2) {
		g2.setColor(Color.BLACK);

		for (int i = 0; i < gridWidth; ++i) {
			for (int j = 0; j < gridHeight; ++j) {
				Rectangle drawn = new Rectangle(i * SQUARE_SIZE, j
						* SQUARE_SIZE, SQUARE_SIZE, SQUARE_SIZE);
				g2.draw(drawn);
			}
		}
	}
	
	// Paints the item that is currently being edited
	private void paintEditable(Graphics2D g2) {
		List<? extends ChunkComponent> components = parent.editing.getComponents();              
        for (ChunkComponent comp: components) {
                Location roomLoc = comp.getLocation();
                BufferedImage roomImage = ImageManager.getInstance().getImage(comp.getClass(), comp.getName());
                BufferedImage rotated = ImageUtility.rotateImage(roomImage, comp.getRotation());
                g2.drawImage(rotated, roomLoc.getX() * SQUARE_SIZE, roomLoc.getY() * SQUARE_SIZE, null);
        }
	}
	
	private void paintSelection(Graphics2D g2) {
		g2.setColor(Color.YELLOW);
		
		if (parent.hasSelection()) {
			g2.fill(selectedArea);
			g2.drawImage(parent.getSelectedImage(), selectedArea.x, selectedArea.y, null);
		} else {
			g2.fill(selectedArea);
		}
	}
	
	public void moveSelection(Location moveAmt) {
		Location reqMove = new Location(moveAmt.getX() + selectedArea.x, moveAmt.getY() + selectedArea.y);
		fixBounds(reqMove);
		selectedArea.setLocation(reqMove.getX(), reqMove.getY());
	}
	
	private void fixBounds(Location loc) {
		if (loc.getX() < 0) {
			loc.setX(0);
		}
		if (loc.getX() + SQUARE_SIZE > gridWidth * SQUARE_SIZE) {
			loc.setX(gridWidth * SQUARE_SIZE - SQUARE_SIZE);
		}
		
		if (loc.getY() < 0) {
			loc.setY(0);
		}
		if (loc.getY() + SQUARE_SIZE > gridHeight * SQUARE_SIZE) {
			loc.setY(gridHeight * SQUARE_SIZE - SQUARE_SIZE); 
		}
	}
	
	public void reset(int gridWidth, int gridHeight) {
		this.gridWidth = gridWidth;
		this.gridHeight = gridHeight;
		setSelectedArea(0, 0, SQUARE_SIZE, SQUARE_SIZE);
	}
	
	public void resetSelectedArea(int x, int y) {
		setSelectedArea(x, y, SQUARE_SIZE, SQUARE_SIZE);
	}
	
	public void changedSelected(int newWidth, int newHeight) {
		setSelectedArea(selectedArea.x, selectedArea.y, newWidth, newHeight);
	}
	
	public void setSelectedArea(int x, int y, int width, int height) {
		selectedArea.setBounds(x, y, width, height);
	}
	
	/**
	 * Sets the given item's location to the current selected location
	 * @param item
	 */
	public void setAtCurrentLocation(Locatable item) {
		Location correct = new Location(selectedArea.x / SQUARE_SIZE, selectedArea.y / SQUARE_SIZE);
		item.setLocation(correct);
	}
	
	public void rotateSelectedArea() {
		// TODO
	}
}
