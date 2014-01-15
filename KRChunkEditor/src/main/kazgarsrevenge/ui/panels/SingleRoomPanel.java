package main.kazgarsrevenge.ui.panels;

import java.awt.Image;

import javax.swing.JPanel;

/**
 * Panel for displaying the image for a single room and its name
 * @author Brandon
 *
 */
public class SingleRoomPanel extends JPanel {

	// The image associated with this room
	private Image roomImage;
	
	// The name of this room
	private String roomName;
	
	public SingleRoomPanel(String roomName, Image roomImage) {
		this.roomName = roomName;
		this.roomImage = roomImage;
		initialize();
	}
	
	private void initialize() {
		// TODO
	}
}
