package main.kazgarsrevenge.ui;

import java.awt.BorderLayout;
import java.awt.Component;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.GridLayout;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.swing.BoxLayout;
import javax.swing.JFrame;
import javax.swing.JPanel;

import main.kazgarsrevenge.ui.panels.ImageDescriptionPanel;
import main.kazgarsrevenge.ui.panels.SidePanel;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.RoomFileUtil;

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
		RoomFileUtil.readKnownRooms();
		createAndShowGUI();
	}
	
	public void createAndShowGUI() {
		JFrame frame = new JFrame();
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setTitle("Kazgar's Revenge Chunk Editor");
		
		setUpFrame(frame);
	}
	
	private void setUpFrame(JFrame frame) {
		Container content = frame.getContentPane();		
		content.setLayout(new BorderLayout());
		
		JPanel sidePanels = new JPanel();
		sidePanels.setPreferredSize(new Dimension(200, 200));
		sidePanels.setLayout(new GridLayout(2, 1));
		
		sidePanels.add(setUpRoomsPanel());
		sidePanels.add(setUpBlocksPanel());
		
		content.add(sidePanels, BorderLayout.EAST);
		
		frame.getContentPane().setPreferredSize(new Dimension(700, 400));
		frame.pack();
		frame.setVisible(true);
	}
	
	private SidePanel setUpRoomsPanel() {
		SidePanel roomsPanel = new SidePanel("Rooms");
		List<String> names = new ArrayList<>(ImageLoader.getRoomImageNames());
		Collections.sort(names);
		for (String name : names) {
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageLoader.getRoomImage(name));
			roomsPanel.addPanel(imgDesc);
		}
		return roomsPanel;
	}
	
	private SidePanel setUpBlocksPanel() {
		SidePanel roomsPanel = new SidePanel("Blocks");
		List<String> names = new ArrayList<>(ImageLoader.getBlockImageNames());
		Collections.sort(names);
		for (String name : names){
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageLoader.getBlockImage(name));
//			imgDesc.setAlignmentX(Component.LEFT_ALIGNMENT);
			roomsPanel.addPanel(imgDesc);
		}
		return roomsPanel;
	}
	
}
