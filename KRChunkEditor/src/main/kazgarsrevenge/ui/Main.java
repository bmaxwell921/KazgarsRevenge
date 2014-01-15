package main.kazgarsrevenge.ui;

import java.awt.EventQueue;

import javax.swing.JFrame;

public class Main {
	
	private JFrame frame;
	
	public static void main(String[] args) {
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				Main window = new Main();
			}
		});
	}
	
	public Main() {
		createAndShowGUI();
	}
	
	public void createAndShowGUI() {
		frame = new JFrame();
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.setTitle("Kazgar's Revenge Chunk Editor");
		// Add panels
		
		frame.pack();
		frame.setVisible(true);
	}
}
