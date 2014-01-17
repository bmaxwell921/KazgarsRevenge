package main.kazgarsrevenge.ui;

import java.awt.BorderLayout;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.GridLayout;
import java.awt.Rectangle;
import java.awt.event.KeyEvent;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.swing.InputMap;
import javax.swing.JComponent;
import javax.swing.JFrame;
import javax.swing.JPanel;
import javax.swing.KeyStroke;
import javax.swing.Timer;

import main.kazgarsrevenge.ui.keybinds.MoveAction;
import main.kazgarsrevenge.ui.listeners.UpdateListener;
import main.kazgarsrevenge.ui.panels.ImageDescriptionPanel;
import main.kazgarsrevenge.ui.panels.MainPanel;
import main.kazgarsrevenge.ui.panels.SidePanel;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.RoomUtil;

public class Main {
	
	private MainPanel mainPanel;
	
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				Main window = new Main();
			}
		});
	}
	
	public Main() {
		ImageLoader.loadImages();
		RoomUtil.readKnownRooms();
		createAndShowGUI();
	}
	
	public MainPanel getMainPanel() {
		return mainPanel;
	}
	
	public void createAndShowGUI() {
		JFrame frame = new JFrame();
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setTitle("Kazgar's Revenge Chunk Editor");
		frame.setFocusable(true);
		setUpFrame(frame);
		registerKeyBindings();
		setUpTimer();
	}
	
	private void setUpFrame(JFrame frame) {
		Container content = frame.getContentPane();		
		content.setLayout(new BorderLayout());
		
		mainPanel = new MainPanel(new Rectangle(0, 0, 500, 360));
		mainPanel.setPreferredSize(new Dimension(500, 360));
		content.add(mainPanel, BorderLayout.CENTER);
		
		JPanel sidePanels = new JPanel();
		sidePanels.setPreferredSize(new Dimension(200, 200));
		sidePanels.setLayout(new GridLayout(2, 1));
		
		sidePanels.add(setUpRoomsPanel());
		sidePanels.add(setUpBlocksPanel());
		
		content.add(sidePanels, BorderLayout.EAST);
		
		frame.getContentPane().setPreferredSize(new Dimension(700, 400));
		frame.setResizable(false);
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
			roomsPanel.addPanel(imgDesc);
		}
		return roomsPanel;
	}
	
	private void registerKeyBindings() {
		registerMovementBinding("LEFT", KeyEvent.VK_LEFT);
		registerMovementBinding("RIGHT", KeyEvent.VK_RIGHT);
		registerMovementBinding("UP", KeyEvent.VK_UP);
		registerMovementBinding("DOWN", KeyEvent.VK_DOWN);
	}
	
	// Handles multiple key inputs
	private void registerMovementBinding(String name, int keyCode) {
		MoveAction pressed = new MoveAction(keyCode, true);
		MoveAction released = new MoveAction(keyCode, false);
		
		String pressedCommand = name + " Pressed";
		String releasedCommand = name + " Released";
		
		InputMap inputMap = mainPanel.getInputMap(JComponent.WHEN_IN_FOCUSED_WINDOW);
		
		KeyStroke pressedKeyStroke = KeyStroke.getKeyStroke(keyCode, 0, false);
		inputMap.put(pressedKeyStroke, pressedCommand);
		mainPanel.getActionMap().put(pressedCommand, pressed);
		
		KeyStroke releasedKeyStroke = KeyStroke.getKeyStroke(keyCode, 0, true);
		inputMap.put(releasedKeyStroke, releasedCommand);
		mainPanel.getActionMap().put(releasedCommand, released);
	}
	
	private void setUpTimer() {
		Timer timer = new Timer(30, new UpdateListener(mainPanel));
		timer.start();
	}
	
}
