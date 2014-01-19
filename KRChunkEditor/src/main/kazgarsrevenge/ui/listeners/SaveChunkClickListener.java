package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.io.File;

import javax.swing.JFileChooser;
import javax.swing.JOptionPane;

import main.kazgarsrevenge.ui.panels.MainPanel;
import main.kazgarsrevenge.util.ChunkIO;
import main.kazgarsrevenge.util.JsonFolderFilter;

public class SaveChunkClickListener extends MouseAdapter {

	// parent frame to show dialog in
	private final Frame parent;
	
	// main panel to act on
	private final MainPanel mp;
	
	public SaveChunkClickListener(Frame parent, MainPanel mp) {
		this.parent = parent;
		this.mp = mp;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		new Thread() {
			@Override
			public void run() {
				JFileChooser jfc = new JFileChooser(".");
				// only allow 1 json file
				jfc.setFileFilter(new JsonFolderFilter());
				jfc.setMultiSelectionEnabled(false);
				int result = jfc.showSaveDialog(parent);
				if (result == JFileChooser.APPROVE_OPTION) {
					StringBuilder errors = new StringBuilder();
					
					// If they didn't add a .json extension add it for them
					File selectedFile = jfc.getSelectedFile();
					if (!selectedFile.getName().endsWith(".json")) {
						selectedFile = new File(selectedFile + ".json");
					}
					
					ChunkIO.saveChunk(mp.getCurrentChunk(), selectedFile, errors);
					if (!errors.toString().isEmpty()) {
						JOptionPane.showMessageDialog(parent, errors.toString());
					}
				}
			}
		}.start();
	}
}
