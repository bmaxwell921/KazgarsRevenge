package main.kazgarsrevenge.ui;

import java.awt.EventQueue;

import javax.swing.JFrame;

import main.kazgarsrevenge.ui.panels.ChunkEditorPanel;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.managers.ComponentManager;

public class Main {

	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				new Main();
			}
		});
	}
	
	public Main() {
		ImageLoader.loadImages();
		// load
		ComponentManager.getInstance();
		createAndShowGUI();
	}
	
	public void createAndShowGUI() {
		JFrame frame = new JFrame("Kazgar's Revenge Chunk Editor");
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setFocusable(true);
		frame.setResizable(false);
		frame.getContentPane().add(new ChunkEditorPanel(frame, 800, 650));
		
		frame.pack();
		frame.setVisible(true);
	}

}
