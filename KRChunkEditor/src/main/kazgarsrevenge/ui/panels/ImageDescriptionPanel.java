package main.kazgarsrevenge.ui.panels;

import java.awt.Color;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.Image;

import javax.swing.BorderFactory;
import javax.swing.ImageIcon;
import javax.swing.JLabel;
import javax.swing.JPanel;

import main.kazgarsrevenge.util.IO.KRImageIO;

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
		this.setLayout(new FlowLayout(FlowLayout.LEFT));
		initialize();
	}
	
	private void initialize() {
		JLabel imageButton = new JLabel(new ImageIcon(roomImage));
		this.add(imageButton);
		
		JLabel roomNameLabel = new JLabel(roomName);
		this.add(roomNameLabel);
		
		this.setBorder(BorderFactory.createLineBorder(Color.BLACK));
		this.setCursor(Cursor.getPredefinedCursor(Cursor.HAND_CURSOR));
	}
}
