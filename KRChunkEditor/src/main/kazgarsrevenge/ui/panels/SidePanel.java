package main.kazgarsrevenge.ui.panels;

import java.awt.Component;
import java.awt.ScrollPane;

import javax.swing.BoxLayout;
import javax.swing.JLabel;
import javax.swing.JPanel;

/**
 * Panel shown on the side of the screen. Made up of other panels
 * @author Brandon
 *
 */
public class SidePanel extends ScrollPane {

	/**
	 * Version 1!
	 */
	private static final long serialVersionUID = 1L;
	
	private JPanel container;
	
	public SidePanel(String name) {
		container = new JPanel();
		container.setLayout(new BoxLayout(container, BoxLayout.Y_AXIS));
		container.add(new JLabel(name));
		this.add(container);
	}
	
	// Used to add panels to this SidePanel
	public void addPanel(JPanel panel) {
		panel.setAlignmentX(Component.LEFT_ALIGNMENT);
		container.add(panel);
	}

}
