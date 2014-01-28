package main.kazgarsrevenge.ui;

import java.awt.EventQueue;
import java.io.File;

import javax.swing.JFrame;

import main.kazgarsrevenge.ui.panels.ChunkEditorPanel;
import main.kazgarsrevenge.util.IO.ComponentIO;
import main.kazgarsrevenge.util.IO.KRImageIO;
import main.kazgarsrevenge.util.managers.ComponentManager;
import main.kazgarsrevenge.util.managers.ImageManager;

public class Main {

	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				new Main();
			}
		});
	}
	
	public Main() {
		// load
		createFolders();
		ImageManager.getInstance();
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
	
	private void createFolders() {
		createFolder(KRImageIO.BASE_BLOCK_FILE_PATH);
		createFolder(KRImageIO.BASE_ROOM_FILE_PATH);
		createFolder(ComponentIO.DEFAULT_BLOCKS_PATH);
		createFolder(ComponentIO.DEFAULT_ROOMS_PATH);
	}
	
	private static void createFolder(String filePath) {
		File file = new File(filePath);
		if (!file.exists()) {
			file.mkdirs();
		}
	}
}
