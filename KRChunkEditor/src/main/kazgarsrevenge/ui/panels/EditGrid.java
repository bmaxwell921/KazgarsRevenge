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
import main.kazgarsrevenge.util.ImageLoader;

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
		paintGrid(g2);
		paintSelection(g2);
		paintEditable(g2);
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
	
	private void paintEditable(Graphics2D g2) {
		List<? extends ChunkComponent> components = parent.editing.getComponents();              
        for (ChunkComponent comp: components) {
                Location roomLoc = comp.getLocation();
                BufferedImage roomImage = ImageLoader.getImage(comp.getClass(), comp.getName());
                g2.drawImage(roomImage, roomLoc.getX() * SQUARE_SIZE, roomLoc.getY() * SQUARE_SIZE, null);
        }
	}
	
	private void paintSelection(Graphics2D g2) {
		g2.setColor(Color.YELLOW);
		
		if (parent.hasSelection()) {
			selectedArea.setBounds(parent.selectedItem.getLocation().getX(), parent.selectedItem.getLocation().getY(), 
					parent.selectedItem.getWidth(), parent.selectedItem.getHeight());
			g2.fill(selectedArea);
			g2.drawImage(parent.getSelectedImage(), selectedArea.x, selectedArea.y, null);
		} else {
			g2.fill(selectedArea);
		}
	}
	
	public void moveSelection(Location moveAmt) {
		selectedArea.translate(moveAmt.getX(), moveAmt.getY());
	}
	
	public void reset(int gridWidth, int gridHeight) {
		this.gridWidth = gridWidth;
		this.gridHeight = gridHeight;
		resetSelectedArea(0, 0);
	}
	
	public void resetSelectedArea(int x, int y) {
		selectedArea.setBounds(x, y, SQUARE_SIZE, SQUARE_SIZE);
	}
	
	public Location getSelectedLocation() {
		return new Location(selectedArea.x, selectedArea.y);
	}
	
	public void rotateSelectedArea() {
		// TODO
	}
}
