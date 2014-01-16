package main.kazgarsrevenge.ui;

import java.awt.BorderLayout;
import java.awt.Container;
import java.awt.EventQueue;

import javax.swing.BoxLayout;
import javax.swing.JFrame;
import javax.swing.JPanel;

import main.kazgarsrevenge.ui.panels.ImageDescriptionPanel;
import main.kazgarsrevenge.ui.panels.SidePanel;
import main.kazgarsrevenge.util.ImageLoader;

public class Main {
	
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				Main window = new Main();
			}
		});
	}
	
	public Main() {
		ImageLoader.loadImages();
		createAndShowGUI();
	}
	
	public void createAndShowGUI() {
		JFrame frame = new JFrame();
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setTitle("Kazgar's Revenge Chunk Editor");
		// Add panels
		
		setUpFrame(frame);
	}
	
	private void setUpFrame(JFrame frame) {
		Container content = frame.getContentPane();		
		content.setLayout(new BorderLayout());
		
		JPanel sidePanels = new JPanel();
		sidePanels.setLayout(new BoxLayout(sidePanels, BoxLayout.Y_AXIS));
		sidePanels.add(setUpRoomsPanel());
//		sidePanels.add(new SidePanel());
		
		content.add(sidePanels, BorderLayout.PAGE_END);
		
		frame.pack();
		frame.setVisible(true);
	}
	
	private SidePanel setUpRoomsPanel() {
		SidePanel roomsPanel = new SidePanel("Rooms");
		for (String name : ImageLoader.getImageNames()) {
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageLoader.getImage(name));
			roomsPanel.addPanel(imgDesc);
		}
		return roomsPanel;
	}
	
}
