package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.JFileChooser;
import javax.swing.JOptionPane;

import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.ui.panels.MainPanel;
import main.kazgarsrevenge.util.ChunkIO;
import main.kazgarsrevenge.util.JsonFolderFilter;

public class LoadChunkClickListener extends MouseAdapter {

	// The parent frame to display a file chooser
	private final Frame parent;
	
	// The MainPanel to act on
	private final MainPanel mp;
	
	public LoadChunkClickListener(Frame parent, MainPanel mp) {
		this.parent = parent;
		this.mp = mp;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		new Thread() {
			@Override
			public void run() {
				JFileChooser jfc = new JFileChooser(".");
				jfc.setFileFilter(new JsonFolderFilter());
				int result = jfc.showOpenDialog(parent);
				if (result == JFileChooser.APPROVE_OPTION) {
					StringBuilder errors = new StringBuilder();
					IChunk loaded = ChunkIO.loadChunk(jfc.getSelectedFile(), errors);
					if (loaded == null) {
						JOptionPane.showMessageDialog(parent, errors.toString());
						return;
					}
					mp.setCurrentChunk(loaded);
				}
			}
		}.start();
	}

}
