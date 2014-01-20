package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.JFileChooser;
import javax.swing.JOptionPane;

import main.kazgarsrevenge.model.impl.Chunk;
import main.kazgarsrevenge.ui.panels.KREditorPanel;
import main.kazgarsrevenge.util.JsonFolderFilter;
import main.kazgarsrevenge.util.IO.ChunkComponentIO;

public class LoadChunkClickListener extends MouseAdapter {

	// The parent frame to display a file chooser
	private final Frame parent;
	
	// The MainPanel to act on
	private final KREditorPanel editorPanel;
	
	public LoadChunkClickListener(Frame parent, KREditorPanel editorPanel) {
		this.parent = parent;
		this.editorPanel = editorPanel;
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
					StringBuilder errors = new StringBuilder();;
					
					editorPanel.load(jfc.getSelectedFile(), errors);
					
					if (errors.length() != 0) {
						JOptionPane.showMessageDialog(parent, errors.toString());
						return;
					}
				}
			}
		}.start();
	}

}
