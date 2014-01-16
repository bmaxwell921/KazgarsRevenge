package main.kazgarsrevenge.ui.panels;

import java.awt.Image;

import javax.swing.ImageIcon;
import javax.swing.JLabel;
import javax.swing.JPanel;

/**
 * Panel for displaying the image for a single room and its name
 * @author Brandon
 *
 */
public class ImageDescriptionPanel extends JPanel {

	/**
	 * Version 1!
	 */
	private static final long serialVersionUID = 1L;

	// The image associated with this room
	private Image roomImage;
	
	// The name of this room
	private String roomName;
	
	public ImageDescriptionPanel(String roomName, Image roomImage) {
		this.roomName = roomName;
		this.roomImage = roomImage;
		initialize();
	}
	
	private void initialize() {
		JLabel imageButton = new JLabel(new ImageIcon(roomImage));
		this.add(imageButton);
		// TODO register an action listener
		
		JLabel roomNameLabel = new JLabel(roomName);
		this.add(roomNameLabel);
	}
}
